define(['services/datacontext', 'services/helpers','durandal/app'],
    function (datacontext, helpers,app) {
        var simultApps    = ko.observableArray([]);
        var leaveApp      = ko.observable();
        var canApprove    = ko.observable();
        var manager       = ko.observable();
        var isEditing     = ko.observable(false);
        var leaveDays     = ko.observable(1);
        var leaveTypes    = ko.observableArray();
        var selectedLeave = ko.observable();
        var man1Id, man2Id, manId, initialLeaveDays, initSickDays;

        function activate() {
            initialLeaveDays = leaveApp().staffMember().DaysDue();
            initSickDays     = leaveApp().staffMember().SickDaysAvilable();
            return datacontext.getCurrentUser(manager).then(function () {
                    man1Id = leaveApp().staffMember().staffManager1Id();
                    man2Id = leaveApp().staffMember().staffManager2Id();
                    manId  = manager().staffId();
                    canApprove((man1Id == manId || man2Id == manId) && (leaveApp().leaveStatus() == datacontext.leaveStatusEnum.pending) && isEditing() == false);
                }).then(function () {
                    return datacontext.leaveTypes(leaveTypes).then(function () {
                            selectedLeave(leaveDays.leaveStatus);
                        });
                });
        }

        function viewAttached() {
            $("#cbxStaff").prop("disabled", true);

            leaveDays(helpers.calculateLeaveDays(leaveApp().leaveDateStart(), leaveApp().leaveDateEnd()));

            if (!isEditing()) return;
            configureStartDatePicker();
            configureEndDatePicker();
        }

        function configureStartDatePicker() {
            $("#leaveStart").datepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd mm yy',
                    showOtherMonths: false,
                    selectOtherMonths: false,
                    onClose: function (selectedDate) {
                        var tempSelectedDate = new Date(selectedDate);
                        $("#leaveEnd").datepicker("option", "minDate", tempSelectedDate);
                        $("#leaveEnd").datepicker("setDate", tempSelectedDate);

                        // a person can take a minimum of a day's leave
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", isEditing() ? new Date(leaveApp().leaveDateStart()) : new Date());
            $("#leaveStart").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function configureEndDatePicker() {
            $("#leaveEnd").datepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd mm yy',
                    onClose: function () {
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", leaveApp().leaveDateEnd());
            $("#leaveEnd").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function init(leaveId, editMode) {
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
            else Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
                    this.clickCancel();
                });

            if (typeof modelObj.leave().approvedBy1() !== "undefined" && typeof modelObj.leave().approvedBy1() !== "undefined") {
                modelObj.leave().leaveStatus(datacontext.leaveStatusEnum.approved);
                modelObj.leave().leaveComments(buildComments(modelObj.leave));
                return Q.fcall(function () {
                    save().then(function() {
                        datacontext.emailLeaveApproved(modelObj.leave().leaveId());
                        app.showMessage("Leave application approved. An approval email has been sent to the applicant", 'Success', ['Close']);
                        return this.modal.close();
                    });
                });
            }
        }

        function save() {
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
            var start = new Date($("#leaveStart").datepicker("getDate"));
            var end   = new Date($("#leaveEnd").datepicker("getDate"));

            objModel.leave().staffMember().DaysDue(parseInt(initialLeaveDays) - parseInt(leaveDays()));
            objModel.leave().leaveDateStart(start);
            objModel.leave().leaveDateEnd(end);
            objModel.leave().leaveComments(buildComments(objModel.leave));
            return datacontext.changesSave().then(function () {
                datacontext.emailLeaveApplication(objModel.leave().leaveId()).then(function () {
                    Q.resolve(app.showMessage("Your have successfuly updated your leave application", "Leave Application", ['Close'])).then(function (data) {
                        router.navigateTo('#/view_staffmenu');
                        return;
                    });
                });
            });
        }

        ///recursive function
        ///This is a work around to get the difference in between days
        function calculateLeaveDays() {
            var start = new Date($("#leaveStart").datepicker("getDate"));
            var end = new Date($("#leaveEnd").datepicker("getDate"));
            leaveDays(helpers.calculateLeaveDays(start, end));
        }

        var vm = function (leave, edit) {
            init(leave.leaveId(), edit);
            leaveApp(leave);

            this.activate = activate;
            this.viewAttached = viewAttached;

            this.leaveTypes = leaveTypes;
            this.selectedLeave = selectedLeave;
            this.leaveDays = leaveDays;
            this.leave = leaveApp;
            this.simltApps = simultApps;

            this.canApprove = canApprove;
            this.isEditing = isEditing;

            this.approveLeave = approveLeave;
            this.declineLeave = declineLeave;
            this.updateLeaveApplication = updateLeaveApplication;
        };
        return vm;
    });