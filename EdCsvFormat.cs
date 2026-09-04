using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using MsWeb.Databases.Commondbases;
using MsWeb.Databases.Paydbases;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace MsWeb.Report.Formats
{
    public class EdCsvFormat
    {
        public byte[] Csv(List<EmptrsRpt> entries, string reportBy)
        {
            if (entries == null || !entries.Any())
                return Array.Empty<byte>();

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, new UTF8Encoding(true));
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            var descriptions = entries
                .Select(x => x.Eddsc)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (reportBy == "By Analysis")
            {
                WriteLineAnalysis(csv, entries, descriptions);
                WriteGrandTotal(csv, entries, descriptions, groupAnalysis: true);
            }
            else if (reportBy == "By Employees")
            {
                WriteLineEmployee(csv, entries, descriptions);
                WriteGrandTotal(csv, entries, descriptions, groupAnalysis: false);
            }

            writer.Flush();

            Directory.CreateDirectory(@"C:\Temp");

            File.WriteAllBytes(
                @"C:\Temp\Test.csv",
                stream.ToArray());

            return stream.ToArray();
        }

        private void WriteLineAnalysis(CsvWriter csv, List<EmptrsRpt> entries, List<string> descriptions)
        {
            //Header
            csv.WriteField("Analysis");
            csv.WriteField("Employee");

            foreach (var analysisGroup in entries.GroupBy(x => x.andsc))
            {
                decimal[] analysisTotals = new decimal[descriptions.Count];
                decimal analysisGrandTotal = 0m;

                foreach (var employeeGroup in analysisGroup.GroupBy(x => x.firstname))
                {
                    csv.WriteField(analysisGroup.Key);
                    csv.WriteField(employeeGroup.Key);

                    decimal employeeTotal = 0m;

                    for (int i = 0; i < descriptions.Count; i++)
                    {
                        var desc = descriptions[i];

                        decimal amount = Convert.ToDecimal(employeeGroup
                            .Where(x => x.Eddsc == desc)
                            .Sum(x => x.Amount));

                        csv.WriteField(amount);

                        employeeTotal += amount;
                        analysisTotals[i] += amount;
                    }

                    csv.WriteField(employeeTotal);
                    csv.NextRecord();

                    analysisGrandTotal += employeeTotal;
                }

                // ANALYSIS TOTAL
                csv.WriteField($"{analysisGroup.Key} Total");
                csv.WriteField("");

                foreach (var total in analysisTotals)
                {
                    csv.WriteField(total);
                }

                csv.WriteField(analysisGrandTotal);
                csv.NextRecord();

                //Blank Line Between Groups
                csv.NextRecord();
            }
        }

        public void WriteLineEmployee(CsvWriter csv, List<EmptrsRpt> entries, List<string> descriptions)
        {
            // Header
            csv.WriteField("By Employee");

            foreach (var d in descriptions)
            {
                csv.WriteField(d);
            }

            csv.WriteField("Total");
            csv.NextRecord();

            foreach (var empGroup in entries.GroupBy(x => x.firstname))
            {
                csv.WriteField(empGroup.Key);

                decimal total = 0m;

                for (int i = 0; i < descriptions.Count; i++)
                {
                    var desc = descriptions[i];

                    decimal amount = Convert.ToDecimal(empGroup
                        .Where(x => x.Eddsc == desc)
                        .Sum(x => x.Amount));

                    csv.WriteField(amount);

                    total += amount;
                }

                csv.WriteField(total);
                csv.NextRecord();
            }
        }

        private void WriteGrandTotal(CsvWriter csv, List<EmptrsRpt> entries, List<string> descriptions, bool groupAnalysis)
        {
            csv.NextRecord();
            csv.WriteField("GRAND TOTAL");

            if (!groupAnalysis)
            {
                csv.WriteField("");
            }

            decimal grandTotal = 0m;

            foreach (var desc in descriptions)
            {
                decimal total = Convert.ToDecimal(entries
                    .Where(x => x.Eddsc == desc)
                    .Sum(x => x.Amount));

                csv.WriteField(total);
                grandTotal += total;
            }

            csv.WriteField(grandTotal);
            csv.NextRecord();
        }
    }
}
