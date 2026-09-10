using ClosedXML.Excel;
using MsWeb.Databases.Lvedbases;
using System.Globalization;

namespace MsWeb.Report.Formats
{
    public class EmpltExcelFormat
    {
        public byte[] Excel(List<EmpltModel> entries)
        {
            if (entries == null || !entries.Any())
            {
                return Array.Empty<byte>();
            }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("reportxls");

            int row = 1;

            // GROUP EMPLOYEES
            var employees = entries
                .GroupBy(x => new
                {
                    x.empcode,
                    x.fullName
                })
                .OrderBy(x => x.Key.empcode)
                .ToList();

            foreach (var employee in employees)
            {
                // EMPLOYEE HEADER
                ws.Cell(row, 1).Value =
                    $"{employee.Key.empcode} - {employee.Key.fullName}";

                var employeeHeader = ws.Range(row, 1, row, 4);

                employeeHeader.Merge();

                employeeHeader.Style.Font.Bold = true;
                employeeHeader.Style.Font.FontSize = 12;
                employeeHeader.Style.Font.FontColor = XLColor.Blue;
                employeeHeader.Style.Fill.BackgroundColor = XLColor.LightGray;
                employeeHeader.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                employeeHeader.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Left;

                row++;

                // TABLE HEADER
                ws.Cell(row, 1).Value = "Date";
                ws.Cell(row, 2).Value = "Leave Code";
                ws.Cell(row, 3).Value = "Leave Amount";
                ws.Cell(row, 4).Value = "Leave Description";

                var headerRange = ws.Range(row, 1, row, 4);

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Border.OutsideBorder =
                    XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder =
                    XLBorderStyleValues.Thin;
                headerRange.Style.Alignment.Horizontal =
                    XLAlignmentHorizontalValues.Center;

                row++;

                // DATA ROWS
                int rowIndex = 0;

                foreach (var entry in employee)
                {
                    var background = rowIndex % 2 == 0
                        ? XLColor.White
                        : XLColor.LightGray;

                    // DATE
                    DateTime parsedDate;

                    if (DateTime.TryParseExact(
                        entry.lvedate,
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out parsedDate))
                    {
                        ws.Cell(row, 1).Value = parsedDate;
                        ws.Cell(row, 1).Style.DateFormat.Format = "dd/MM/yyyy";
                    }
                    else
                    {
                        ws.Cell(row, 1).Value = entry.lvedate ?? "";
                    }

                    // LEAVE CODE
                    ws.Cell(row, 2).Value = entry.lvecode ?? "";

                    // LEAVE AMOUNT
                    ws.Cell(row, 3).Value = entry.lveamt;
                    ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";

                    // LEAVE DESCRIPTION
                    ws.Cell(row, 4).Value = entry.lvedsc ?? "";

                    // DATA STYLING
                    var dataRange = ws.Range(row, 1, row, 4);

                    dataRange.Style.Fill.BackgroundColor = background;
                    dataRange.Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    // ALIGNMENT
                    ws.Cell(row, 1).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Left;

                    ws.Cell(row, 2).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Left;

                    ws.Cell(row, 3).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Right;

                    ws.Cell(row, 4).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Left;

                    row++;
                    rowIndex++;
                }

                // SPACE BETWEEN EMPLOYEES
                row++;
            }

            // COLUMN WIDTHS
            ws.Column(1).Width = 15;
            ws.Column(2).Width = 15;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 35;

            // FREEZE HEADER
            ws.SheetView.FreezeRows(1);

            // PAGE SETUP
            ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
            ws.PageSetup.AdjustTo(100);

            // PRINT SETTINGS
            ws.PageSetup.SetRowsToRepeatAtTop(1, 1);

            // SAVE
            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}