﻿define(['services/logger', 'services/datacontext','services/helpers'],
    function (logger, dataContext,helpers) {

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

        function getCompanyClockSummaries() {
            leaverecords.removeAll();
            companyList.removeAll();


            searching(true);
            var startDate = new Date($("#startDate").datepicker("getDate"));
            var endDate = new Date($("#endDate").datepicker("getDate"));
            var company = selectedCompany() == null ? null : selectedCompany();
            var division = selectedDivision() == null ? null : selectedDivision();
            var staffMember = selectedStaff() == null ? null : selectedStaff();
            var leaveType = selectedLeave() == null ? null : selectedLeave();


            return dataContext.getCompanyStaffClockingSummaries(leaverecords, new Date(startDate).toString(), new Date(endDate).toString(), company, division, staffMember, leaveType).then(function () {
                companyList(buildSummary(leaverecords));
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
            var compList = [];

            for (var m = 0; m < records().length; m++) {
                totals += records()[m].totalCallCost;
            }
            return compList;
        }

        function cancel() {
            return helpers.returnToPrevPage();
        }

        var vm = {
            activate: activate,
            viewAttached: viewattached,
            summary: summary,
            leaverecords: leaverecords,
            authorised: authorised,
            canSearch: canSearch,
            searching: searching,
            search: getCompanyClockSummaries,
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
                cancel : cancel

        };
        return vm;
    });

