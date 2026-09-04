using DocumentFormat.OpenXml.Presentation;
using MsWeb.Databases.Commondbases;
using MsWeb.Databases.Paydbases;
using MsWeb.Sharedfunctions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;
using static MsWeb.Sharedfunctions.Msg;

namespace MsWeb.Report.Formats
{
    public class EmployeePdfFormat : IDocument
    {
        private readonly List<EmployeesDashboard> _employees;
        private readonly List<ColumnDefinition> _columns;
        public readonly string _header;
        public readonly string _compName;
        public readonly string _dbaseName;
        public readonly string _reportBy;
        public readonly string? _prdDsc;

        public EmployeePdfFormat(List<EmployeesDashboard> employees, List<ColumnDefinition> columns, string header, string? compName, string? dbaseName, string? reportBy, string? prdDsc)
        {
            _employees = employees ?? new List<EmployeesDashboard>();
            _columns = columns ?? new List<ColumnDefinition>();
            _header = header;
            _compName = compName ?? "";
            _dbaseName = dbaseName ?? "";
            _reportBy = reportBy ?? "";
            _prdDsc = prdDsc;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Column(col =>
                {
                    col.Item().Element(ComposeBrandHeader);
                    col.Item().Element(ComposeReportHeader);
                });

                page.Content().Element(ComposeTable);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }

