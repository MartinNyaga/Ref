using MsWeb.Databases.Paydbases;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MsWeb.Report.Formats
{
    public class EmpByLveGrpPdfFormat : IDocument
    {
        private readonly List<Employees> _entries;
        public readonly string _header;
        public readonly string? _compName;
        public readonly string? _dbaseName;
        public readonly string? _reportBy;
        public readonly string? _prdDsc;

        public EmpByLveGrpPdfFormat(
            List<Employees> entries,
            string header,
            string? compName,
            string? dbaseName,
            string? reportBy,
            string? prdDsc)
        {
            _entries = entries ?? new List<Employees>();
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
                row.RelativeItem()
                    .AlignRight()
                    .PaddingBottom(10)
                    .Text(_prdDsc);
            });
        }

        void ComposeBrandHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Text(_compName).Bold();
                row.RelativeItem().AlignCenter().Text(_dbaseName).SemiBold();
                row.RelativeItem()
                    .AlignRight()
                    .Text($"Printed at: {DateTime.UtcNow}");
            });
        }

        void ComposeTable(IContainer container)
        {
            var leaveGroups = _entries
                .GroupBy(x => new
                {
                    x.lvegrpcode1,
                    x.lvegrpdsc
                })
                .OrderBy(x => x.Key.lvegrpcode1)
                .ToList();

            container.Column(main =>
            {
                foreach (var leaveGroup in leaveGroups)
                {
                    main.Item()
                        .PaddingTop(12);

                    main.Item()
                        .PaddingBottom(5)
                        .Element(c =>
                            LeaveGroupHeader(
                                c,
                                leaveGroup.Key.lvegrpcode1,
                                leaveGroup.Key.lvegrpdsc
                            )
                        );

                    main.Item()
                        .Element(c => ComposeEmployeeTable(c, leaveGroup));
                }
            });
        }

        private void ComposeEmployeeTable(
            IContainer container,
            IEnumerable<Employees> entries)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(column =>
                {
                    column.RelativeColumn(2);
                    column.RelativeColumn(5);
                });

                table.Header(header =>
                {
                    header.Cell()
                        .Element(HeaderStyle)
                        .Text("Employee Code")
                        .Bold();

                    header.Cell()
                        .Element(HeaderStyle)
                        .Text("Employee Name")
                        .Bold();
                });

                int rowIndex = 0;

                foreach (var entry in entries.OrderBy(x => x.empcode))
                {
                    var background = rowIndex % 2 == 0
                        ? Colors.White
                        : Colors.Grey.Lighten3;

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(entry.empcode ?? "");

                    var fullName = string.Join(
                        " ",
                        new[]
                        {
                            entry.firstname,
                            entry.othername,
                            entry.sirname
                        }
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                    );

                    table.Cell()
                        .Element(c => DataCell(c, background))
                        .Text(fullName);

                    rowIndex++;
                }
            });
        }

        public void LeaveGroupHeader(
            IContainer container,
            string? lvegrpcode,
            string? lvegrpdsc)
        {
            container
                .Background(Colors.Grey.Lighten2)
                .Border(1)
                .Padding(7)
                .Text(text =>
                {
                    text.Span($"{lvegrpcode} - {lvegrpdsc}")
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

        private static IContainer DataCell(
            IContainer container,
            string background)
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