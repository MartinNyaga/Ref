        //Fetch Employees with a payrolled record
        [HttpPost("GetEmployeesWithPayrolled")]
        public async Task<ActionResult<EmptrsRpt>> GetEmployeesWithPayrolled(
            [FromBody] RequestBody request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var empCodes = request.empCodes;
            ///var prdCode = request.prdCode;
            var prdsCodes = request.prdCodes;
            var edCodes = request.edCodes;
            ///var edCode = request.edCode;
            var header = request.header;
            var compName = request.compName;
            var dbaseName = request.dbaseName;
            var prdDsc = request.prdDsc;
            string reportBy = "By Employees";
            
            var entries = _employeeservice.GetEmployeesWithPayrolled(empCodes, prdsCodes, edCodes);
            byte[] file;
            string? printFomart = request.printFomart;
            string fileName;
            string fileFormat;

            if (edCodes == null || edCodes.Count == 0)
            {
                return BadRequest("No fields provided");
            }

            if (printFomart == "pdf")
            {
                var document = new EdPdfFormat(entries, header, compName, dbaseName, reportBy, prdDsc);
                file = document.GeneratePdf();
                fileName = "EmployeesWithPayrolled.pdf";
                fileFormat = "application/pdf";
            }
            else if (printFomart == "excel")
            {
                var document = new EdsExcelFormat();
                file = document.Excel(entries, reportBy);
                fileName = "EmployeesWithPayrolled.xlsx";
                fileFormat = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else if (printFomart == "csv")
            {
                var document = new EdCsvFormat();
                file = document.Csv(entries, reportBy);
                fileName = "EmployeesWithPayrolled.csv";
                fileFormat = "text/csv";
            }
            else
            {
                var document = new EdPdfFormat(entries, header, compName, dbaseName, reportBy, prdDsc);
                file = document.GeneratePdf();
                fileName = "EmployeesWithPayrolled.pdf";
                fileFormat = "application/pdf";
            }

            return File(file, fileFormat, fileName);
        }


        //Fetch Employees with a payroll ed record
        public List<EmptrsRpt> GetEmployeesWithPayrolled([FromQuery(Name = "empCodes[]")] List<string> empCodes, List<int> prdCodes, List<string> edCodes)
        {
            if (empCodes == null || empCodes.Count == 0)
            {
                return new List<EmptrsRpt>();
            }

            if (prdCodes == null || prdCodes.Count == 0)
            {
                return new List<EmptrsRpt>();
            }

            if (edCodes == null || edCodes.Count == 0)
            {
                return new List<EmptrsRpt>();
            }

            var codes = string.Join(",", empCodes.Select(c => $"'{c}'"));
            var prds = string.Join(",", prdCodes.Select(c => $"'{c}'"));
            var eds = string.Join(",", edCodes.Select(c => $"'{c}'"));

            var whereClause = string.Join(" OR ", 
                edCodes.Select(ed => $"COALESCE({ed}, 0) <> 0"));

            // SQL script execution (Truncated context)
            string sql = $"SELECT p.eddsc, emp{CurrentDatabase}.firstname, tr.* FROM emptr{CurrentDatabase} AS tr INNER JOIN emp{CurrentDatabase} ON (tr.empcode = emp{CurrentDatabase}.empcode) INNER JOIN payeds AS p ON (p.edcode = tr.edcode) WHERE tr.empcode IN ({codes} AND tr.payprdcode IN ({prds}) AND emp{CurrentDatabase}.termdate IS NULL)";
            
            Console.WriteLine(sql);
            var table = _databaseAccess.SelectQuery(sql);
            
            return table.AsEnumerable().Select(row =>
            {
                return row.ToObject<EmptrsRpt>();
            }).ToList();
        }
