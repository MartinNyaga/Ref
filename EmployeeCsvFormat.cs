using MsWeb.Databases.Commondbases;
using MsWeb.Databases.Paydbases;
using System.Reflection;
using System.Text;

namespace MsWeb.Report.Formats
{
    public class EmployeeCsvFormat
    {
        public byte[] Csv(List<EmployeesDashboard> employees, List<ColumnDefinition> columns)
        {
            var sb = new StringBuilder();

            //HEADER ROW
            var headers = columns.Where(c => !string.IsNullOrWhiteSpace(c.Key)).Select(col => 
            {
                var header = string.IsNullOrWhiteSpace(col.Title)
                    ? col.Key
                    : col.Title;

                return header.Trim();
                //return Escape(header.Trim());
            }).ToList();

            if (!headers.Any())
            {
                throw new Exception("Invalid column configuration");
            }

            sb.AppendLine(string.Join(",", headers));

            //DATA ROWS
            foreach (var emp in employees)
            {
                var row = new List<string>();

                foreach (var col in columns)
                {
                    var property = emp.GetType().GetProperty(
                        col.Key,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    var value = property?.GetValue(emp);
                    row.Add(Escape(FormatValue(value)));
                }

                sb.AppendLine(string.Join(",", row));
            }

            // RETURN AS BYTE[] AND OPENS IN EXCEL
            return Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
                .ToArray();
        }

        public string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        public string FormatValue(object value)
        {
            if (value == null) return " ";

            return value switch
            {
                decimal d => d.ToString("N2"),
                double d => d.ToString("N2"),
                DateTime dt => dt.ToString("dd MM yyyy"),
                _ => value.ToString()
            };
        }
    }
}
