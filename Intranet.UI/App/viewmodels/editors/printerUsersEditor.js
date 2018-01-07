define(['services/datacontext', 'durandal/app'],
    function (dataContext, app) {
        var thePrinter = ko.observable();
        var staff = ko.observable();

        var selectedCompany = ko.observable();
        var selectedDivision = ko.observable();
        var selectedStaff = ko.observable();

        var companies = ko.observableArray();
        var divisions = ko.observableArray();
        var leaveTypes = ko.observableArray();

        var canSelectDivision = ko.observable(false);
        var canSelectStaff = ko.observable(false);
        var divisionStaff = ko.observableArray();

        var selectedUsers = ko.observableArray();

        function activate() {

            return dataContext.leaveTypes(leaveTypes).then(function () {
                return dataContext.getBranches(companies).then(function () {
                    var test = companies();
                });
            });
        }

        function viewAttached() {
        }

        function sortStaff() {
            staff().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

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
                    sortStaff();
                    canSelectStaff(staff().length > 0);
                    for (var i = 0; i < staff().length; i++) {
                        if (staff()[i].staffDefaultPrinter() === thePrinter().printerId()) {
                            selectedUsers.push(staff()[i]);
                        }
                    }
                    divisionStaff(staff);

                });
            } else {
                staff([]);
                return canSelectStaff(false);
            }
        }

        function init(print) {
            thePrinter(print);
            selectedUsers([]);
        }

        function saveDefaults(objModel) {
            // var test = objModel;
            // if (selectedUsers().length > 0) {

            //return Q.resolve(app.showMessage("Please NOTE. No staff member in this division has a default printer. Are you sure you want to save?", "Printer users", ['No', 'Yes'])).then(function (data) {
            // if (data == "No") {
            //     return;
            // } else {
            for (var i = 0; i < staff().length; i++) {
                for (var j = 0; j < selectedUsers().length; j++) {
                    if (selectedUsers()[j].staffId() === staff()[i].staffId()) {
                        staff()[i].staffDefaultPrinter(thePrinter().printerId());
                        break;
                    } else {
                        staff()[i].staffDefaultPrinter(null);
                    }
                }
            }
            this.clickSave();
            companies([]);
            selectedUsers([]);
            this.modal.close();
            //   }
            // });
            // } else {
            Q.resolve(app.showMessage("Default printer set.", "Printer users", ['Close'])).then(function (data) {
                this.modal.close();

            });
            // }
        }

        function closeWindow() {
            companies([]);
            selectedUsers([]);
            this.modal.close();
        }

        var vm = function (print) {
            this.thePrinter = print;
            this.activate = activate;
            this.viewAttached = viewAttached;
            this.saveDefaults = saveDefaults;
            this.closeWindow = closeWindow;
            this.staff = staff;
            this.cmbCompanyChange = cmbCompanyChange;
            this.cmbDivisionChange = cmbDivisionChange;
            this.companies = companies;
            this.divisions = divisions;
            this.selectedCompany = selectedCompany;
            this.selectedDivision = selectedDivision;
            this.selectedStaff = selectedStaff;
            this.canSelectDivision = canSelectDivision;
            this.canSelectStaff = canSelectStaff;
            this.divisionStaff = divisionStaff;
            this.selectedUsers = selectedUsers;
            init(print);
        };

        return vm;
    });