define(['services/datacontext', 'services/model', 'durandal/plugins/router', 'durandal/app', 'services/helpers'],
    function (dataContext, model, router, app, helper) {
        var allStaff = ko.observableArray();
        var staff = ko.observable();
        var leaveTypes = ko.observableArray();
        var leaveApplication = ko.observable();
        var leaveDays = ko.observable();
        var displayLeaveHours = ko.observable();
        var defLeaveType = dataContext.leaveTypeEnum.annual;
        var selectedLeave = ko.observable(defLeaveType);
        var initialLeaveDays;
        var initialLeaveHours;
        var initSickLEaveDays;
        var daysAllowed = ko.observable();
        var canSave = ko.observable(true);
        var showWarning = ko.observable(false);
        var complusoryComment = ko.observable(false);
        var hideDaysDue = ko.observable(true);
        var hideHoursDue = ko.observable(true);
        var hideTime = ko.observable(false);
        var hideDate = ko.observable(true);
        var hideHours = ko.observable(true);
        var hideDays = ko.observable(true);
        var clockDate = ko.observable();
        var defStartDate = new Date();
        var managerandstaff = ko.observableArray();
        var managerandstaffbeneath = ko.observableArray();
        var manager = ko.observable();
        var managerandstaffarray = [];
        var userRoles = ko.observableArray();
        var isPartOfAssistants = ko.observable(false);
        var isPartOfManagers = ko.observable(false);
        var isPartOfHr = ko.observable(false);
        var isNormal = ko.observable(false);
        var selectedStaffId = ko.observable();
        var staffToSelect = ko.observableArray();

        var automaticAllDay = ko.observable(false);
        var disableTime = ko.observable(false);

        var defaultStartTime = ko.observable();
        var defaultEndTime = ko.observable();

        var hoursAlreadyWorked = ko.observable();
        var staffClockData = ko.observableArray();

        var disabledDays = ko.observableArray();
        

        function setDefaultTimesOfSelectedStaff() {
            var d = new Date();
            var day = d.getDay();

            for (var i = 0; i < staff().staffHoursData().length; i++) {
                if (d.getDay() === 6 || d.getDay() === 0) {
                    day = 1;
                }
                if (staff().staffHoursData()[i].dayId() === day) {
                    defaultStartTime(staff().staffHoursData()[i].displayStartTime());
                    defaultEndTime(staff().staffHoursData()[i].displayEndTime());
                    break;
                }
            }
            return true;
        }

        function setDefaultTimes() {
            var d = new Date();
            var day = d.getDay();

            for (var i = 0; i < staff().staffHoursData().length; i++) {
                if (d.getDay() === 6 || d.getDay() === 0) {
                    day = 1;
                }
                if (staff().staffHoursData()[i].dayId() === day) {
                    defaultStartTime(staff().staffHoursData()[i].displayStartTime());
                    defaultEndTime(staff().staffHoursData()[i].displayEndTime());
                    break;
                }
            }
            return true;
        }

        function getDefaultDateTime(date) {

            clockDate(moment(date).format('YYYY MMMM Do'));
        }

        function sortStaff() {
            allStaff().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

        function sortManagerAndTheirStaff() {
            managerandstaffbeneath().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

        function activate() {
            var start = new Date();
            start = moment(start).format('YYYY-MM-DD');

            var end = new Date();
            end = moment(end).format('YYYY-MM-DD');


            defStartDate = new Date(defStartDate.setDate(defStartDate.getUTCDate() - 7));
            //return dataContext.allStaff(allStaff).then(function () {

            return dataContext.getCurrentUserWithLeaveData(staff).then(function () {
                selectedStaffId(staff().staffId());
                return dataContext.getCurrentUserRoles(userRoles).then(function () {
                    isPartOfManagers(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.manager));
                    isPartOfAssistants(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.assistants));
                    isPartOfHr(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.humanResource));
                    isNormal(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.humanResource) || dataContext.isInRole(userRoles(), dataContext.userRoleEnum.assistants));
                    initialLeaveDays = staff().daysDue();
                    initialLeaveHours = staff().hoursDue();
                    initSickLEaveDays = staff().sickDaysAvilable();

                    var leaveInit = {
                        leaveStatus: dataContext.leaveStatusEnum.pending,
                        staffId: staff().staffId(),
                        leaveRequestDate: new Date(),
                        recordStatus: dataContext.recStatus.statusActive,
                    };
                    leaveApplication(dataContext.manufactureEntity(model.entityNames.staffLeaveName, leaveInit, true));
                    return dataContext.getPublicHolidays(disabledDays).then(function () {
                        return dataContext.leaveTypes(leaveTypes).then(function () {
                        setDefaultTimes();
                        staffToSelect(staff());//isPartOfManagers

                        if (isPartOfHr()) {
                            staffToSelect([]);
                            return dataContext.allStaff(allStaff).then(function () {
                                sortStaff();
                                staffToSelect(allStaff());
                                var test2 = staff();
                            });

                        } else if (isPartOfManagers()) {
                            staffToSelect([]);
                            return dataContext.getStaffByManager(managerandstaffbeneath, staff).then(function () {
                                sortManagerAndTheirStaff();
                                staffToSelect(managerandstaffbeneath());
                            });
                        } else if (isPartOfAssistants()) {
                            staffToSelect([]);
                            return dataContext.getStaffbyId(managerandstaff, staff().staffManager1Id()).then(function () {
                                staffToSelect.push(managerandstaff());
                                managerandstaff(staff());
                                managerandstaff(managerandstaff());
                                staffToSelect.splice(1, 0, staff());
                                var test = staffToSelect();
                                return dataContext.getStaffbyId(manager, staff().staffManager1Id()).then(function () {
                                    managerandstaff(manager());
                                });
                            });
                        }
                     
                        return canSave(true);
                    });
                });
                });
            });

            //});
        }

        function viewAttached() {
            //$("#daysAllowed").val(initialLeaveDays);
            //$("#hoursAvil").val(initialLeaveHours);
            configureStartDatePicker();
            configureEndDatePicker();
            //configureStartTimePicker();
            //configureEndTimePicker();
            // configureTime();
           configureClockDate();
            showWarning((daysAllowed() - leaveDays()) < 0);
            complusoryComment(leaveApplication().leaveType() === dataContext.leaveTypeEnum.special);
        }

        function configureStartDatePicker() {
            var d = new Date();
            var month = d.getMonth() + 1;
            var day = d.getDate();
            var output = d.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;
            var valueToDisplay = $("#leaveStart").val();
            if ($("#leaveStart").val() != "") {
               
                valueToDisplay = valueToDisplay.split(" ");
                valueToDisplay = valueToDisplay[0];
                output = valueToDisplay;
            }

          
            var defaultStartHour = defaultStartTime();
            defaultStartHour = parseInt(defaultStartHour.substring(0, 2));


            var defaultEndHour = defaultEndTime();
            defaultEndHour = parseInt(defaultEndHour.substr(0, 2));

            var timeworked = hoursAlreadyWorked();
            if (timeworked != null) {
                timeworked = parseInt(timeworked.substring(0, 2));
            }

            if (timeworked > 0) {
                defaultStartHour = defaultStartHour + timeworked;
            }

            //var publicHolidays = holdidays();

            $("#leaveStart").datepicker(
            {
                constrainInput: true,
                beforeShowDay: noWeekendsOrHolidays, //$.datepicker.noWeekends,
                dateFormat: 'yy-mm-dd',
                //timeFormat: 'HH:mm:ss',
                //minTime: '07:00:00',
                pickTime: false,
                pickDate: false,
                hourMax: defaultEndHour,
                hourMin: 06,
                showOtherMonths: true,
                selectOtherMonths: true,
                onClose: function(selectedDate) {
                    var tempSelectedDate = $("#leaveStart").datepicker("getDate");
                    var t2 = selectedDate.split(' ');
                    if (t2.length === 1) {
                        $("#leaveStart").datepicker("option", "minDate", "").val(selectedDate + " " + defaultStartTime());
                        $("#leaveEnd").datepicker("option", "minDate", "").val(selectedDate + " " + defaultEndTime());
                    }


                    // a person can take a minum of a day's leave
                    calculateLeaveDays(1);
                }
            }).datepicker("setDate", new Date());//.val(output+ " " + defaultStartTime()/* */);
            $("#leaveStart").datepicker({ beforeShowDay: $.datepicker.noWeekends, format: 'yyyy-MM-dd hh:mm' });
    

            document.getElementById("toggler").onclick = function () {
                var s = document.getElementById("t3");
                (s.style.display == "none") ? s.style.display = "" : s.style.display = "none";
                setDefaultTimes();
                $("#leaveStart").datepicker("setDate", "").val(output + " " + defaultStartTime());

                if (s.style.display == "") {
                    $("#leaveStart").datepicker("setDate", "").val(output);
                }

                var s2 = document.getElementById("t4");
                (s2.style.display == "none") ? s2.style.display = "" : s2.style.display = "none";
                setDefaultTimes();
               
                $("#leaveEnd").datepicker("option", "minDate", "").val(output + " " + defaultEndTime());

                if (s.style.display == "") {
                    $("#leaveEnd").datepicker("setDate", "").val(output);
                }

                calculateLeaveDays(1);
            };
  
       
        }
       
        function configureEndDatePicker() {
            var d = new Date();
            var month = d.getMonth() + 1;
            var day = d.getDate();
            var output = d.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;
       
            var valueToDisplay = $("#leaveEnd").val();
            if ($("#leaveEnd").val() != "") {

                valueToDisplay = valueToDisplay.split(" ");
                valueToDisplay = valueToDisplay[0];
                output = valueToDisplay;
            }

           
            var defaultStartHour = defaultStartTime();
            defaultStartHour = parseInt(defaultStartHour.substring(0, 2));



            var defaultEndHour = defaultEndTime();
            defaultEndHour = parseInt(defaultEndHour.substr(0, 2));

            var timeworked = hoursAlreadyWorked();
            if (timeworked != null) {
                timeworked = parseInt(timeworked.substring(0, 2));
            }

            if (timeworked > 0) {
                defaultStartHour = defaultStartHour + timeworked;
            }

            $("#leaveEnd").datepicker(
                {
                    //pickTime: pickTimme(),
                    pickTime: false,
                    pickDate: false,
                    constrainInput: true,
                    beforeShowDay: noWeekendsOrHolidays, //$.datepicker.noWeekends,
                    dateFormat: 'yy-mm-dd',
                    //timeFormat: 'HH:mm:ss',
                    //minTime: '17:00:00',
                    hourMax: 23,
                    hourMin: defaultStartHour,
                    onClose: function(selectedDate) {
                        var t2 = selectedDate.split(' ');
                        if (t2.length === 1) {
                            $("#leaveEnd").datepicker("option", "minDate", "").val(selectedDate + " " + defaultEndTime());
                        }
                        if (defaultEndHour > 13) {
                            calculateLeaveDays(1);
                        } else {
                            calculateLeaveDaysForHalfDayStaff(1);
                        }

                    }
                }
            ).datepicker("setDate", new Date());
            $("#leaveEnd").datepicker({ beforeShowDay: $.datepicker.noWeekends, format: 'yyyy-MM-dd hh:mm' });
          calculateLeaveDays(1);
        
        }
    
        function configureClockDate() {
            var defaultStartHour = defaultStartTime();
            defaultStartHour = parseInt(defaultStartHour.substring(0, 2));



            var defaultEndHour = defaultEndTime();
            defaultEndHour = parseInt(defaultEndHour.substr(0, 2));

            $("#clockDateStart").timepicker(
                {
                    hourMax: defaultEndHour,
                    hourMin: 06,
                    onClose: function (selectedDate) {
                         
                        defaultStartTime($("#clockDateStart").val());
                        var t2 = selectedDate.split(' ');
                        if (t2.length === 1) {
                            
                        }
                       calculateLeaveDays(1);
                    }
                }
                );
            var defDate = new Date();
            $("#clockDateStart").timepicker('setTime', defaultStartTime());

            $("#clockDateEnd").timepicker(
                {
                    hourMax: 23,
                    hourMin: defaultStartHour,
                    onClose: function (selectedDate) {
                        defaultEndTime($("#clockDateEnd").val());
                        var t2 = selectedDate.split(' ');
                        if (t2.length === 1) {
                            
                        }
                        if (defaultEndHour > 13) {
                            calculateLeaveDays(1);
                        } else {
                            calculateLeaveDaysForHalfDayStaff(1);
                        }
                    }
                }
                );
            var defDate2 = new Date();
            $("#clockDateEnd").timepicker('setTime', defaultEndTime());
            

        }

        function configureTime() {

            //$("#leaveEndTime").timepicker({
            //    beforeShowDay: $.datepicker.noWeekends
            //});

            $("#leaveEnd").timepicker("option", "max", defaultStartTime());

            //$("#leaveStartTime").timepicker({
            //    beforeShowDay: $.datepicker.noWeekends
            //});
            $("#leaveStart").timepicker("option", "min", defaultEndTime());
            $("#leaveStart").timepicker({ min: '7:00 am', max: '6:00 pm' });

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

        function onLeaveTypeChange(objmodel) {
            complusoryComment(leaveApplication().leaveType() === dataContext.leaveTypeEnum.special);
            switch (leaveApplication().leaveType()) {
                case dataContext.leaveTypeEnum.annual:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);

                    daysAllowed(staff().daysDue());
                    break;
                case dataContext.leaveTypeEnum.sick:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);
                    daysAllowed(staff().sickDaysAvilable());
                    break;
                case dataContext.leaveTypeEnum.offsite:
                    hideDaysDue(false);
                    hideHoursDue(false);
                    //daysAllowed(365);//since at work
                    hideTime(true);
                    hideDate(false);
                    hideHours(false);
                    hideDays(false);

                    break;
                case dataContext.leaveTypeEnum.study:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);
                    //daysAllowed(16);//Due to timetable and the number of courses  this will be a 
                    break;
                case dataContext.leaveTypeEnum.unpaid:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);
                    //daysAllowed(31); // Since a person can not leave more than two months without getting paid
                    break;
                case dataContext.leaveTypeEnum.family:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);
                    //daysAllowed(3); // Since a person can not leave more than two months without getting paid
                    break;
                case dataContext.leaveTypeEnum.special:
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(false);
                    hideHours(true);
                    hideDays(true);
                    hideDate(true);
                    complusoryComment(true);
                    //daysAllowed(365);
                    break;

                default:
                    daysAllowed(0);
                    hideDaysDue(true);
                    hideHoursDue(true);
                    hideTime(true);
                    hideDate(true);
                    break;
            }
            $("#daysAllowed").val(daysAllowed());
            showWarning((daysAllowed() - leaveDays()) < 0);


        }

        function onSelectedStaffChange(objModel) {
            for (var i = 0; i < staffToSelect().length; i++) {
                if (staffToSelect()[i].staffId() === selectedStaffId()) {
                    staff(staffToSelect()[i]);
                    break;
                }
            }
       
            setDefaultTimesOfSelectedStaff();
            //setDefaultTimes();
            configureStartDatePicker();
            configureEndDatePicker();
            configureClockDate();

        }

        function save(objModel) {
            if ($("#toggler").is(':checked')) {
                automaticAllDay(true);
            }

            canSave(true);
            if ($("#leaveEnd").val() < $("#leaveStart").val()) {
                return Q.resolve(app.showMessage("End date cannot be lesser than start date.", "End date greater than start date", ['Close'])).then(function (data) {
                });
            }
            if (leaveApplication().leaveType() === dataContext.leaveTypeEnum.special && $("#txtComment").val() === "") {
                return Q.resolve(app.showMessage("A reason for applying for special leave is compulsory for record purposes. Please add a comment.", "Special Leave Application", ['Close'])).then(function (data) {
                });
            }

            var test1 = $("#clockDateStart").val();
            var tes1 = defaultStartTime();
            var te1 = $("#leaveStart").val();
            var t1 = te1.split(' ');

            var test2 = $("#clockDateEnd").val();
            var tes2 = defaultEndTime();
            var te2 = $("#leaveEnd").val();
            var t2 = te2.split(' ');


            var start = $("#leaveStart").val();
            //start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            start = new Date(t1[0] + " " + defaultStartTime()); //start = new Date(start);
            if (automaticAllDay()) {
                var startTime = defaultStartTime().split(":");
                start.setHours(parseInt(startTime[0]) + 2);
                start.setMinutes(parseInt(startTime[1]));
            } else {
                start.setHours(start.getHours() + 2);
                start.setMinutes(start.getMinutes());
            }

            var end = $("#leaveEnd").val();
            end = new Date(t2[0] + " " + defaultEndTime()); //end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            if (automaticAllDay()) {
                var endTime = defaultEndTime().split(":");
                end.setHours(parseInt(endTime[0]) + 2);
                end.setMinutes(parseInt(endTime[1]));
            }
            else {
                end.setHours(end.getHours() + 2);
                end.setMinutes(end.getMinutes());
            }


            switch (leaveApplication().leaveType()) {
                case dataContext.leaveTypeEnum.annual:
                    objModel.staff().daysDue(parseInt(initialLeaveDays) - parseInt(leaveDays()));
                    break;
                case dataContext.leaveTypeEnum.sick:
                    objModel.staff().sickDaysAvilable(parseInt(initSickLEaveDays) - parseInt(leaveDays()));
                    break;
                case dataContext.leaveTypeEnum.offsite:
                    objModel.leaveApplication().leaveStatus(dataContext.leaveStatusEnum.pending);
            }
            objModel.leaveApplication().leaveDateStart(start);
            objModel.leaveApplication().leaveDateEnd(end);
            objModel.leaveApplication().staffId(selectedStaffId());

            if (leaveApplication().leaveType() === dataContext.leaveTypeEnum.offsite &&  objModel.leaveApplication().staffMember().offsiteApproval() === false) {
                objModel.leaveApplication().leaveStatus(dataContext.leaveStatusEnum.approved);
            } else {
                objModel.leaveApplication().leaveStatus(dataContext.leaveStatusEnum.pending);
            }
       

            var leavetypeName = $("#cbxLeaveType :selected").text();

            var startForConfirm = new Date(start);

            startForConfirm.setHours(startForConfirm.getHours() - 2);
            startForConfirm.setMinutes(startForConfirm.getMinutes());

            var startForConfirmTime = startForConfirm.toLocaleTimeString();
            var startForConfirmDate = startForConfirm.toLocaleDateString();

            var endForConfirm = new Date(end);

            endForConfirm.setHours(endForConfirm.getHours() - 2);
            endForConfirm.setMinutes(endForConfirm.getMinutes());

            var endForConfirmTime = endForConfirm.toLocaleTimeString();
            var endForConfirmDate = endForConfirm.toLocaleDateString();


            //Check if the data being saved is not a replica of whats in the database
            //if the dates are the same, verify that the date or tiime is greater than the previous
            return app.showMessage("You have applied for : " + leaveDays() + "\n" + " day(s) starting on " + startForConfirmDate + "," + startForConfirmTime + "\n" + " ending on the" + endForConfirmDate + "," + endForConfirmTime + "\n" + " type of: " + leavetypeName, "Confirm Leave Application", ['Yes', 'No']).then(function (result) {
                if (result === "Yes") {
                    return dataContext.changesSave().then(function () {
                     
                        dataContext.emailLeaveApplication(objModel.leaveApplication().leaveId()).then(function () {
                            Q.resolve(app.showMessage("Your have successfuly applied for leave", "Leave Application", ['Close'])).then(function (data) {
                                router.navigateTo('#/view_staffmenu');
                                return;
                            });
                        });
                    });
                } else {

                }
                canSave(true);

            });
        }

        function cancel() {
            dataContext.changesCancel();
            router.navigateTo('#/view_staffmenu');
        }

        function onAutomaticAllDayChange() {
            //var defaultStartTimeTest = defaultStartTime();
            //var defaultEndTimeTest = defaultEndTime();

            //$("#leaveStart").timepicker({ hour: 8,minute : 00 });

            //$("#leaveEnd").timepicker({ hour: 17, minute: 00 });

            disableTime(true);
            automaticAllDay(true);

            configureEndDatePicker();
            configureStartDatePicker();
       
        }

        ///recursive function
        ///This is a work around to get the difference in between
        function calculateLeaveDays() {
            var test1 = $("#clockDateStart").val();
            var tes1 = defaultStartTime();
            var te1 = $("#leaveStart").val();
            var t1 = te1.split(' ');

            var test2 = $("#clockDateEnd").val();
            var tes2 = defaultEndTime();
            var te2 = $("#leaveEnd").val();
            var t2 = te2.split(' ');

            //var str = (t1[0] + " " + defaultStartTime()).replace(/-/g, '/');  // replaces all occurances of "-" with "/"
            //var dateObject = new Date(str);

            //var str2 = (t2[0] + " " + defaultEndTime()).replace(/-/g, '/');  // replaces all occurances of "-" with "/"
            //var dateObject2 = new Date(str2);


            var publicHolidayCounter = 0;
            leaveDays(helper.calculateLeaveDays((t1[0] + " " + defaultStartTime()).replace(/-/g, '/'), (t2[0] + " " + defaultEndTime()).replace(/-/g, '/')));

            var start = $("#leaveStart").val();
            start = new Date(t1[0] + " " + defaultStartTime());

            var end = $("#leaveEnd").val();
            end = new Date(t2[0] + " " + defaultEndTime());
                 displayLeaveHours(helper.calculateLeaveHours((t1[0] + " " + defaultStartTime()).replace(/-/g, '/'), (t2[0] + " " + defaultEndTime()).replace(/-/g, '/')));
            //displayLeaveHours(helper.calculateLeaveHours(t1[0] + " " + defaultStartTime(), t2[0] + " " + defaultEndTime()));
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

            leaveDays(leaveDays() - publicHolidayCounter);

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

            showWarning((daysAllowed() - leaveDays()) < 0);
        }

        function calculateLeaveDaysForHalfDayStaff() {
            var publicHolidayCounter = 0;
            leaveDays(helper.calculateLeaveDays($("#leaveStart").datepicker("getDate"), $("#leaveEnd").datepicker("getDate")));

            var start = $("#leaveStart").val();
            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            var end = $("#leaveEnd").val();
            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

            displayLeaveHours(helper.calculateLeaveHours(start, end));
            displayLeaveDays(helper.calculateLeaveDays(start, end));

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
            leaveDays(leaveDays() - publicHolidayCounter);

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
            showWarning((daysAllowed() - leaveDays()) < 0);
        }

        function test() {
            return;
        }


        var vm = {
            activate: activate,
            viewAttached: viewAttached,

            onLeaveTypeChange: onLeaveTypeChange,
            onSelectedStaffChange: onSelectedStaffChange,
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
            displayLeaveHours: displayLeaveHours,
            hideDaysDue: hideDaysDue,
            hideHoursDue: hideHoursDue,
            hideTime: hideTime,
            hideDate: hideDate,
            hideHours: hideHours,
            hideDays: hideDays,
            getDefaultDateTime: getDefaultDateTime,
            clockDate: clockDate,
            managerandstaff: managerandstaff,
            isPartOfAssistants: isPartOfAssistants,
            isPartOfHr: isPartOfHr,
            isNormal: isNormal,
            allStaff: allStaff,
            selectedStaffId: selectedStaffId,
            staffToSelect: staffToSelect,
            automaticAllDay: automaticAllDay,
            onAutomaticAllDayChange: onAutomaticAllDayChange,
            test: test

        };
        return vm;
    });

