define(['services/datacontext'],
    function (dataContext) {
        var theDevice = ko.observable();
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
                        if (staff()[i].clockDevice() === theDevice().clockDeviceId()) {
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
            theDevice(print);

        }

        function saveDefaults(objModel) {
            var test = objModel;
            for (var i = 0; i < staff().length; i++) {
                for (var j = 0; j < selectedUsers().length; j++) {
                    if (selectedUsers()[j].staffId() === staff()[i].staffId()) {
                        staff()[i].clockDevice(theDevice().clockDeviceId());
                    } else {
                        staff()[i].clockDevice(null);
                    }
                }
            }

            this.clickSave();

            companies([]);
            selectedUsers([]);
            this.modal.close();
        }

        function closeWindow() {
            companies([]);
            selectedUsers([]);
            this.modal.close();
        }

        var vm = function (device) {
            this.theDevice = device;
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
            init(device);
        };

        return vm;
    });