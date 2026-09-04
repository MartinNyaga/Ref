using MsWeb.Databases.Commondbases;
using MsWeb.Databases.Paydbases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace MsWeb.Report.Formats
{
    public class EdPdfFormat : IDocument
    {
        private readonly List<EmptrsRpt> _entries;
        public readonly string _header;
        public readonly string? _compName;
        public readonly string? _dbaseName;
        public readonly string? _reportBy;
        public readonly string? _prdDsc;

        public EdPdfFormat(List<EmptrsRpt> entries, string header, string? compName, string? dbaseName, string? reportBy, string? prdDsc)
        {
            _entries = entries ?? new List<EmptrsRpt>();
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
                page.Size(PageSizes.A3.Landscape());
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

        void ComposeBrandHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(_compName).Bold();
                row.RelativeItem().AlignCenter().Text(_dbaseName).SemiBold();
                row.RelativeItem().AlignRight().Text($"Printed at: {DateTime.UtcNow}");
            });
        }

        void ComposeTable(IContainer container)
        {
            if (_reportBy == "By Employees")
            {
                // DYNAMIC ED COLUMNS
                var edsColumn = _entries
                    .Select(x => x.Eddsc)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                // GROUP EMPLOYEES
                var employees = _entries
                    .GroupBy(x => new
                    {
                        x.Empcode,
                        x.FullName
                    })
                    .OrderBy(x => x.Key.Empcode)
                    .ToList();

                container.Table(table =>
                {
                    // COLUMN DEFINITION
                    table.ColumnsDefinition(columns =>
                    {
                        // Employee Column
                        columns.ConstantColumn(100);
                        columns.ConstantColumn(150);

                        // DYNAMIC EDS COLUMNS
                        foreach (var eds in edsColumn)
                        {
                            columns.ConstantColumn(85);
                        }

                        // TOTAL COLUMNS
                        columns.ConstantColumn(80);
                    });

                    // HEADER
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderCellStyle).Text("EmpCode").Bold();
                        header.Cell().Element(HeaderCellStyle).Text("Employee").Bold();

                        foreach (var eds in edsColumn)
                        {
                            header.Cell()
                                .Element(HeaderCellStyle)
                                .AlignRight()
                                .Text(Truncate(eds, 12))
                                .Bold();
                        }

                        header.Cell()
                            .Element(HeaderCellStyle)
                            .AlignRight()
                            .Text("Total")
                            .Bold();
                    });

                    // Employee Rows
                    int rowIndex = 0;
                    foreach (var emp in employees)
                    {
                        var background = rowIndex % 2 == 0
                            ? Colors.Grey.Lighten3
                            : Colors.White;

                        table.Cell()
                            .Element(c => DataCellStyle(c, background))
                            .Text(emp.Key.Empcode);

                        table.Cell()
                            .Element(c => DataCellStyle(c, background))
                            .Text(emp.Key.FullName);

                        decimal rowTotals = 0;

                        // Dynamic Eds Value
                        foreach (var eds in edsColumn)
                        {
                            var amount = emp
                                .Where(x => x.Eddsc == eds)
                                .Sum(x => x.Amount);

                            rowTotals += Convert.ToDecimal(amount);

                            table.Cell()
                                .Element(c => DataCellStyle(c, background))
                                .AlignRight()
                                .Text(amount == 0 ? "" : amount.ToString("N2"));
                        }

                        // Row Total
                        table.Cell()
                            .Element(c => TotalCellStyle(c, background))
                            .AlignRight()
                            .Text(rowTotals.ToString("N2"))
                            .Bold();

                        rowIndex++;
                    }

                    // GRAND TOTAL ROW
                    table.Cell().Element(GrandTotalStyle).Text("Grand Total").Bold();
                    table.Cell().Element(GrandTotalStyle); // Empty padding cell under name column

                    decimal grandTotal = 0;

                    foreach (var eds in edsColumn)
                    {
                        var total = _entries
                            .Where(x => x.Eddsc == eds)
                            .Sum(x => x.Amount);

                        grandTotal += Convert.ToDecimal(total);

                        table.Cell()
                            .Element(GrandTotalStyle)
                            .AlignRight()
                            .Text(total.ToString("N2"))
                            .Bold();
                    }

                    table.Cell()
                        .Element(GrandTotalStyle)
                        .AlignRight()
                        .Text(grandTotal.ToString("N2"))
                        .Bold();
                });
            }
            else if (_reportBy == "By Analysis")
            {
                var edsColumn = _entries
                    .Select(x => x.Eddsc)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                var analysisGroups = _entries
                    .GroupBy(x => x.andsc)
                    .OrderBy(x => x.Key)
                    .ToList();

                container.Column(column =>
                {
                    foreach (var analysisGroup in analysisGroups)
                    {
                        // Analysis Heading
                        column.Item()
                            .PaddingTop(10)
                            .Text(analysisGroup.Key)
                            .FontSize(14)
                            .Bold();

                        column.Item().Table(table =>
                        {
                            BuildAnalysisTable(
                                table,
                                analysisGroup.ToList(),
                                edsColumn);
                        });
                    }

                    // Grand Totals Table
                    column.Item()
                        .PaddingTop(10);

                    column.Item().Table(table =>
                    {
                        BuildGrandTotalRow(
                            table,
                            _entries,
                            edsColumn);
                    });
                });
            }
        }

        private void BuildAnalysisTable(TableDescriptor table, List<EmptrsRpt> records, List<string> edsColumn)
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                foreach (var eds in edsColumn)
                {
                    columns.RelativeColumn(2);
                }
                columns.RelativeColumn(2);
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Employee");

                foreach (var eds in edsColumn)
                {
                    header.Cell()
                        .Element(HeaderCellStyle)
                        .Text(Truncate(eds, 12));
                }

                header.Cell()
                    .Element(HeaderCellStyle)
                    .Text("Total");
            });

            // Employee Rows
            var employees = records
                .GroupBy(x => x.firstname)
                .OrderBy(x => x.Key);   
                                 
                    int rowIndex = 0;
                    foreach (var emp in employees)
                    {
                        var background = rowIndex % 2 == 0 
                            ? Colors.Grey.Lighten3 
                            : Colors.White;

                        table.Cell()
                            .Element(c => DataCellStyle(c, background))
                            .Text(emp.Key);

                        decimal rowTotal = 0;

                        foreach (var eds in edsColumn)
                        {
                            decimal amount = Convert.ToDecimal(emp
                                .Where(x => x.Eddsc == eds)
                                .Sum(x => x.Amount));

                            rowTotal += amount;

                            table.Cell()
                                .Element(c => DataCellStyle(c, background))
                                .AlignRight()
                                .Text(amount == 0 ? "" : amount.ToString("N2"));
                        }

                        // Group Totals
                        table.Cell()
                            .Element(TotalCellStyle)
                            .Text("Sub Total")
                            .Bold();
                    }

                    decimal groupGrandTotal = 0;

                    foreach (var eds in edsColumn)
                    {
                        decimal total = Convert.ToDecimal(records
                            .Where(x => x.Eddsc == eds)
                            .Sum(x => x.Amount));

                        groupGrandTotal += total;

                        table.Cell()
                            .Element(GrandTotalStyle)
                            .AlignRight()
                            .Text(total.ToString("N2"));
                    }

                    table.Cell()
                        .Element(GrandTotalStyle)
                        .AlignRight()
                        .Text(groupGrandTotal.ToString("N2"))
                        .Bold();
                }

        private void BuildGrandTotalRow(TableDescriptor table, List<EmptrsRpt> records, List<string> edsColumn)
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);
                foreach (var eds in edsColumn)
                {
                    columns.RelativeColumn(2);
                }
                columns.RelativeColumn(2);
            });

            table.Cell()
                .Element(GrandTotalStyle)
                .Text("Grand Total")
                .Bold();

            decimal overallTotal = 0;

            foreach (var eds in edsColumn)
            {
                decimal total = Convert.ToDecimal(records
                    .Where(x => x.Eddsc == eds)
                    .Sum(x => x.Amount));

                overallTotal += total;

                table.Cell()
                    .Element(GrandTotalStyle)
                    .AlignRight()
                    .Text(overallTotal.ToString("N2")); 
            }
        }

        static IContainer HeaderCellStyle(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten2)
                .Padding(5);
        }

        private static IContainer DataCellStyle(IContainer container, string? background)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Background(background ?? Colors.White)
                .Padding(5);
        }

        static IContainer TotalCellStyle(IContainer container, string? background = null)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Medium)
                .Background(background ?? Colors.White)
                .Padding(5);
        }

        static IContainer GrandTotalStyle(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten2) 
                .Padding(5);
        }

        private static string Truncate(string value, int maxLength = 12)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return value.Length <= maxLength 
                ? value 
                : value.Substring(0, maxLength) + "...";
        }
    }
}

