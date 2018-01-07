define(['services/datacontext', 'services/model', 'durandal/plugins/router', 'durandal/app', 'services/helpers'],
    function (dataContext, model, router, app, helper) {
        var staff = ko.observable();
        var leaveTypes = ko.observableArray();
        var leaveApplication = ko.observable();
        var leaveDays = ko.observable();
        var displayLeaveHours = ko.observable();
        var defLeaveType = dataContext.leaveTypeEnum.annual;
        var selectedLeave = ko.observable(defLeaveType);
        var initialLeaveDays;
        var initSickLEaveDays;
        var daysAllowed = ko.observable();
        var canSave = ko.observable(true);
        var showWarning = ko.observable(false);
        var complusoryComment = ko.observable(false);

        function activate() {
            return dataContext.getCurrentUserWithLeaveData(staff).then(function () {
                initialLeaveDays = staff().DaysDue();
                initSickLEaveDays = staff().SickDaysAvilable();
                var leaveInit = {
                    leaveStatus: dataContext.leaveStatusEnum.pending,
                    staffId: staff().staffId(),
                    leaveRequestDate: new Date(),
                    recordStatus: dataContext.recStatus.statusActive,
                };

                leaveApplication(dataContext.manufactureEntity(model.entityNames.staffLeaveName, leaveInit, true));
                return dataContext.leaveTypes(leaveTypes).then(function () {
                    return canSave(true);
                });
            });
        }

        function viewAttached() {
            $("#daysAllowed").val(initialLeaveDays);
            configureStartDatePicker();
            configureEndDatePicker();
            showWarning((daysAllowed() - leaveDays()) < 0);
            complusoryComment(leaveApplication().leaveType() === dataContext.leaveTypeEnum.special);
        }

        function configureStartDatePicker() {
            //$("#leaveStart").datetimepicker({
            //    beforeShowDay: $.datepicker.noWeekends,
            //    });
            $("#leaveStart").datetimepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd-mm-yy',
                    showOtherMonths: false,
                    selectOtherMonths: false,
                    onClose: function (selectedDate) {
                        var tempSelectedDate = $("#leaveStart").datepicker("getDate");

                        //var tempSelectedDate = new Date(selectedDate);
                        $("#leaveEnd").datepicker("option", "minDate", tempSelectedDate);
                        $("#leaveEnd").datepicker("setDate", tempSelectedDate);

                        // a person can take a minum of a day's leave
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", new Date());
            $("#leaveStart").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function configureEndDatePicker() {
            //$("#leaveEnd").datetimepicker({
            //    beforeShowDay: $.datepicker.noWeekends,
            //    });
            $("#leaveEnd").datetimepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd-mm-yy',
                    onClose: function (selectedDate) {
                        calculateLeaveDays(1);
                    }
                }
            ).datepicker("setDate", new Date());
            $("#leaveEnd").datepicker("option", "minDate", new Date());
            $("#leaveEnd").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function onLeaveTypeChange(objmodel) {
            complusoryComment(leaveApplication().leaveType() === dataContext.leaveTypeEnum.special);
            switch (leaveApplication().leaveType()) {
                case dataContext.leaveTypeEnum.annual:
                    daysAllowed(staff().DaysDue());
                    break;
                case dataContext.leaveTypeEnum.sick:
                    daysAllowed(staff().SickDaysAvilable());
                    break;
                case dataContext.leaveTypeEnum.offsite:
                    daysAllowed(365);//since at work
                    break;
                case dataContext.leaveTypeEnum.study:
                    daysAllowed(16);//Due to timetable and the number of courses  this will be a 
                    break;
                case dataContext.leaveTypeEnum.unpaid:
                    daysAllowed(31); // Since a person can not leave more than two months without getting paid
                    break;
                case dataContext.leaveTypeEnum.family:
                    daysAllowed(3); // Since a person can not leave more than two months without getting paid
                    break;
                case dataContext.leaveTypeEnum.special:
                    complusoryComment(true);
                    daysAllowed(365);
                    break;
              
                default:
                    daysAllowed(0);
                    break;
            }
            $("#daysAllowed").val(daysAllowed());
            showWarning((daysAllowed() - leaveDays()) < 0);


        }
         
        function save(objModel) {
            if (leaveApplication().leaveType() === dataContext.leaveTypeEnum.special && $("#txtComment").val() === "" ) {
                return  Q.resolve(app.showMessage("A reason for applying for special leave is compulsory for record purposes. Please add a comment.", "Special Leave Application", ['Close'])).then(function (data) {
                });
            }
                canSave(false);

                var start = $("#leaveStart").val();
                start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

                var end = $("#leaveEnd").val();
                end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

                switch (leaveApplication().leaveType()) {
                    case dataContext.leaveTypeEnum.annual:
                        objModel.staff().DaysDue(parseInt(initialLeaveDays) - parseInt(leaveDays()));
                        break;
                    case dataContext.leaveTypeEnum.sick:
                        objModel.staff().SickDaysAvilable(parseInt(initSickLEaveDays) - parseInt(leaveDays()));
                        break;
                    case dataContext.leaveTypeEnum.offsite:
                        objModel.leaveApplication().leaveStatus(dataContext.leaveStatusEnum.approved);
                }
                objModel.leaveApplication().leaveDateStart(start);
                objModel.leaveApplication().leaveDateEnd(end);
            //Check if the data being saved is not a replica of whats in the database
            //if the dates are the same, verify that the date or tiime is greater than the previous
                return dataContext.changesSave().then(function () {
                    dataContext.emailLeaveApplication(objModel.leaveApplication().leaveId()).then(function () {
                        Q.resolve(app.showMessage("Your have successfuly applied for a leave application", "Leave Application", ['Close'])).then(function (data) {
                            router.navigateTo('#/view_staffmenu');
                            return;
                        });
                        
                    });
                });
        }

        function cancel() {
            dataContext.changesCancel();
            router.navigateTo('#/view_staffmenu');
        }

        ///recursive function
        ///This is a work around to get the difference in between
        function calculateLeaveDays() {
            leaveDays(helper.calculateLeaveDays($("#leaveStart").datepicker("getDate"), $("#leaveEnd").datepicker("getDate")));

            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            displayLeaveHours(helper.calculateLeaveHours(start, end));
            showWarning((daysAllowed() - leaveDays()) < 0);
        }
         
        var vm = {
            activate: activate,
            viewAttached: viewAttached,

            onLeaveTypeChange: onLeaveTypeChange,
            staff: staff,
            leaveTypes: leaveTypes,
            leaveApplication: leaveApplication,

            daysAllowed: daysAllowed,
            selectedLeave: selectedLeave,
            leaveDays: leaveDays,

            canSave: canSave,
            save: save,
            cancel: cancel,
            showWarning: showWarning,
            complusoryComment: complusoryComment,
            displayLeaveHours: displayLeaveHours
             

        };
        return vm;
    });