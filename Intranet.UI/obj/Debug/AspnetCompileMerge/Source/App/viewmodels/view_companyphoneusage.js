define(['services/logger', 'services/datacontext'],
    function (logger, dataContext) {

        var phonerecords = ko.observableArray();
        var canSearch = ko.observable(true);
        var searching = ko.observable(false);
        var summary = ko.observable();
        var defStart = new Date();
        var defEnd = new Date();
        defStart.setDate(defStart.getDate() - 7);

        var selectedCompany = ko.observable();
        var selectedDivision = ko.observable();
        var selectedStaff = ko.observable();
        var selectedCarrier = ko.observable();

        var companies = ko.observableArray();
        var divisions = ko.observableArray();
        var staff     = ko.observableArray();
        var carriers  = ko.observableArray();

        var canSelectDivision = ko.observable(false);
        var canSelectStaff = ko.observable(false);

        var companyList = ko.observableArray();
        var summaryTotalAmount = ko.observable();
        var summaryTotalPercentage = ko.observable();
        var showSummaryFooter = ko.observable(false);
        var authorised = ko.observable(false);

        function activate() {
            if (dataContext.UserInRoles(authorised, dataContext.userRoleEnum.manager)) {
                authorised(true);
            }
            return dataContext.getBranches(companies);
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

        function getCompanyCDR() {
            canSearch(false);
            searching(true);
            phonerecords.removeAll();
            companyList.removeAll();
            
            var startDate = new Date($("#startDate").datepicker("getDate"));
            var endDate = new Date($("#endDate").datepicker("getDate"));

            var company     = selectedCompany()  == null ? null : selectedCompany();
            var division    = selectedDivision() == null ? null : selectedDivision();
            var staffMember = selectedStaff()    == null ? null : selectedStaff();
            var carrier     = selectedCarrier()  == null ? null : selectedCarrier();
            return dataContext.getCompanyCDR(phonerecords, new Date(startDate).toString(), new Date(endDate).toString(), company, division, staffMember, carrier).then(function () {
                companyList(buildSummary(phonerecords));
                showSummaryFooter(phonerecords().length > 0);
                canSearch(true);
                searching(false);
            });
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
        
        function buildSummary(records) {
            var totals = 0;
            var percTotal = 0;
            var compList = [];

            for (var m = 0; m < records().length; m++) {
                //get the total call cost for all companies
                totals += records()[m].totalCallCost;

                getCarrier(records()[m]);

                var percValue = calculatePercentage(records()[m].totalCallCost);
                compList.push({ companyName: records()[m].companyName, totalCost: records()[m].displayTotalCallCost, percentage: percValue + "%" });
                percTotal += Number(percValue);
            }
            summaryTotalAmount("R" + totals.toFixed(2));
            summaryTotalPercentage(percTotal);
            return compList;

            function calculatePercentage(callCost) {
                if (totals < 1 || callCost < 1)
                    return 0;
                var value = (callCost / totals);
                return (value * 100).toFixed(2);
            }
        }

        function getCarrier(record) {
            for (var i = 0; i < record.companyDivisions.length; i++) {
                for (var j = 0; j < record.companyDivisions[i].divisionStaff.length; j++) {
                    for (var k = 0; k < record.companyDivisions[i].divisionStaff[j].staffCallRecords.length; k++) {
                        var carrier = record.companyDivisions[i].divisionStaff[j].staffCallRecords[k].destination;
                        if (carriers.indexOf(carrier) < 0) {
                            carriers.push(carrier);
                        }
                    }
                }
            }
        }

        var vm = {
            activate: activate,
            viewAttached      : viewattached,
            summary           : summary,
            phonerecords      : phonerecords,
            authorised        : authorised,
            canSearch         : canSearch,
            searching         : searching,
            search            : getCompanyCDR,
            selectedCompany   : selectedCompany,
            selectedDivision  : selectedDivision,
            selectedStaff     : selectedStaff,
            selectedCarrier   : selectedCarrier,
            canSelectDivision : canSelectDivision,
            canSelectStaff    : canSelectStaff,
            cmbCompanyChange  : cmbCompanyChange,
            cmbDivisionChange : cmbDivisionChange,
            companies         : companies,
            divisions         : divisions,
            staff             : staff,
            carries           : carriers,
            companyList       : companyList,
            totalAmount       : summaryTotalAmount,
            totalPercentage   : summaryTotalPercentage,
            sumFooter         : showSummaryFooter,
        };
        return vm;
    });