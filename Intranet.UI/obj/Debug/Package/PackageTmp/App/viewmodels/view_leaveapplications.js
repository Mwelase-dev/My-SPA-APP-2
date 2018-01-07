define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/leaveapplication', 'viewmodels/editors/actionReason', 'durandal/app', 'viewmodels/editors/leavedata', 'viewmodels/editors/leaveSummaryTest', 'viewmodels/editors/specialLeave', 'services/helpers', 'viewmodels/editors/companyLeaveSummary'],
    function (dataContext, baseEditor, leaveAppEditor, getActionReason, app, leavedata, leaveSummaryTest, specialLeave, helper, companyLeaveSummary) {

        var staff = ko.observable();
        var filterStaff = ko.observable(true);
        var allStaff = ko.observableArray();
        var daysRemaining = ko.observable();
        var userRoles = ko.observableArray();

        var selectedLeaveStat = ko.observable();
        var selectedStaff = ko.observable();
        var enableFiltering = ko.observable();
        var leaveApplications = ko.observableArray();
        var leaveinfo = ko.observableArray();
        var simultApps = ko.observableArray();
        var leaveStatuses = ko.observableArray();

        var canFilterStaff = ko.observable(true);
        var canEdit = ko.observable(false);
        var canCancel = ko.observable(false);
        var canAppDecl = ko.observable(false);

        var statusSelected = ko.observable(false);

        var disableApprove = ko.observable(false);

        var isLeaveApplicationInCurrentLeaveCycle = ko.observable(false);

        var isPartOfManagers = ko.observable(false);
        var isPartOfHr = ko.observable(false);
        var staffToSelect = ko.observableArray();

        var hours = ko.observable();
        var days = ko.observable();

        function sortStaff() {
            allStaff().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

        function activate() {
            leaveApplications([]);
            selectedStaff([]);
            selectedLeaveStat([]);
            allStaff([]);
            return dataContext.getCurrentUser(staff).then(function () {
                return dataContext.getCurrentUserRoles(userRoles).then(function () {
                    return dataContext.getLeaveStatuses(leaveStatuses).then(function () {
                        isPartOfManagers(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.manager));
                        isPartOfHr(dataContext.isInRole(userRoles(), dataContext.userRoleEnum.humanResource));
                        allStaff(staff());

                        if (isPartOfHr()) {
                            allStaff([]);
                            return dataContext.allStaff(allStaff).then(function () {
                                sortStaff();
                            });
                        } else if (isPartOfManagers()) {
                            allStaff([]);
                            return dataContext.getStaffByManager(allStaff, staff).then(function () {
                                sortStaff();
                            });
                        }

                        selectedStaff(staff().staffId());
                        daysRemaining(staff().DaysDue());
                        enableFiltering(true);
                        return canFilterStaff(true);
                    });
                });
            });
        }

        function sortLeaveApplicationsByName() {
            leaveApplications().sort(function (a, b) {
                if (a.staffMember() !== null && b.staffMember() !== null) {
                    return a.staffMember().fullName() < b.staffMember().fullName() ? -1 : 1;

                } else {
                    return;
                }
            });
        };

        function viewAttached() {
            //return refreshByStatus();
        }

        function cancelLeave(objModel) {
            Q.resolve(app.showMessage("Are you sure you want to cancel your leave application", "Cancellation", ['Yes', 'No'])).then(function (data) {
                if (data == 'No')
                    return;


                return baseEditor.objEdit(new getActionReason(objModel)).then(function (result) {
                    //objModel.staffMember().daysDue(parseInt(objModel.staffMember().daysDue()) + parseInt(objModel.displayLeaveDays));
                    if (result == undefined && objModel.reasonForAction() == "")
                        return;


                    var start = objModel.leaveDateStart();
                    start = new Date(start);
                    start.setHours(start.getHours() + 2);
                    start.setMinutes(start.getMinutes());


                    var end = objModel.leaveDateEnd();
                    end = new Date(end);
                    end.setHours(end.getHours() + 2);
                    end.setMinutes(end.getMinutes());


                    objModel.leaveDateStart(start);
                    objModel.leaveDateEnd(end);
                    objModel.reasonForAction(objModel.reasonForAction());
                    objModel.leaveStatus(dataContext.leaveStatusEnum.cancelled);
                    objModel.entityAspect.setModified();
                    dataContext.changesSave().then(function () {
                        var emailRes = ko.observable(false);
                        return dataContext.emailLeaveCancelled(emailRes, objModel.leaveId()).then(function () {
                            if (emailRes() === true) {
                                app.showMessage("You leave application has been cancelled and your manager has been notified.", "Success", ["Close"]);
                            } else {
                                app.showMessage("Your leave has been cancelled but there was a problem emailling the update to your manager.", "Failure", ["Close"]);
                            }
                            return refreshByStatus();

                        }).fail(function () {
                            app.showMessage("Your leave has been cancelled but there was a problem emailling the update to your manager.", "Failure", ["Close"]);
                            return refreshByStatus();
                        });
                    });
                });
            });
        }

        function getLeaveApplications() {
            return refreshByStatus();
        }

        function cmbStaffMemberChange() {
            enableFiltering(selectedStaff() != null);
        }

        var gotoLeaveDetails = function (selectedLeave) {
            leaveDetails(selectedLeave, false);
        };

        var updateLeaveApplication = function (selectedLeave) {
            leaveDetails(selectedLeave, true);
        };

        function leaveDetails(selectedLeave, isUpdate) {
            return baseEditor.objEdit(new leaveAppEditor(selectedLeave, isUpdate)).then(function () {
                return refreshByStatus(selectedLeaveStat()).then(function () {

                });
            });
        }

        function refreshByStatus() {
            canEdit(selectedLeaveStat() == dataContext.leaveStatusEnum.pending);
            disableApprove(selectedLeaveStat() == dataContext.leaveStatusEnum.pending);
            statusSelected(selectedLeaveStat() != undefined);
            var tempLeaves = ko.observableArray();
            //if (selectedStaff() != undefined) {
            tempLeaves([]);
            return dataContext.getStaffLeaveApplicationsByStatusHr(tempLeaves,
                selectedStaff(), selectedLeaveStat()).then(function () {

                    for (var j = 0; j < allStaff().length; j++) {
                        if (allStaff()[j].staffId() === selectedStaff()) {
                            for (var k = 0; k < allStaff()[j].staffHoursData().length; k++) {
                                if (allStaff()[j].staffHoursData()[k].dayHoursRequired() < 540) {

                                    break;
                                } else {

                                    break;
                                }
                            }
                        }
                    }

                    leaveApplications(tempLeaves());

                    for (var i = 0; i < leaveApplications().length; i++) {
                        if (leaveApplications()[i].staffMember() !== null) {
                            if (leaveApplications()[i].displayLeaveStart() < leaveApplications()[i].staffMember().cycleStart()) {
                                leaveApplications().splice(i, 1);
                            }
                        } else {
                            break;
                        }
                    }

                    sortLeaveApplicationsByName();

                    canAppDecl(dataContext.isInRole(userRoles, dataContext.userRoleEnum.manager) || dataContext.isInRole(userRoles, dataContext.userRoleEnum.humanResource));
                    var data = leaveApplications()[0];
                    if (typeof data === 'undefined' || data == null) {
                        filterStaff(null);
                        return;
                    }
                    sortLeaveApplicationsByName();
                    filterStaff(data.staffMember());
                });
            /* } else {
                 tempLeaves([]);
                 return dataContext.getStaffLeaveApplicationsByStatusHrNoStaff(tempLeaves,
                     selectedStaff(), selectedLeaveStat()).then(function () {
                  
                     for (var j = 0; j < allStaff().length; j++) {
                         if (allStaff()[j].staffId() === selectedStaff()) {
                             for (var k = 0; k < allStaff()[j].staffHoursData().length; k++) {
                                 if (allStaff()[j].staffHoursData()[k].dayHoursRequired() < 540) {
                                     //calculateLeaveDaysForHalfDayStaff(tempLeaves());
                                     break;
                                 } else {
                                     //calculateLeaveDays(tempLeaves());
                                     break;
                                 }
                             }
                         }
                     }
 
 
                     leaveApplications(tempLeaves());
 
                     for (var i = 0; i < leaveApplications().length; i++) {
                         if (leaveApplications()[i].staffMember() !== null) {
                             if (leaveApplications()[i].displayLeaveStart() < leaveApplications()[i].staffMember().cycleStart()) {
                                 leaveApplications().splice(i, 1);
                             }
                         } else {
                             break;
                         }
                     }
 
                     sortLeaveApplicationsByName();
 
                     canAppDecl(dataContext.isInRole(userRoles, dataContext.userRoleEnum.manager) || dataContext.isInRole(userRoles, dataContext.userRoleEnum.humanResource));
                     var data = leaveApplications()[0];
                     if (typeof data === 'undefined' || data == null) {
                         filterStaff(null);
                         return;
                     }
                     sortLeaveApplicationsByName();
                     filterStaff(data.staffMember());
                 });
             }*/
        };

        function staffLeaveSummary() {
            return baseEditor.objEdit(new leavedata(selectedStaff()));
        }

        var staffLeaveSummary1 = function () {
            if (selectedStaff()) {
                return baseEditor.objEdit(new leaveSummaryTest(selectedStaff())).then(function () {
                    //history.go(0);
                });
            } else {
                return baseEditor.objEdit(new companyLeaveSummary()).then(function () {

                });
            }
        }

        var specialLeaveAdd = function () {
            //window.location.href = '#/view_specialLeave';
            return baseEditor.objEdit(new specialLeave(selectedStaff())).then(function () {

            });
        }

        function approveMultiple(objMOdel) {
            var apps = objMOdel.leaveApplications();
            var count = 0;
            for (var i = 0; i < apps.length; i++) {
                var approved = apps[i].approve() == true;
                var stats = apps[i].leaveStatus() == dataContext.leaveStatusEnum.pending;
                if (approved == stats) {
                    //check what manager the user is
                    //if manager is both manager 1 and manager 2
                    if (apps[i].staffMember().staffManager1Id() == staff().staffId() && apps[i].staffMember().staffManager2Id() == staff().staffId()) {
                        apps[i].approvedBy1(staff().staffId());
                        apps[i].approvedBy2(staff().staffId());
                    } else if (apps[i].staffMember().staffManager1Id() == staff().staffId())
                        apps[i].approvedBy1(staff().staffId());
                    else if (apps[i].staffMember().staffManager2Id() == staff().staffId())
                        apps[i].approvedBy2(staff().staffId());

                    if (typeof apps[i].approvedBy1() !== "undefined" && typeof apps[i].approvedBy2() !== "undefined") {
                        apps[i].leaveStatus(dataContext.leaveStatusEnum.approved);
                        //apps[i].leaveComments("Mutiple approval - by " + staff().fullName());
                    }
                    count++;
                }
            }

            if (count > 0) {
                return Q.fcall(function () {
                    dataContext.changesSave().then(function () {
                        app.showMessage("Leave application approved. An approval email has been sent to the applicant", 'Success', ['Close']);
                        return refreshByStatus();
                    });
                });
            }
        }

        function cancel() {
            return history.go(-1);
        }

        function callLeave() {
            return dataContext.getleavedata(leaveinfo);
        }

        function calculateLeaveDays(obj) {

            if (obj.length === 0)
                return obj;
            var revisedLeaveHours = ko.observable();
            var revisedLeaveDays = ko.observable();
            var revisedDisplayHours = ko.observable();

            for (var i = 0; i < obj.length; i++) {
                revisedDisplayHours(helper.calculateLeaveHours(obj[i].displayLeaveStart, obj[i].displayLeaveEnd));
                revisedLeaveHours(obj[i].displayLeaveHours());
                revisedLeaveDays(obj[i].displayLeaveDays());
                //var timespan = moment.utc(moment(staffHours()[0].displayEndTime(), "HH:mm:ss").diff(moment(staffHours()[0].displayStartTime(), "HH:mm:ss"))).format("HH:mm:ss");

                if (revisedDisplayHours() === "08:00:00") {
                    revisedDisplayHours(0);
                    var test = ko.observable();
                    test(revisedLeaveDays());
                    if (test() === 0) {
                        test(test() + 1);
                        //revisedLeaveDays([]);
                        revisedLeaveDays(test);

                    } else if (test() > 0) {
                        test(test() + 1);
                        revisedLeaveDays(test);
                    }
                }

                obj.displayLeaveHours = revisedDisplayHours();
                obj.displayLeaveDays = revisedLeaveDays();
                days(revisedLeaveDays);
                hours(revisedDisplayHours);
            }
            return obj;
        }

        function calculateLeaveDaysForHalfDayStaff(obj) {

            if (obj.length === 0)
                return obj;

            for (var i = 0; i < obj.length; i++) {
                var revisedLeaveHours = obj[i].displayLeaveHours;
                var revisedLeaveDays = obj[i].displayLeaveDays;
                //var timespan = moment.utc(moment(staffHours()[0].displayEndTime(), "HH:mm:ss").diff(moment(staffHours()[0].displayStartTime(), "HH:mm:ss"))).format("HH:mm:ss");

                if (revisedLeaveHours === "05:00:00") {
                    revisedLeaveHours = 0;
                    var test = revisedLeaveDays;
                    if (test === 0) {
                        test = test + 1;
                        //revisedLeaveDays([]);
                        revisedLeaveDays = test;

                    } else if (test > 0) {
                        test++;
                        revisedLeaveDays = test;
                    }
                }

                obj[i].displayLeaveHours = revisedLeaveHours;
                obj[i].displayLeaveDays = revisedLeaveDays;
            }

            return obj;

        }

        var vm = {
            activate: activate,
            viewAttached: viewAttached,
            staff: staff,
            filterStaff: filterStaff,
            allStaff: allStaff,
            leaveApplications: leaveApplications,
            simultApps: simultApps,
            leaveStatuses: leaveStatuses,
            daysRemaining: daysRemaining,
            canFilterStaff: canFilterStaff,
            canEdit: canEdit,
            canCancel: canCancel,
            enableFiltering: enableFiltering,
            selectedStaff: selectedStaff,
            selectedLeaveStat: selectedLeaveStat,
            getLeaveApplications: getLeaveApplications,
            cmbStaffMemberChange: cmbStaffMemberChange,
            cancelLeave: cancelLeave,
            gotoLeaveDetails: gotoLeaveDetails,
            staffLeaveSummary: staffLeaveSummary,
            updateLeaveApplication: updateLeaveApplication,
            cancel: cancel,
            approveMultiple: approveMultiple,
            leaveinfo: leaveinfo,
            callLeave: callLeave,
            staffLeaveSummary1: staffLeaveSummary1,
            specialLeaveAdd: specialLeaveAdd,
            disableApprove: disableApprove,
            statusSelected: statusSelected,
            isLeaveApplicationInCurrentLeaveCycle: isLeaveApplicationInCurrentLeaveCycle,
            staffToSelect: staffToSelect,
            hours: hours,
            days: days
        };
        return vm;
    });















































