```csharp
using CsvHelper;
using MsWeb.Databases.Lvedbases;
using System.Globalization;
using System.Text;

namespace MsWeb.Report.Formats
{
    public class EmpltCsvFormat
    {
        public byte[] Csv(List<EmpltModel> entries)
        {
            if (entries == null || !entries.Any())
            {
                return Array.Empty<byte>();
            }

            using var stream = new MemoryStream();
            using var writer = new StreamWriter(
                stream,
                new UTF8Encoding(true),
                leaveOpen: true);

            using var csv = new CsvWriter(
                writer,
                CultureInfo.InvariantCulture);

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
                csv.WriteField(
                    $"{employee.Key.empcode} - {employee.Key.fullName}");

                csv.NextRecord();

                // TABLE HEADER
                csv.WriteField("Date");
                csv.WriteField("Leave Code");
                csv.WriteField("Leave Amount");
                csv.WriteField("Leave Description");

                csv.NextRecord();

                // DATA ROWS
                foreach (var entry in employee)
                {
                    // DATE
                    DateTime parsedDate;

                    if (DateTime.TryParseExact(
                        entry.lvedate,
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out parsedDate))
                    {
                        csv.WriteField(parsedDate.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        csv.WriteField(entry.lvedate ?? "");
                    }

                    // LEAVE CODE
                    csv.WriteField(entry.lvecode ?? "");

                    // LEAVE AMOUNT
                    csv.WriteField(
                        entry.lveamt.ToString("N2"));

                    // LEAVE DESCRIPTION
                    csv.WriteField(entry.lvedsc ?? "");

                    csv.NextRecord();
                }

                // BLANK LINE BETWEEN EMPLOYEES
                csv.NextRecord();
            }

            writer.Flush();

            return stream.ToArray();
        }
    }
}
```
