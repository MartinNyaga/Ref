        //Employees Leave Listing Report
        [HttpPost("GetEmpLveListingRpt")]
        public async Task<ActionResult<EmployeesDashboard>> GetEmpLveListingRpt(
            [FromBody] RequestBody request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var empCodes = request.empCodes;
            var lveCodes = request.lveCodes;
            var dateFrom = request.dateFrom;
            var dateTo = request.dateTo;
            //var columns = request.Fields;
            var header = request.header;
            var compName = request.compName;
            var dbaseName = request.dbaseName;
            var prdDsc = request.prdDsc;
            string reportBy = "By Employees";
            var employees = _employeeservice.GetEmpLveListingRpt(empCodes, lveCodes, dateFrom, dateTo);
            byte[] file;
            string? printFomart = request.printFomart;
            string fileName;
            string fileFormat;

            //if (printFomart == "pdf")
            //{
            //    var document = new EmpltPdfFormat(employees, header, compName, dbaseName, reportBy, prdDsc);
            //    file = document.GeneratePdf();
            //    fileName = "Employees Leave Listing Report.pdf";
            //    fileFormat = "application/pdf";
            //}
            //else if (printFomart == "excel")
            //{
            //    var document = new EmployeeExcelFormat();
            //    file = document.Excel(employees, columns);
            //    fileName = "Employees Leave Listing Report.xlsx";
            //    fileFormat = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //}
            //else if (printFomart == "csv")
            //{
            //    var document = new EmployeeCsvFormat();
            //    file = document.Csv(employees, columns);
            //    fileName = "Employees Leave Listing Report.csv";
            //    fileFormat = "text/csv";
            //}
            //else
            //{
            //    var document = new EmployeePdfFormat(employees, columns, header, compName, dbaseName, reportBy, prdDsc);
            //    file = document.GeneratePdf();
            //    fileName = "Employees Leave Listing Report.pdf";
            //    fileFormat = "application/pdf";
            //}

            var document = new EmpltPdfFormat(employees, header, compName, dbaseName, reportBy, prdDsc);
            file = document.GeneratePdf();
            fileName = "Employees Leave Listing Report.pdf";
            fileFormat = "application/pdf";

            return File(file, fileFormat, fileName);
        }

        //Employees Leave Listing Report
        public List<EmpltModel> GetEmpLveListingRpt([FromQuery(Name = "empCodes[]")] List<string> empCodes, List<string> lveCodes, string dateFrom, string dateTo)
        {
            if (empCodes == null || empCodes.Count == 0)
            {
                return new List<EmpltModel>();
            }

            var codes = string.Join(",", empCodes.Select(c => $"'{c}'"));

            if (lveCodes == null || lveCodes.Count == 0)
            {
                return new List<EmpltModel>();
            }

            var lvecodes = string.Join(",", lveCodes.Select(c => $"'{c}'"));

            //string sql = $"SELECT DISTINCT e.* FROM emp{CurrentDatabase} AS e LEFT JOIN emptr{CurrentDatabase} ON (emptr{CurrentDatabase}).empcode = e.empc
            string sql = $"SELECT emp.Title,emp.firstname,emp.othername,emp.sirname,lveeds.lvedsc,e.* FROM emplt{CurrentDatabase} AS e INNER JOIN lveeds ON (lveeds.lvecode = e.lvecode) INNER JOIN emp{CurrentDatabase} AS emp ON (emp.empcode  = e.empcode) WHERE emp.termdate IS NULL AND e.empcode IN ({codes} AND e.lvecode IN ({lvecodes}) AND e.lvedate BETWEEN '{dateFrom}' AND '{dateTo}'";

            var table = _databaseAccess.SelectQuery(sql);
            return table.AsEnumerable().Select(row =>
            {
                return row.ToObject<EmpltModel>();
            }).ToList();
        }