/*    function cancelLeave(objModel) {
           Q.resolve(app.showMessage("Are you sure you want to cancel your leave application", "Cancellation", ['Yes', 'No'])).then(function (data) {
               if (data == 'No')
                   return;

               objModel.staffMember().daysDue(parseInt(objModel.staffMember().daysDue()) + parseInt(objModel.displayLeaveDays));
               objModel.leaveStatus(dataContext.leaveStatusEnum.cancelled);
               objModel.entityAspect.setModified();

               var startDate = null;
               var endDate = null;
               var start = objModel.leaveDateStart();
               var end = objModel.leaveDateEnd();
               startDate = moment(start).format('YYYY-MM-DD');
               endDate = moment(end).format('YYYY-MM-DD');

               var clockRecord = ko.observable();
               return dataContext.getClockDataForLeaveClockRecord(clockRecord, objModel.staffId(), startDate, endDate).then(function () {

                   clockRecord().dataStatus = 3;

                   var comments = clockRecord().comments;
                   comments += "Update to clock record has been deleted because leave was cancelled";
                   clockRecord().comments = comments;
                   dataContext.changesSave().then(function () {
                       var emailRes = ko.observable();
                       return dataContext.emailLeaveCancelled(emailRes, objModel.leaveId()).then(function () {
                           //if (emailRes() === true) {
                           app.showMessage("You leave application has been cancelled and your manager has been notified.", "Success", ["Close"]);
                           //} else {
                           //     app.showMessage("Your leave has been cancelled but there was a problem email the update to your manager.", "Failure", ["Close"]);
                           //}
                           return refreshByStatus();

                       }).fail(function () {
                           //app.showMessage("Your leave has been cancelled but there was a problem email the update to your manager.", "Failure", ["Close"]);
                           return refreshByStatus();
                       });
                   });
               });
           });
       }

    */

//recursive function
//This is a work around to get the difference in between days
//function calculateLeaveDays(dayCounter) {
//    if (dayCounter === "undefined")
//        dayCounter = 1;

//    var start = new Date($("#leaveStart").datepicker("getDate"));
//    var end = new Date($("#leaveEnd").datepicker("getDate"));
//    var startValue = new Date(start.setDate(start.getUTCDate() + dayCounter));
//    if (startValue.getFullYear() == end.getFullYear() && startValue.getMonth() == end.getMonth() && startValue.getDate() == end.getDate()) {
//        for (var i = 0; i < dayCounter; i++) {
//            var tempDay = new Date($("#leaveStart").datepicker("getDate"));
//            tempDay.setDate(tempDay.getDate() + i);
//            var day = tempDay.getDay();
//            if ((day == 6) || (day == 0))
//                dayCounter--;
//        }
//        leaveDays(dayCounter);
//        return;
//    } else {
//        dayCounter += 1;
//        calculateLeaveDays(dayCounter);
//    }
//}