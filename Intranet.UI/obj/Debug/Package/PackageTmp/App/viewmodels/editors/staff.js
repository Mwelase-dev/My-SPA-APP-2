define(['services/datacontext', 'config', 'viewmodels/editors/leavecounter', 'viewmodels/editors/baseeditor', 'services/model', 'durandal/app', 'viewmodels/editors/workhours'],
    function (datacontext, config, leaveEditor, baseEditor, model, app, workHours) {
        var isAdmin = ko.observable();
        var branches = ko.observableArray();
        var printers = ko.observableArray();
        var staffMembers = ko.observableArray();
        var phoneDetails = ko.observable();
        var ringers = ['Ringer1', 'Ringer2', 'Ringer3', 'Ringer4', 'Ringer5', 'Ringer6', 'Ringer7', 'Ringer8', 'Ringer9'];
        var staffMem = ko.observable();
        var divisionId = ko.observable();
        var selectedStaffDivision = ko.observable();
        var selectedStaffDivisionId = ko.observable();
        var selectedStaffBranchId = ko.observable();

        var staffJoinDate = ko.observable();
        var cyclestart = ko.observable();
        var cycleend = ko.observable();
        var sickcyclestart = ko.observable();
        var sickcycleend = ko.observable();
        var savedStaffDivision = ko.observable();

        var clockDevices = ko.observableArray();



        function activate() {

            return datacontext.getAllStaff(staffMembers).then(function () {
                var test = $('#divisionsDropd').val();
                var opt = document.querySelector('#select optgroup[label="' + branches().branchName + '"]' + ' [value="' + selectedStaffDivision() + '"]');
                if (opt) {
                    opt.selected = true;
                }
                //$(function () {
                //    $('#divisionsDropd').val(selectedStaffDivision());
                //    //var groupId = "1";
                //    //var value = "70";

                //    //$("#divisionsDropd" + groupId + "' option[value='" + selectedStaffDivision() + "']").prop("selected", true);
                //});
                return datacontext.getClockDevices(clockDevices).then(function () {
                    var test2 = clockDevices();
                });
            });
        }

        function viewAttached() {
            var dateToday = new Date(staffJoinDate());
            var cyclestartdate = new Date(cyclestart());
            var cycleenddate = new Date(cycleend());
            var sickcyclestartdate = new Date(sickcyclestart());
            var sickcycleenddate = new Date(sickcycleend());

            $("#staffJoinDate").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", dateToday);
            $("#CycleStart").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", cyclestartdate);


            $("#CycleEnd").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", cycleenddate);


            $("#StaffJoinDate").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", dateToday);


            $("#SickCycleStart").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", sickcyclestartdate);


            $("#SickCycleEnd").datepicker({
                dateFormat: "yy-mm-dd"
            }).datepicker("setDate", sickcycleenddate);

            //$("select optgroup option:last").attr("selected", "selected");


            //var branch = selectedStaffBranchId();
            //var division = selectedStaffDivisionId();

            //var optgroup = $('select optgroup[label="' + branch + '"]');
            //var option = optgroup.find('option[value="' + division + '"]');

            //option.attr('selected', true);


        }

        function init(defBranch, staff) {
            savedStaffDivision(staff.divisionId());
            selectedStaffDivision(staff.divisionId());
            staffJoinDate(staff.staffJoinDate());
            cyclestart(staff.cycleStart());
            cycleend(staff.cycleEnd());
            sickcyclestart(staff.sickCycleStart());
            sickcycleend(staff.sickCycleEnd());
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
                   // if (typeof defBranch === "undefined") {
                        return datacontext.getBranchlookup(branches).then(function() {
                            $('#divisionsDropd').val(selectedStaffDivision());

                            for (var i = 0; i < branches().length; i++) {
                                for (var j = 0; j < branches()[i].branchDivisions().length ; j++) {
                                    if (branches()[i].branchDivisions()[j].divisionId() === staff.divisionId()) {
                                        selectedStaffDivision(branches()[i].branchDivisions()[j].divisionName());
                                        selectedStaffDivisionId(branches()[i].branchDivisions()[j].divisionId());
                                        var test = selectedStaffDivisionId();
                                        staff.divisionId(branches()[i].branchDivisions()[j].divisionId());
                                        selectedStaffBranchId(branches()[i].branchName());
                                    }
                                }
                            }
                             
                        });
                    //}
                });
            });
        }
        
        function save(objModel) {
            var tempjoinDate = new Date($("#staffJoinDate").datepicker("getDate")).setHours(2, 0, 0, 0);
            objModel.staff.divisionId(selectedStaffDivisionId());
           // objModel.staff.divisionId(divisionId());
            objModel.staff.staffJoinDate(new Date(tempjoinDate));

            //var selectedStaff = ko.observable();
            //return datacontext.getStaffbyId(selectedStaff, objModel.staff.staffId()).then(function () {

            //    var timespan = moment.utc(moment(objModel.staff.staffJoinDate(), "DD/MM/YYYY HH:mm:ss").diff(moment(selectedStaff().staffJoinDate(), "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

                //if (timespan > "") {

                //}

                this.clickSave();
                return;
            //});

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
            return app.showMessage("Are you sure you want to delete leave increment?", "Delete Working Hours", ["Yes", "No"]).then(function (data) {
                if (data == "No")
                    return;
                if (objModel) {
                    objModel.entityAspect.rejectChanges();
                }
                return;
            });
            //objModel.canRemove(false);
            //objModel.recordStatus(datacontext.recStatus.statusDelete);
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

        function cmbDivisionOfStaffMemberChange(objmodel) {
            for (var i = 0; i < branches().length; i++) {
                for (var j = 0; j < branches()[i].branchDivisions().length ; j++) {
                    if (branches()[i].branchDivisions()[j].divisionId() === objmodel.divisionId()) {
                        selectedStaffDivision(branches()[i].branchDivisions()[j].divisionName());
                        selectedStaffDivisionId(branches()[i].branchDivisions()[j].divisionName());
                        var test = selectedStaffDivisionId();
                        savedStaffDivision(objmodel.divisionId());
                    }
                }
            }
        }

        function selectid() {
            var elements = document.getElementById("divisionsDropd").options;

            for (var k = 0; k < elements.length; k++) {
                if (elements[k] == staff.divisionId())
                    elements[k].selected = true;
            }
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
            this.divisionId = divisionId;
            this.selectedStaffDivision = selectedStaffDivision;
            //this.savedStaffDivision = savedStaffDivision;
            this.cmbDivisionOfStaffMemberChange = cmbDivisionOfStaffMemberChange;
            this.clockDevices = clockDevices;
            this.selectedStaffDivisionId = selectedStaffDivisionId;
            this.selectid = selectid;
        };
        return vm;
    });