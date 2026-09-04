    // Employees Leave Listing Report
    if (selectedRptCode == "SR061") {
      if (!empCodes && !anCodes) {
        setNotificationMessage(
          "Select Employee(s) or Analysis in Reporting Criteria page."
        );
        setShowNotification(true);
        setTimeout(() => {
          setShowNotification(false);
        }, 1000);
      }
      if (!dateFrom || !dateTo) {
        setNotificationMessage(
          "Select Date From and To in Reporting Criteria."
        );
        setShowNotification(true);
        setTimeout(() => {
          setShowNotification(false);
        }, 1000);
      }
      if (!showSelectedLveRow) {
        setNotificationMessage("Select Leave Code.");
        setShowNotification(true);
        setTimeout(() => {
          setShowNotification(false);
        }, 1000);
      }
      fields = [
        { key: "empcode", Title: "Employee Code" },
        { key: "FullName", Title: "Employee Name" },
      ];
      if (reportBy == "employee") {
        if (!empCodes || !dateFrom || !dateTo) return;
        const url = `${
          new Constants().baseUrl
        }/StandardLveRpt/GetEmpLveListingRpt`;
        const response = await axios.post<any[]>(
          url,
          {
            empCodes: empCodes,
            lveCodes: lveCodes,
            dateFrom: dateFrom,
            dateTo: dateTo,
            printFomart: "pdf",
            compName: userSettings.companyname,
            dbaseName: userSettings.dbaseName,
            prdDsc: userSettings.payprddsc,
            header: `Employees Leave Listing Report Between ${formatDate(
              dateFrom
            )} - ${formatDate(dateTo)} Reporter`,
            // fields: fields,
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
            ? "Employees Leave Listing Report.pdf"
            : selectedFilters.printFormat === "excel"
            ? "Employees Leave Listing Report.xlsx"
            : "Employees Leave Listing Report.csv";
        document.body.appendChild(a);
        a.click();

        //Cleanup
        a.remove();
        return window.URL.revokeObjectURL(Url);
      } else if (reportBy == "analysis") {
        if (!anCodes || !dateFrom || !dateTo) return;
        const url = `${
          new Constants().baseUrl
        }/StandardLveRpt/GetEmpByAnLveListingRpt`;
        const response = await axios.post<any[]>(
          url,
          {
            anCodes: anCodes,
            lveCodes: lveCodes,
            dateFrom: dateFrom,
            dateTo: dateTo,
            printFomart: "pdf",
            compName: userSettings.companyname,
            dbaseName: userSettings.dbaseName,
            prdDsc: userSettings.payprddsc,
            header: `Employees Without a Leave Code Between ${formatDate(
              dateFrom
            )} - ${formatDate(dateTo)} Reporter`,
            fields: fields,
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
            ? "Employees By Analysis Leave Listing Report.pdf"
            : selectedFilters.printFormat === "excel"
            ? "Employees By Analysis Leave Listing Report.xlsx"
            : "Employees By Analysis Leave Listing Report.csv";
        document.body.appendChild(a);
        a.click();

        //Cleanup
        a.remove();
        return window.URL.revokeObjectURL(Url);
      }
    }
