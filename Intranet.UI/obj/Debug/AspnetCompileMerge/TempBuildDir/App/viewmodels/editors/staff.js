define(['services/datacontext', 'config', 'viewmodels/editors/leavecounter', 'viewmodels/editors/baseeditor', 'services/model', 'durandal/app', 'viewmodels/editors/workhours'],
    function (datacontext, config, leaveEditor, baseEditor, model, app, workHours) {
        var isAdmin = ko.observable();
        var branches = ko.observableArray();
        var printers = ko.observableArray();
        var staffMembers = ko.observableArray();
        var phoneDetails = ko.observable();
        var ringers = ['Ringer1', 'Ringer2', 'Ringer3', 'Ringer4', 'Ringer5', 'Ringer6', 'Ringer7', 'Ringer8', 'Ringer9'];
        var staffMem = ko.observable();


        function activate() {
            return datacontext.getAllStaff(staffMembers);
        }

        function viewAttached() {
            var dateToday = new Date(staffMembers()[0].displayStaffJoinDate());
            $("#staffJoinDate").datepicker({
                dateFormat: "dd-mm-yy"
            }).datepicker("setDate", dateToday);
        }

        function init(defBranch, staff) {
            staffMem(staff);
            return datacontext.getIsAdmin(isAdmin).then(function () {
                return datacontext.getPrinters(printers).then(function () {

                    if (staff != null) {

                        var phoneDet = staffMem().phoneDetails();
                        if (typeof phoneDet === "undefined" || phoneDet == null) {
                            var defData = {
                                staffId: staffMem().staffId(),
                                recordStatus: datacontext.recStatus.statusActive,
                                staffPhoneRinger: 1
                            };
                            phoneDetails(datacontext.manufactureEntity('StaffPhoneDetailModel', defData, false));
                        } else {
                            phoneDetails(phoneDet);
                        }

                    }

                    if (typeof defBranch === "undefined")
                        return datacontext.getBranchlookup(branches);

                    return branches(defBranch);
                });
            });
        }

        function save(objModel) {
            var tempjoinDate = new Date($("#staffJoinDate").datepicker("getDate")).setHours(2, 0, 0, 0);
            objModel.staff.staffJoinDate(new Date(tempjoinDate));
            this.clickSave();
            return;
        }

        //Leave counter Accum
        //========================================>
        function addLeaveAccum() {
            var startDate = new Date();
            var endDate = new Date().setYear(startDate.getFullYear() + 3);
            var defCounter = {
                staffId: staffMem().staffId(),
                startPeriod: startDate,
                endPeriod: new Date(endDate),
                recordStatus: datacontext.recStatus.statusActive,
            };
            return getLeaveAccumView(datacontext.manufactureEntity(model.entityNames.staffLeaveCounterName, defCounter, true));
        }

        function editLeaveAccum(objmodel) {
            return getLeaveAccumView(objmodel);
        }

        function getLeaveAccumView(accum) {
            return baseEditor.objEdit(new leaveEditor(accum));
        }

        function deleteLeaveCounter(objModel) {
            objModel.canRemove(false);
            objModel.recordStatus(datacontext.recStatus.statusDelete);
        }
        //=======================================>

        //Working hours
        //========================================>
        function deleteDayWorkHours(objModel) {
            return app.showMessage("Are you sure you want to delete staff working hours?", "Delete Working Hours", ["Yes", "No"]).then(function (data) {
                if (data == "No")
                    return;

                objModel.recordStatus(datacontext.recStatus.statusDelete);
                return;
            });
        }

        function updateDayWorkHours(objModel) {
            return getWorkHoursView(objModel, true);
        }

        function addStaffWorkHours() {
            var whDef = {
                dayId: 100,
                staffId: staffMem().staffId(),
                dayTimeStart: new Date(0, 0, 0, 08, 0, 0, 0),
                dayTimeEnd: new Date(0, 0, 0, 17, 0, 0, 0),
                dayLunchLength: 60
            };
            var wh = datacontext.manufactureEntity(model.entityNames.StaffHoursModelName, whDef, false);
            return getWorkHoursView(wh, false);
        }

        function getWorkHoursView(wh, edit) {
            return baseEditor.objEdit(new workHours(wh, edit));
        }

        var loadDivisions = function () {
            datacontext.getBranchlookup(branches);
            var test = branches();
            return test;
        }
        var close = function () {
            window.location('#/view_directory');
            return test;
        }
        //========================================>
        var vm = function (staff, defBranch) {
            this.activate = activate;
            this.viewAttached = viewAttached;
            this.phoneDetails = phoneDetails;
            init(defBranch, staff);
            this.branches = branches;
            this.staff = staffMem();
            this.printers = printers;
            this.staffMembers = staffMembers;
            this.ringers = ko.observableArray(ringers);
            this.save = save;
            this.addLeaveAccum = addLeaveAccum;
            this.deleteLeaveCounter = deleteLeaveCounter;
            this.editLeaveAccum = editLeaveAccum;

            this.deleteDayWorkHours = deleteDayWorkHours;
            this.updateDayWorkHours = updateDayWorkHours;
            this.addStaffWorkHours = addStaffWorkHours;
            this.loadDivisions = loadDivisions;
            this.clickCancel = close;
            
        };
        return vm;
    });