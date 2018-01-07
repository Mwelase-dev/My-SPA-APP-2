define(['services/datacontext', 'config', 'viewmodels/editors/leavecounter', 'viewmodels/editors/baseeditor', 'services/model', 'durandal/app', 'viewmodels/editors/workhours'],
    function (datacontext, config, leaveEditor, baseEditor, model, app, workHours) {
        var isAdmin = ko.observable();
        var branches = ko.observableArray();
        var printers = ko.observableArray();
        var staffMembers = ko.observableArray();
        var phoneDetails = ko.observable();
        var ringers = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        var staffMem = ko.observable();

        function activate() {
            return datacontext.getAllStaff(staffMembers).then(function () {
                return datacontext.getCurrentUser1(staffMem).then(function () {
                    return datacontext.getDivisionLookUp(branches);
                });
            });
        }


        function viewAttached() {

            var tempDate = new Date(staffMembers()[0].displayStaffJoinDate()).setHours(2, 0, 0, 0, 0);

            $("#staffJoinDate").datepicker("option", "dateFormat", config.localDateformat);
            $("#staffJoinDate").datepicker();
            $("#staffJoinDate").datepicker("setDate", new Date(tempDate));
        }

        //function activate() {
        //    return datacontext.getAllStaff(staffMembers).then(function () {
        //    });
        //}

        //function viewAttached() {

        //    var tempDate = new Date(staffMem().displayStaffJoinDate()).setHours(2, 0, 0, 0, 0);

        //    $("#staffJoinDate").datepicker("option", "dateFormat", config.localDateformat);
        //    $("#staffJoinDate").datepicker();
        //    $("#staffJoinDate").datepicker("setDate", new Date(tempDate));
        //}

        function init(defBranch, staff) {

            staffMem(staff);
            return datacontext.getIsAdmin(isAdmin).then(function () {
                return datacontext.getPrinters(printers).then(function () {

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

                    if (typeof defBranch === "undefined")
                        return datacontext.getBranchlookup(branches);

                    var data = [];

                    data.push(defBranch);

                    return branches(data);


                });
            });
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
                dayTimeStart: new Date(),
                dayTimeEnd: new Date(),
                dayLunchLength: 60
            };

            var wh = datacontext.manufactureEntity(model.entityNames.StaffHoursModelName, whDef, false);

            return getWorkHoursView(wh, false);
        }

        function getWorkHoursView(wh, edit) {
            return baseEditor.objEdit(new workHours(wh, edit));
        }

        //========================================>

        var vm = function (staff, defBranch) {
            this.activate = activate;
            this.viewAttached = viewAttached;
            this.phoneDetails = phoneDetails;
            init(defBranch, staff);
            this.branches = branches;
            this.staff = staffMem;
            this.printers = printers;
            this.staffMembers = staffMembers;
            this.ringers = ko.observableArray(ringers);

            //this.staffDev = staffDev;

            this.addLeaveAccum = addLeaveAccum;
            this.deleteLeaveCounter = deleteLeaveCounter;
            this.editLeaveAccum = editLeaveAccum;

            this.deleteDayWorkHours = deleteDayWorkHours;     
            this.updateDayWorkHours = updateDayWorkHours;     
            this.addStaffWorkHours  = addStaffWorkHours;     
            this.branches           = branches;     
        };
        return vm;
    });