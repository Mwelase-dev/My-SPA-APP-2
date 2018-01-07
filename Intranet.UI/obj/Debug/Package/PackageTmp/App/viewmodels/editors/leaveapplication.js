define(['services/datacontext', 'services/helpers', 'durandal/app', 'services/helpers'],
    function (datacontext, helpers, app, helper) {
        var simultApps = ko.observableArray([]);
        var leaveApp = ko.observable();
        var canApprove = ko.observable();
        var manager = ko.observable();
        var isEditing = ko.observable(false);
        var showWarning = ko.observable(false);
        var leaveDays = ko.observable(1);
        var displayLeaveHours = ko.observable(0);
        var leaveTypes = ko.observableArray();
        var selectedLeave = ko.observable();
        var man1Id, man2Id, manId, initialLeaveDays, initSickDays;

        var disabledDays = ko.observableArray();

        function activate() {
            initialLeaveDays = leaveApp().staffMember().daysDue();
            initSickDays = leaveApp().staffMember().sickDaysAvilable();
            return datacontext.getPublicHolidays(disabledDays).then(function () {
                return datacontext.getCurrentUser(manager).then(function () {
                    man1Id = leaveApp().staffMember().staffManager1Id();
                    man2Id = leaveApp().staffMember().staffManager2Id();
                    manId = manager().staffId();
                    canApprove((man1Id == manId || man2Id == manId) && (leaveApp().leaveStatus() == datacontext.leaveStatusEnum.pending) && isEditing() == false);
                }).then(function () {
                    return datacontext.leaveTypes(leaveTypes).then(function () {
                        selectedLeave(leaveDays.leaveStatus);
                    });
                });
            });
        }

        function viewAttached() {
            // $("#cbxStaff").prop("disabled", true);
            leaveDays(leaveApp().displayLeaveDays);
            displayLeaveHours(leaveApp().displayLeaveHours);
            leaveApp().displayLeaveStart = + " " + leaveApp().displayLeaveHours;
            leaveApp().displayLeaveEnd = + " " + leaveApp().displayLeaveHours;
            //leaveDays(helpers.calculateLeaveDays(leaveApp().leaveDateStart(), leaveApp().leaveDateEnd()));
            //if (!isEditing()) return;
            configureStartDatePicker();
            configureEndDatePicker();
        }

        function configureStartDatePicker() {
            $("#leaveStart").datetimepicker(
                {
                    //hourMax: 22,
                    //hourMin: 06,
                    beforeShowDay: noWeekendsOrHolidays,//$.datepicker.noWeekends,
                    dateFormat: 'yy-mm-dd',
                    showOtherMonths: false,
                    selectOtherMonths: false,
                    onClose: function (selectedDate) {
                        //var tempSelectedDate = new Date(selectedDate);
                        //$("#leaveEnd").datepicker("option", "minDate", tempSelectedDate);
                        //$("#leaveEnd").datepicker("setDate", tempSelectedDate);

                        // a person can take a minimum of a day's leave
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", leaveApp().leaveDateStart());
            $("#leaveStart").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function configureEndDatePicker() {
            $("#leaveEnd").datetimepicker(
                {
                    //hourMax: 22,
                    //hourMin: 06,
                    beforeShowDay: noWeekendsOrHolidays,// $.datepicker.noWeekends,
                    dateFormat: 'yy-mm-dd',
                    onClose: function () {
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", leaveApp().leaveDateEnd());
            $("#leaveEnd").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function nationalDays(date) {

            var m = date.getMonth(), d = date.getDate(), y = date.getFullYear();
            for (i = 0; i < disabledDays().length; i++) {
                var holidayDate = new Date(disabledDays()[i]);
                var m2 = holidayDate.getMonth();
                var d2 = holidayDate.getDate();
                var y2 = holidayDate.getFullYear();
                if ((y + '-' + (m + 1) + '-' + d) === (y2 + '-' + (m2 + 1) + '-' + d2)) {
                    //publicHolidayCounter++;
                    return [false];
                }
            }
            return [true];
        }

        function noWeekendsOrHolidays(date) {
            var noWeekend = jQuery.datepicker.noWeekends(date);
            return noWeekend[0] ? nationalDays(date) : noWeekend;
        }


        function init(leaveId, editMode, theleave) {

            var start = new Date(theleave.leaveDateStart());
            start.setHours(start.getHours() - 2);
            start.setMinutes(start.getMinutes());
            theleave.leaveDateStart(start);


            var end = new Date(theleave.leaveDateEnd());
            end.setHours(end.getHours() - 2);
            end.setMinutes(end.getMinutes());
            theleave.leaveDateEnd(end);


            isEditing(editMode);
            return datacontext.getSimultaniousLeaveApps(simultApps, leaveId);
        }

        function approveLeave(modelObj) {
            //check what manager the user is
            //if manager is both manager 1 and manager 2
            if (man1Id == manId && man2Id == manId) {
                modelObj.leave().approvedBy1(manager().staffId());
                modelObj.leave().approvedBy2(manId);
            } else if (man1Id == manId)
                modelObj.leave().approvedBy1(manId);
            else if (man2Id == manId)
                modelObj.leave().approvedBy2(manId);
            //else Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
            //        this.clickCancel();
            //    });

            if (typeof modelObj.leave().approvedBy1() !== "undefined" && typeof modelObj.leave().approvedBy1() !== "undefined") {
                modelObj.leave().leaveStatus(datacontext.leaveStatusEnum.approved);
                modelObj.leave().leaveComments(buildComments(modelObj.leave));
                this.clickSave();
                datacontext.emailLeaveApproved(modelObj.leave().leaveId()).then(function () {;
                    app.showMessage("Leave application approved. An approval email has been sent to the applicant", 'Success', ['Close']);
                });
            }
        }

        function save() {
            return datacontext.changesSave();
        }

        function saveLeaveApplication() {
            return datacontext.changesSave();
        }

        function declineLeave(modelObj) {
            var comments = buildComments(modelObj.leave) + "\n Declined by - " + manager().fullName();
            modelObj.leave().leaveComments(comments);
            modelObj.leave().leaveStatus(datacontext.leaveStatusEnum.declined);
            this.clickSave();
        }

        function buildComments(leave) {
            return leave().leaveComments() + "; Action Reason-'" + " " + $("#actionReason").val() + "'";
        }

        function updateLeaveApplication(objModel) {

            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            start.setHours(start.getHours() + 2);
            start.setMinutes(start.getMinutes());


            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            end.setHours(end.getHours() + 2);
            end.setMinutes(end.getMinutes());


            objModel.leave().staffMember().daysDue(parseInt(initialLeaveDays) - parseInt(leaveDays()));
            objModel.leave().leaveDateStart(start);
            objModel.leave().leaveDateEnd(end);
            objModel.leave().leaveComments(buildComments(objModel.leave));
            this.clickSave();
            //return datacontext.changesSave().then(function () {
            datacontext.emailLeaveApplication(objModel.leave().leaveId()).then(function () {
                Q.resolve(app.showMessage("Your have successfuly updated your leave application", "Leave Application", ['Close'])).then(function (data) {

                });
            });
            //});
        }

        ///recursive function
        ///This is a work around to get the difference in between days
        /*function calculateLeaveDays() {
            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            leaveDays(helpers.calculateLeaveDays(start, end));


            var test = leaveDays();
            if (test === 0) {
                test = test + 1;
                leaveDays([]);
                leaveDays(test);

            } else if (test > 0) {
                test++;
                leaveDays(test);
            }
        }*/

        function calculateLeaveDays() {
            var publicHolidayCounter = 0;
            leaveDays(helper.calculateLeaveDays($("#leaveStart").datepicker("getDate"), $("#leaveEnd").datepicker("getDate")));

            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            displayLeaveHours(helper.calculateLeaveHours(start, end));

            while (end.toDateString() != start.toDateString()) {
                var date = new Date(start);
                var m = date.getMonth(), d = date.getDate(), y = date.getFullYear();
                for (var i = 0; i < disabledDays().length; i++) {
                    var holidayDate = new Date(disabledDays()[i]);
                    var m2 = holidayDate.getMonth();
                    var d2 = holidayDate.getDate();
                    var y2 = holidayDate.getFullYear();
                    if ((y + '-' + (m + 1) + '-' + d) === (y2 + '-' + (m2 + 1) + '-' + d2)) {
                        publicHolidayCounter++;
                    }
                }
                start = new Date(moment(start).add('d', 1));
            }

            if (displayLeaveHours() === "08:00:00") {
                displayLeaveHours(0);
                var test = leaveDays();
                if (test === 0) {
                    test = test + 1;
                    leaveDays([]);
                    leaveDays(test);

                } else if (test > 0) {
                    test++;
                    leaveDays(test);
                }
            }

        }

        function calculateLeaveDaysForHalfDayStaff() {
            leaveDays(helper.calculateLeaveDays($("#leaveStart").datepicker("getDate"), $("#leaveEnd").datepicker("getDate")));

            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            displayLeaveHours(helper.calculateLeaveHours(start, end));

            if (displayLeaveHours() === "05:00:00") {
                displayLeaveHours(0);
                var test = leaveDays();
                if (test === 0) {
                    test = test + 1;
                    leaveDays([]);
                    leaveDays(test);

                } else if (test > 0) {
                    test++;
                    leaveDays(test);
                }
            }

        }

        function close() {
            var test = leaveApp();
            if (isEditing()) {
                this.clickCancel();
            } else {

                var start = new Date(leaveApp().leaveDateStart());
                start.setHours(start.getHours() + 2);
                start.setMinutes(start.getMinutes());
                leaveApp().leaveDateStart(start);


                var end = new Date(leaveApp().leaveDateEnd());
                end.setHours(end.getHours() + 2);
                end.setMinutes(end.getMinutes());
                leaveApp().leaveDateEnd(end);
                this.modal.close();
            }
        }


        var vm = function (leave, edit) {
            init(leave.leaveId(), edit, leave);
            showWarning(leave.staffMember().daysDue() < leave.displayLeaveDays);
            leaveApp(leave);

            this.activate = activate;
            this.viewAttached = viewAttached;

            this.leaveTypes = leaveTypes;
            this.selectedLeave = selectedLeave;
            this.leaveDays = leaveDays;
            this.displayLeaveHours = displayLeaveHours;
            this.leave = leaveApp;
            this.simltApps = simultApps;

            this.canApprove = canApprove;
            this.isEditing = isEditing;

            this.approveLeave = approveLeave;
            this.declineLeave = declineLeave;
            this.updateLeaveApplication = updateLeaveApplication;
            this.showWarning = showWarning;
            this.close = close;
        };
        return vm;
    });