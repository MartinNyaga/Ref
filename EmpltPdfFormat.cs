using MsWeb.Databases.Lvedbases;
using MsWeb.Databases.Paydbases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace MsWeb.Report.Formats
{
    public class EmpltPdfFormat : IDocument
    {
        private readonly List<EmpltModel> _entries;
        public readonly string _header;
        public readonly string? _compName;
        public readonly string? _dbaseName;
        public readonly string? _reportBy;
        public readonly string? _prdDsc;

        public EmpltPdfFormat(List<EmpltModel> entries, string header, string? compName, string? dbaseName, string? reportBy, string? prdDsc)
        {
            _entries = entries ?? new List<EmpltModel>();
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
                page.Size(PageSizes.A4.Portrait());
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
                //GROUP EMPLOYEES
                var employees = _entries
                    .GroupBy(x => new
                    {
                        x.empcode,
                        x.fullName
                    })
                    .OrderBy(x => x.Key.empcode)
                    .ToList();

                container.Column(main =>
                {
                    foreach (var employee in employees)
                    {
                        main.Item().PaddingTop(12);
                        main.Item().PaddingBottom(5);
                        main.Item().Element(c => EmployeeHeader(c, employee.Key.empcode, employee.Key.fullName));

                        //Employee Leave Table
                        main.Item().Element(c => ComposeLeaveTable(c, employee));
                    }
                });
            }
            else if (_reportBy == "By Analysis")
            {
                // Implementation for By Analysis handled outside this viewport view cut
            }
        }

        private void ComposeLeaveTable(IContainer container, IEnumerable<EmpltModel> entries)
        {
            container.Table(table =>
            {
                //Columns
                table.ColumnsDefinition(column =>
                {
                    column.RelativeColumn(2);
                    column.RelativeColumn(2);
                    column.RelativeColumn(2);
                    column.RelativeColumn(5);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderStyle).Text("Date").Bold();
                    header.Cell().Element(HeaderStyle).AlignRight().Text("Leave Code").Bold();
                    header.Cell().Element(HeaderStyle).Text("Leave Amount").Bold();
                    header.Cell().Element(HeaderStyle).Text("Leave Description").Bold();
                });

                // DATA
                int rowIndex = 0;
                foreach (var entry in entries)
                {
                    var background = rowIndex % 2 == 0
                        ? Colors.White
                        : Colors.Grey.Lighten3;

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(DateTime.ParseExact(entry.lvedate, "yyyyMMdd", null).ToString("dd/MM/yyyy"));

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(entry.lvecode ?? "");

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(entry.lveamt.ToString() ?? "");

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(entry.lvedsc ?? "");

                    rowIndex++;
                }
            });
        }

        public void EmployeeHeader(IContainer container, string empcode, string fullName)
        {
            container
                .Background(Colors.Grey.Lighten2)
                .Border(1)
                .Padding(7)
                .Text(text =>
                {
                    text.Span($"{empcode} - {fullName}")
                        .FontSize(12)
                        .FontColor(Colors.Blue.Accent4)
                        .Bold();
                });
        }

        private static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Darken1)
                .Background(Colors.Grey.Lighten2)
                .Padding(5);
        }

        private static IContainer DataCell(IContainer container, string background)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Background(background)
                .Padding(5);
        }

        private static IContainer TotalCell(IContainer container)
        {
            return container
                .Border(1)
                .BackgroundColor(Colors.Black)
                .Background(Colors.Grey.Lighten2)
                .Padding(5);
        }
    }
}
