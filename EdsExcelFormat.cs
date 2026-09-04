using ClosedXML.Excel;
using MsWeb.Databases.Commondbases;
using MsWeb.Databases.Paydbases;
using System.Reflection;

namespace MsWeb.Report.Formats
{
    public class EdsExcelFormat
    {
        public byte[] Excel(List<EmptrsRpt> entries, string reportBy)
        {
            if (entries == null || !entries.Any())
            {
                return Array.Empty<byte>();
            }

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("reportxls");

            // Get dynamic columns
            var columns = entries
                .Select(x => x.Eddsc)
                .Distinct()
                .ToList();

            // Group by Employee
            if (reportBy == "By Employees")
            {
                // HEADER
                ws.Cell(1, 1).Value = "Employee";

                int colIndex = 2;
                foreach (var col in columns)
                {
                    ws.Cell(1, colIndex).Value = col;
                    colIndex++;
                }

                ws.Cell(1, colIndex).Value = "Total";

                var headerRange = ws.Range(1, 1, 1, colIndex);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;

                var grouped = entries
                    .GroupBy(x => x.Empcode);

                foreach (var empGroup in grouped)
                {
                    ws.Cell(row, 1).Value = empGroup.Key;

                    decimal total = 0m;

                    for (int i = 0; i < columns.Count; i++)
                    {
                        var desc = columns[i];

                        var amount = empGroup
                            .Where(x => x.Eddsc == desc)
                            .Sum(x => x.Amount);

                        ws.Cell(row, i + 2).Value = amount;
                        ws.Cell(row, i + 2).Style.NumberFormat.Format = "#,##0.00";

                        total += Convert.ToDecimal(amount);
                    }

                    // total column
                    ws.Cell(row, columns.Count + 2).Value = total;
                    ws.Cell(row, columns.Count + 2).Style.Font.Bold = true;

                    row++;
                }

                // STYLING
                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();

                ws.Range(1, 1, row - 1, columns.Count + 2)
                    .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                ws.Range(1, 1, row - 1, columns.Count + 2)
                    .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.SheetView.FreezeRows(1);
            }
            // Group BY Analysis
            else if (reportBy == "By Analysis")
            {
                // HEADER
                ws.Cell(1, 1).Value = "Analysis";
                ws.Cell(1, 2).Value = "Employee";

                int colIndex = 3;
                foreach (var col in columns)
                {
                    ws.Cell(1, colIndex).Value = col;
                }

                ws.Cell(1, colIndex).Value = "Total";

                var headerRange = ws.Range(1, 1, 1, colIndex);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                int row = 2;

                foreach (var analysisGroup in entries.GroupBy(x => x.andsc))
                {
                    string analysis = analysisGroup.Key;

                    // Analysis Heading Row
                    ws.Cell(row, 1).Value = analysis;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    // Analysis Totals
                    decimal[] analysisTotals = new decimal[columns.Count];
                    decimal analysisGrandTotal = 0m;

                    // Employees With Analysis
                    foreach (var employeeGroup in analysisGroup.GroupBy(x => x.firstname))
                    {
                        ws.Cell(row, 2).Value = employeeGroup.Key;

                        decimal employeeTotal = 0m;

                        for (int i = 0; i < columns.Count; i++)
                        {
                            string desc = columns[i];

                            decimal amount = Convert.ToDecimal(employeeGroup
                                .Where(x => x.Eddsc == desc)
                                .Sum(x => x.Amount));

                            ws.Cell(row, i + 3).Value = amount;
                            ws.Cell(row, i + 3).Style.NumberFormat.Format = "#,##0.00";

                            employeeTotal += amount;
                            analysisTotals[i] += amount;
                        }

                        ws.Cell(row, columns.Count + 3).Value = employeeTotal;
                        ws.Cell(row, columns.Count + 3).Style.NumberFormat.Format = "#,##0.00";

                        analysisGrandTotal += employeeTotal;

                        row++;
                    }

                    // Analysis Total Row
                    ws.Cell(row, 1).Value = $"{analysis} Total";
                    ws.Cell(row, 1).Style.Font.Bold = true;

                    for (int i = 0; i < columns.Count; i++)
                    {
                        ws.Cell(row, i + 3).Value = analysisTotals[i];
                        ws.Cell(row, i + 3).Style.Font.Bold = true;
                        ws.Cell(row, i + 3).Style.NumberFormat.Format = "#,##0.00";
                    }

                    ws.Cell(row, columns.Count + 3).Value = analysisGrandTotal;
                    ws.Cell(row, columns.Count + 3).Style.Font.Bold = true;
                    ws.Cell(row, columns.Count + 3).Style.NumberFormat.Format = "#,##0.00";

                    row += 2;
                }

                // STYLING
                ws.Columns().AdjustToContents();
                ws.Rows().AdjustToContents();

                ws.Range(1, 1, row - 1, columns.Count + 3)
                    .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                ws.Range(1, 1, row - 1, columns.Count + 3)
                    .Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.SheetView.FreezeRows(1);
            }

            // EXPORT
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}