/* function configureStartTimePicker() {
         var d = new Date();
         var month = d.getMonth() + 1;
         var day = d.getDate();
         var output = d.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;

         $("#leaveStartTime").datepicker(
             {
                 beforeShowDay: $.datepicker.noWeekends,
                 dateFormat: 'dd-mm-yy',
                 timeFormat: 'HH:mm',
              showOtherMonths: false,
                 selectOtherMonths: false,
                 onClose: function (selectedDate) {
                     //var tempSelectedDate = $("#leaveStart").datepicker("getDate");
                     //$("#leaveEnd").datepicker("option", "minDate", tempSelectedDate).val(output + " " + defaultEndTime());
                     calculateLeaveDays(1);
                     //$("#leaveEnd").datepicker("setDate", tempSelectedDate).val(output + " " + defaultEndTime());
                 }
             }).datepicker("setDate", new Date()).val(output);;
         $("#leaveStartTime").datepicker({ beforeShowDay: $.datepicker.noWeekends, format: 'yyyy-MM-dd' });
         document.getElementById("toggler").onclick = function () {
             var s = document.getElementById("t3");
             (s.style.display == "none") ? s.style.display = "" : s.style.display = "none";

             var s2 = document.getElementById("t4");
             (s2.style.display == "none") ? s2.style.display = "" : s2.style.display = "none";
         };
  
     }

     function configureEndTimePicker() {
        var d = new Date();
        var month = d.getMonth() + 1;
        var day = d.getDate();
        var output = d.getFullYear() + '-' + (month < 10 ? '0' : '') + month + '-' + (day < 10 ? '0' : '') + day;

         $("#leaveEndTime").datepicker(
             {
                 beforeShowDay: $.datepicker.noWeekends,
                 dateFormat: 'dd-mm-yy',
                 timeFormat: 'HH:mm',
              onClose: function (selectedDate) {
                     calculateLeaveDays(1);
                 }
             }
         ).datepicker("setDate", new Date()).val(output);
         $("#leaveEndTime").datepicker("option", "minDate", new Date()).val(output);
         $("#leaveEndTime").datepicker({ beforeShowDay: $.datepicker.noWeekends, format: 'yyyy-MM-dd' });

     } */
