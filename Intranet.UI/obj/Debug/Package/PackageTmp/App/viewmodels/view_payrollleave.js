define(['services/logger', 'services/datacontext', 'services/helpers', 'viewmodels/editors/baseeditor', 'viewmodels/editors/leaveSummaryTest'],
    function (logger, dataContext, helpers, baseEditor, leaveSummaryTest) {

        var leaverecords = ko.observableArray();
        var canSearch = ko.observable(true);
        var searching = ko.observable(false);
        var summary = ko.observable();
        var defStart = new Date();
        var defEnd = new Date();
        defStart.setDate(defStart.getDate() - 7);

        var selectedCompany = ko.observable();
        var selectedDivision = ko.observable();
        var selectedStaff = ko.observable();
        var selectedLeave = ko.observable();

        var companies = ko.observableArray();
        var divisions = ko.observableArray();
        var staff = ko.observableArray();
        var carriers = ko.observableArray();
        var leaveTypes = ko.observableArray();


        var canSelectDivision = ko.observable(false);
        var canSelectStaff = ko.observable(false);

        var companyList = ko.observableArray();
        var summaryTotalAmount = ko.observable();
        var summaryTotalPercentage = ko.observable();
        var showSummaryFooter = ko.observable(false);
        var authorised = ko.observable(false);

        var csvFile = ko.observableArray();

        function activate() {
            if (dataContext.UserInRoles(authorised, dataContext.userRoleEnum.manager)) {
                authorised(true);
            }
            return dataContext.leaveTypes(leaveTypes).then(function () {
                return dataContext.getBranches(companies).then(function () {
                    //return canSave(true);
                });
            });
        }

        function viewattached() {
            $("#startDate").datepicker({
                dateFormat: "dd-mm-yy",
                onClose: function () {
                    var tempDate = $("#startDate").datepicker("getDate");
                    $("#endDate").datepicker("option", "minDate", tempDate);
                }
            }).datepicker("setDate", defStart);
            $("#endDate").datepicker({ dateFormat: "dd-mm-yy", minDate: defStart }).datepicker("setDate", defEnd);
        }

        function getCompanyLeave() {

            leaverecords.removeAll();
            companyList.removeAll();

            searching(true);
            var startDate = new Date($("#startDate").datepicker("getDate"));
            var endDate = new Date($("#endDate").datepicker("getDate"));
            var company = selectedCompany() == null ? null : selectedCompany();
            var division = selectedDivision() == null ? null : selectedDivision();
            var staffMember = selectedStaff() == null ? null : selectedStaff();
            var leaveType = selectedLeave() == null ? null : selectedLeave();

            return dataContext.getCompanyStaffLeaveBalances(leaverecords, new Date(startDate).toString(), new Date(endDate).toString(), company, division, staffMember, leaveType).then(function () {
                var test = leaverecords();
                companyList(buildSummary(leaverecords));
                showSummaryFooter(leaverecords().length > 0);
                canSearch(true);
                searching(false);
            });
        }

        function buildSummary(records) {
            var totals = 0;
            var percTotal = 0;
            var compList = [];

            for (var m = 0; m < records().length; m++) {
                //get the total leave for all companies
                totals += records()[m].companyTotal;
            }

            for (var i = 0; i < records().length; i++) {
                var percValue = calculatePercentage(records()[i].companyTotal);
                compList.push({ companyName: records()[i].companyName, companyTotal: records()[i].companyTotal, percentage: percValue + "%" });
                percTotal += Number(percValue);
            }
            summaryTotalAmount(totals.toFixed(2));
            summaryTotalPercentage(percTotal.toFixed(2));
            return compList;

            function calculatePercentage(callCost) {
                if (totals < 1 || callCost < 1)
                    return 0;
                var value = (callCost / totals);
                return (value * 100).toFixed(2);
            }
        }

        function cmbCompanyChange() {
            if (selectedCompany() != null) {
                return dataContext.getDivisions(divisions, selectedCompany()).then(function () {
                    canSelectDivision(divisions().length > 0);
                });
            } else {
                divisions([]);
                return canSelectDivision(false);
            }

        }

        function cmbDivisionChange() {

            if (selectedDivision() != null) {
                return dataContext.getDivisionStaff(staff, selectedDivision()).then(function () {
                    canSelectStaff(staff().length > 0);
                });
            } else {
                staff([]);
                return canSelectStaff(false);
            }
        }

        function getDetails(objModel) {

            return baseEditor.objEdit(new leaveSummaryTest(objModel.staffId())).then(function () {
                //history.go(0);
            });

        }

        function cancel() {
            return helpers.returnToPrevPage();
        }

        function download(strData, strFileName, strMimeType) {
            var D = document, A = arguments, a = D.createElement("a"),
                 d = A[0], n = A[1], t = A[2] || "application/pdf";

            var newdata = "data:" + strMimeType + ";base64," + escape(strData);

            //build download link:
            a.href = newdata;

            if ('download' in a) {
                a.setAttribute("download", n);
                a.innerHTML = "downloading...";
                D.body.appendChild(a);
                setTimeout(function () {
                    var e = D.createEvent("MouseEvents");

                    e.initMouseEvent("click", true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null
                );
                    a.dispatchEvent(e);
                    D.body.removeChild(a);
                }, 66);

                return true;
            };

        }

        function exportToFile() {
   
            var startDate = new Date($("#startDate").datepicker("getDate"));
            var endDate = new Date($("#endDate").datepicker("getDate"));
            var company = selectedCompany() == null ? null : selectedCompany();
            var division = selectedDivision() == null ? null : selectedDivision();
            var staffMember = selectedStaff() == null ? null : selectedStaff();
            var leaveType = selectedLeave() == null ? null : selectedLeave();
         
            return dataContext.exportPayrollLeaveBalances(csvFile, new Date(startDate).toString(), new Date(endDate).toString(), company, division, staffMember, leaveType).then(function () {
                if (result) {
                    download(csvFile().$value, "LeaveBalances", 'application/vnd.ms-excel');
                }

                //var test = leaverecords();
                //companyList(buildSummary(leaverecords));
                //showSummaryFooter(leaverecords().length > 0);
                //canSearch(true);
                //searching(false);
                //return dataContext.exportPayrollLeaveBalances(csvFile, leaverecords()).then(function(result) {
                   
                //});


            });
        }

        var vm = {
            activate: activate,
            viewAttached: viewattached,
            summary: summary,
            leaverecords: leaverecords,
            authorised: authorised,
            canSearch: canSearch,
            searching: searching,
            search: getCompanyLeave,
            selectedCompany: selectedCompany,
            selectedDivision: selectedDivision,
            selectedStaff: selectedStaff,
            selectedLeave: selectedLeave,
            canSelectDivision: canSelectDivision,
            canSelectStaff: canSelectStaff,
            cmbCompanyChange: cmbCompanyChange,
            cmbDivisionChange: cmbDivisionChange,
            companies: companies,
            divisions: divisions,
            staff: staff,
            carries: carriers,
            leaveTypes: leaveTypes,
            companyList: companyList,
            totalAmount: summaryTotalAmount,
            totalPercentage: summaryTotalPercentage,
            sumFooter: showSummaryFooter,
            cancel: cancel,
            getDetails: getDetails,
            exportToFile: exportToFile
        };
        return vm;
    });