        void ComposeBrandHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(_compName).Bold();
                row.RelativeItem().AlignCenter().Text(_dbaseName).SemiBold();
                row.RelativeItem().AlignRight().Text($"Printed at: {DateTime.UtcNow}");
            });
        }

        void ComposeReportHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(_reportBy);
                row.RelativeItem().AlignCenter().Text(_header).SemiBold();
                row.RelativeItem().AlignRight().PaddingBottom(10).Text(_prdDsc);
                //row.RelativeItem().PaddingBottom(10);
            });
        }

        void ComposeTable(IContainer container)
        {
            //Safety Check
            if (!_columns.Any())
            {
                container.Text("NO COLUMN SELECTED!");
                return;
            }

            if (_reportBy == "By Analysis")
            {
                //Grouping By Analysis
                var groups = _employees
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.andsc) ? "Uncategorized" : e.andsc)
                    .OrderBy(g => g.Key);



                container.Column(mainCol =>
                {
                    foreach (var group in groups)
                    {
                        //SUBHEADING

                        mainCol.Item()
                            .AlignCenter()
                            .PaddingTop(10)
                            .PaddingBottom(5)
                            .Text($"{_employees.tablename} - {group.Key}")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);
                            
                        //Grouping By Referencies
                        var refs = group
                            .GroupBy(e => string.IsNullOrWhiteSpace(e.refdsc) ? "Uncategorized" : e.refdsc)
                            .OrderBy(g => g.Key);

                        var list = refs.ToList();
                        var refPresent = list.Key;

                        if (refPresent != "Uncategorized")
                        {
                            foreach (var item in refs)
                            {
                                mainCol.Item()
                                    .AlignLeft()
                                    .PaddingTop(3)
                                    .PaddingBottom(5)
                                    .Text($"{item.Key}")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                //TABLE PER ANALYSIS
                                mainCol.Item().Table(table =>
                                {
                                    //COLUMNS
                                    table.ColumnsDefinition(cols =>
                                    {
                                        foreach (var _ in _columns)
                                        {
                                            cols.RelativeColumn();
                                        }
                                    });

                                    //HEADER AUTO REPEATS
                                    table.Header(header =>
                                    {
                                        foreach (var col in _columns)
                                        {
                                            var title = string.IsNullOrWhiteSpace(col.Title)
                                                ? col.Key
                                                : col.Title;

                                            header.Cell()
                                                .Element(HeaderStyle)
                                                .Text(title)
                                                .Bold();
                                        }
                                    });

                                    //DATA ROWS
                                    int rowIndex = 0;

                                    foreach (var emp in item)
                                    {
                                        var background = rowIndex % 2 == 0
                                            ? Colors.Grey.Lighten3
                                            : Colors.White;

                                        foreach (var col in _columns)
                                        {
                                            var property = emp.GetType().GetProperty(
                                                col.Key,
                                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                                            var value = property?.GetValue(emp);

                                            table.Cell()
                                                .Element(c => DataCell(c, background))
                                                .Text(FormatValue(value));
                                        }

                                        rowIndex++;
                                    }
                                });
                            }
                        }
                        else{
                            //TABLE PER ANALYSIS
                            mainCol.Item().Table(table =>
                            {
                                //COLUMNS
                                table.ColumnsDefinition(cols =>
                                {
                                    foreach (var _ in _columns)
                                    {
                                        cols.RelativeColumn();
                                    }
                                });

                                //HEADER AUTO REPEATS
                                table.Header(header =>
                                {
                                    foreach (var col in _columns)
                                    {
                                        var title = string.IsNullOrWhiteSpace(col.Title)
                                            ? col.Key
                                            : col.Title;

                                        header.Cell()
                                            .Element(HeaderStyle)
                                            .Text(title)
                                            .Bold();
                                    }
                                });

                                //DATA ROWS
                                int rowIndex = 0;

                                foreach (var emp in group)
                                {
                                    var background = rowIndex % 2 == 0
                                        ? Colors.Grey.Lighten3
                                        : Colors.White;

                                    foreach (var col in _columns)
                                    {
                                        var property = emp.GetType().GetProperty(
                                            col.Key,
                                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                                        var value = property?.GetValue(emp);

                                        table.Cell()
                                            .Element(c => DataCell(c, background))
                                            .Text(FormatValue(value));
                                    }

                                    rowIndex++;
                                }
                            });
                        }
                    }
                });
            }else if (_reportBy == "By References")
            {
                // Grouping By References
                var groups = _employees
                    .GroupBy(e => string.IsNullOrWhiteSpace(e.refdsc) ? "Uncategorized" : e.refdsc)
                    .OrderBy(g => g.Key);

                container.Column(mainCol =>
                {
                    foreach (var group in groups)
                    {
                        // SUBHEADING
                        mainCol.Item()
                            .AlignCenter()
                            .PaddingTop(10)
                            .PaddingBottom(5)
                            .Text($"{_employees.tablename} - {group.Key}")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Blue.Medium);

                        var refs = _employees
                            .GroupBy(e => string.IsNullOrWhiteSpace(e.refdsc) ? "Uncategorized" : e.refdsc)
                            .OrderBy(g => g.Key);

                        var list = refs.ToList();
                        var refPresent = list.Key;

                        if (refPresent != "Uncategorized")
                        {
                            foreach (var item in refs)
                            {
                                mainCol.Item()
                                    .AlignLeft()
                                    .PaddingTop(3)
                                    .PaddingBottom(5)
                                    .Text($"{item.Key}")
                                    .FontSize(12)
                                    .Bold()
                                    .FontColor(Colors.Blue.Darken2);

                                // TABLE PER ANALYSIS
                                mainCol.Item().Table(table =>
                                {
                                    // COLUMNS
                                    table.ColumnsDefinition(cols =>
                                    {
                                        foreach (var _ in _columns)
                                        {
                                            cols.RelativeColumn();
                                        }
                                    });

                                    // HEADER AUTO REPEATS
                                    table.Header(header =>
                                    {
                                        foreach (var col in _columns)
                                        {
                                            var title = string.IsNullOrWhiteSpace(col.Title)
                                                ? col.Key
                                                : col.Title;

                                            header.Cell()
                                                .Element(HeaderStyle)
                                                .Text(title)
                                                .Bold();
                                        }
                                    });

                                    // DATA ROWS
                                    int rowIndex = 0;

                                    foreach (var emp in item)
                                    {
                                        var background = rowIndex % 2 == 0
                                            ? Colors.Grey.Lighten3
                                            : Colors.White;

                                        foreach (var col in _columns)
                                        {
                                            var property = emp.GetType().GetProperty(
                                                col.Key,
                                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                                            var value = property?.GetValue(emp);

                                            table.Cell()
                                                .Element(c => DataCell(c, background))
                                                .Text(FormatValue(value));
                                        }

                                        rowIndex++;
                                    }
                                });
                            }
                        }
                        else
                        {
                            mainCol.Item().Table(table =>
                            {
                                // COLUMNS
                                table.ColumnsDefinition(cols =>
                                {
                                    foreach (var _ in _columns)
                                    {
                                        cols.RelativeColumn();
                                    }
                                });

                                // HEADER AUTO REPEATS
                                table.Header(header =>
                                {
                                    foreach (var col in _columns)
                                    {
                                        var title = string.IsNullOrWhiteSpace(col.Title)
                                            ? col.Key
                                            : col.Title;

                                        header.Cell()
                                            .Element(HeaderStyle)
                                            .Text(title)
                                            .Bold();
                                    }
                                });

                                foreach (var emp in _employees)
                                {
                                    foreach (var col in _columns)
                                    {
                                        var property = emp.GetType().GetProperty(
                                            col.Key,
                                            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                                        var value = property?.GetValue(emp);

                                        table.Cell()
                                            .Element(c => DataCell(c))
                                            .Text(FormatValue(value));
                                    }
                                }
                            });
                        }
                    }
                });
            }
            else
            {
                container.Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        foreach (var _ in _columns)
                        {
                            cols.RelativeColumn();
                        }
                    });

                    table.Header(header =>
                    {
                        foreach (var col in _columns)
                        {
                            var title = string.IsNullOrWhiteSpace(col.Title)
                                ? col.Key
                                : col.Title;

                            header.Cell()
                                .Element(HeaderStyle)
                                .Text(title)
                                .Bold();
                        }
                    });

                    foreach (var emp in _employees)
                    {
                        foreach (var col in _columns)
                        {
                            var property = emp.GetType().GetProperty(
                                col.Key,
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                            var value = property?.GetValue(emp);

                            table.Cell()
                                .Element(c => DataCell(c))
                                .Text(FormatValue(value));
                        }
                    }
                });
            }
        }

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten2)
                .Padding(5);
        }

        static IContainer DataCell(IContainer container, string? background = null)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Background(background ?? Colors.White)
                .Padding(5);
        }

        static IContainer DataCell2(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten3)
                .Padding(5);
        }

        public string FormatValue(object value)
        {
            if (value == null) return " ";

            return value switch 
            {
                decimal d => d.ToString("N2"),
                DateTime dt => dt.ToString("dd MM yyyy"),
                _ => value.ToString()
                
            };
        }
    }
}

