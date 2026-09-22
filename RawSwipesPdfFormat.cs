using MsWeb.Databases.TnaDbases;
using MsWeb.Sharedfunctions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Reflection;

namespace MsWeb.Report.Formats
{
    public class RawSwipesPdfFormat : IDocument
    {
        private readonly List<RawSwipes> _swipes;
        private readonly List<ColumnDefinition> _columns;
        public readonly string _header;
        public readonly string _compName;
        public readonly string _dbaseName;
        public readonly string _reportBy;
        public readonly string? _prdDsc;

        public RawSwipesPdfFormat(
            List<RawSwipes> swipes,
            List<ColumnDefinition> columns,
            string header,
            string? compName,
            string? dbaseName,
            string? reportBy,
            string? prdDsc)
        {
            _swipes = swipes ?? new List<RawSwipes>();
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
            });
        }

        void ComposeTable(IContainer container)
        {
            if (!_columns.Any())
            {
                container.Text("NO COLUMN SELECTED!");
                return;
            }

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

                int rowIndex = 0;

                foreach (var swipe in _swipes)
                {
                    var background = rowIndex % 2 == 0
                        ? Colors.Grey.Lighten3
                        : Colors.White;

                    foreach (var col in _columns)
                    {
                        var property = swipe.GetType().GetProperty(
                            col.Key,
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance);

                        var value = property?.GetValue(swipe);

                        table.Cell()
                            .Element(c => DataCell(c, background))
                            .Text(FormatValue(value));
                    }

                    rowIndex++;
                }
            });
        }

        static IContainer HeaderStyle(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten2)
                .Padding(5);
        }

        static IContainer DataCell(
            IContainer container,
            string? background = null)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten3)
                .Background(background ?? Colors.White)
                .Padding(5);
        }

        public string FormatValue(object value)
        {
            if (value == null)
                return " ";

            return value switch
            {
                decimal d => d.ToString("N2"),
                DateTime dt => dt.ToString("dd MM yyyy"),
                _ => value.ToString()
            };
        }
    }
}