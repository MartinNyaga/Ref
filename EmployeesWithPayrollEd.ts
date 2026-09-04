        //Employees with Payroll Ed
        if (selectedRptCode == "TR002") {
          if (!empCodes && !anCodes) {
            setNotificationMessage(
              "Select Employee(s) or Analysis in Reporting Criteria page."
            );
            setShowNotification(true);
            setTimeout(() => {
              setShowNotification(false);
            }, 1000);
          }
          if (!fields) {
            setNotificationMessage("Select Employee fields in Reporting Criteria.");
            setShowNotification(true);
            setTimeout(() => {
              setShowNotification(false);
            }, 1000);
          }
          if (!empCodes || !anCodes) return;
          if (!fields) return;
          if (reportBy == "employee") {
            const url = `${
              new Constants().baseUrl
            }/TroubleshootingRpt/GetEmployeesWithPayrolled`;
            const response = await axios.post<any[]>(
              url,
              {
                empCodes: empCodes,
                prdCodes: prdCodes,
                edCodes: edCodes,
                // prdCode : userSettings.payprdcode,
                printFomart: selectedFilters.printFormat,
                compName: userSettings.companyname,
                dbaseName: userSettings.dbaseName,
                prdDsc: userSettings.payprddsc,
                header: `Employees With Payroll Ed`,
              },
              { responseType: "blob" }
            );
            
            let blob = response.data;
            // let blob = new Blob([response.data],{type : "text/csv;charset=utf-8;" })
            let a;
            
            const Url = window.URL.createObjectURL(blob as any);
            a = document.createElement("a");
            a.href = Url;
            a.download =
              selectedFilters.printFormat === "pdf"
                ? "EmployeesWithPayrolled.pdf"
                : selectedFilters.printFormat === "excel"
                ? "EmployeesWithPayrolled.xlsx"
                : "EmployeesWithPayrolled.csv";
            document.body.appendChild(a);
            a.click();
            
            //Cleanup
            a.remove();
            return window.URL.revokeObjectURL(Url);
          } else if (reportBy == "analysis") {
            const url = `${
              new Constants().baseUrl
            }/TroubleshootingRpt/GetEmployeesByAnWithPayrolled`;
            const response = await axios.post<any[]>(
              url,
              {
                anCodes: anCodes,
                prdCodes: prdCodes,
                edCodes: edCodes,
                printFomart: selectedFilters.printFormat,
                compName: userSettings.companyname,
                dbaseName: userSettings.dbaseName,
                prdDsc: userSettings.payprddsc,
                header: `Employees With Ed Record`,
              },
              { responseType: "blob" }
            );
            
            let blob = response.data;
            let a;
            
            const Url = window.URL.createObjectURL(blob as any);
            a = document.createElement("a");
            a.href = Url;
            a.download =
              selectedFilters.printFormat === "pdf"
                ? "EmployeesByAnWithPayrolled.pdf"
                : selectedFilters.printFormat === "excel"
                ? "EmployeesByAnWithPayrolled.xlsx"
                : "EmployeesByAnWithPayrolled.csv";
            document.body.appendChild(a);
            a.click();
            
            //Cleanup
            a.remove();
            return window.URL.revokeObjectURL(Url);
          }
        }
