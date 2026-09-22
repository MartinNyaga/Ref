using ClosedXML.Excel;
using MsWeb.Databases.Commondbases;
using MsWeb.Databases.TnaDbases;
using System.Reflection;

namespace MsWeb.Report.Formats
{
    public class RawSwipesExcelFormat
    {
        public byte[] Excel(List<RawSwipes> swipes, List<ColumnDefinition> columns)
        {
            using var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("reportxls");

            //HEADER ROW INDEX
            int headerRow = 1;

            //HEADER ROW 1
            for (int colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                var col = columns[colIndex];

                var title = string.IsNullOrWhiteSpace(col.Title)
                    ? col.Key
                    : col.Title;

                var cell = sheet.Cell(headerRow, colIndex + 1);

                cell.Value = title;

                // STYLING
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            //DATA ROWS
            for (int rowIndex = 0; rowIndex < swipes.Count; rowIndex++)
            {
                var swipe = swipes[rowIndex];

                for (int colIndex = 0; colIndex < columns.Count; colIndex++)
                {
                    var col = columns[colIndex];

                    var property = swipe.GetType().GetProperty(
                        col.Key,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    var value = property?.GetValue(swipe);
                    var cell = sheet.Cell(rowIndex + 2, colIndex + 1);

                    cell.Value = FormatValue(value);

                    // BORDER STYLING
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // ALIGNMENT - SMART FORMATTING
                    if (value is decimal || value is int || value is double)
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    else
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                }
            }

            // AUTO WIDTH
            for (int colIndex = 0; colIndex < columns.Count; colIndex++)
            {
                var col = columns[colIndex];
                var excelCol = sheet.Column(colIndex + 1);

                if (col.WidthPx.HasValue)
                {
                    excelCol.Width = col.WidthPx.Value / 7;
                }
                else if (col.WidthRatio.HasValue)
                {
                    excelCol.Width = col.WidthRatio.Value * 5;
                }
                else
                {
                    excelCol.AdjustToContents();
                }
            }

            //FREEZE HEADER ROW
            sheet.SheetView.FreezeRows(1);

            // REPEAT HEADER WHEN PRINTING
            sheet.PageSetup.SetRowsToRepeatAtTop(1, 1);

            //PAGE SETUP
            sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            sheet.PageSetup.AdjustTo(100);

            //SAVE FILE
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
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