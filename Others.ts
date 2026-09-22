    // Employees with No Swipes
    if (selectedRptCode == "MT019") {
      if (!dateFrom || !dateTo) {
        setNotificationMessage(
          "Select Date From and To in Reporting Criteria."
        );
        setShowNotification(true);
        setTimeout(() => {
          setShowNotification(false);
        }, 1000);
        return;
      }

      fields = [
        { key: "empcode", Title: "Employee Code" },
        { key: "FullName", Title: "Employee Name" },
      ];

      const url = `${
        new Constants().baseUrl
      }/StandardLveRpt/GetEmployeesWithNoSwipes`;

      const response = await axios.post<any[]>(
        url,
        {
          dateFrom: dateFrom,
          dateTo: dateTo,
          printFomart: selectedFilters.printFormat,
          compName: userSettings.companyname,
          dbaseName: userSettings.dbaseName,
          prdDsc: userSettings.payprddsc,
          header: `Employees with No Swipes Between ${formatDate(
            dateFrom
          )} - ${formatDate(dateTo)}`,
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
          ? "Employees with No Swipes.pdf"
          : selectedFilters.printFormat === "excel"
          ? "Employees with No Swipes.xlsx"
          : "Employees with No Swipes.csv";

      document.body.appendChild(a);
      a.click();

      // Cleanup
      a.remove();
      return window.URL.revokeObjectURL(Url);
    }