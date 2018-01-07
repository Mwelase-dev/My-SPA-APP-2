define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/leaveapplication', 'durandal/app', 'viewmodels/editors/leavedata', 'viewmodels/editors/leaveSummaryTest', 'viewmodels/editors/specialLeave'],
    function (dataContext, baseEditor, leaveAppEditor, app, leavedata, leaveSummaryTest, specialLeave) {

        var staff = ko.observable();
        var filterStaff = ko.observable(null);
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

        var canFilterStaff = ko.observable(false);
        var canEdit = ko.observable(false);
        var canCancel = ko.observable(false);
        var canAppDecl = ko.observable(false);

        var statusSelected = ko.observable(false);

        var disableApprove = ko.observable(false);

        var isLeaveApplicationInCurrentLeaveCycle = ko.observable(false);
       
        function activate() {

            return dataContext.getCurrentUser(staff).then(function () {
                return dataContext.getCurrentUserRoles(userRoles).then(function () {
                    return dataContext.getLeaveStatuses(leaveStatuses).then(function () {
                        if (dataContext.isInRole(userRoles(), dataContext.userRoleEnum.manager)) {
                            canFilterStaff(true);
                            return dataContext.getAllDirectors(allStaff).then(function () {
                                return refreshByStatus().then(function () {

                                });
                            });
                        } else {

                            var temp = [];
                            temp.push(staff());
                            allStaff(temp);

                            selectedStaff(staff().staffId());
                            daysRemaining(staff().daysDue());
                            enableFiltering(true);
                            return canFilterStaff(false);
                        }
                    });
                });
            });
        }

        function sortLeaveApplicationsByName () {
            leaveApplications().sort(function (a, b) {
                return a.staffMember().fullName() < b.staffMember().fullName() ? -1 : 1;
            });
        };

        function viewAttached() {
            return refreshByStatus();
        }

        function cancelLeave(objModel) {
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
                return dataContext.getClockDataForLeaveClockRecord(clockRecord, objModel.staffId(), startDate, endDate).then(function() {

                    clockRecord().dataStatus = 3;

                    var comments = clockRecord().comments;
                    comments += "Update to clock record has been deleted because leave was cancelled";
                    clockRecord().comments = comments;
                    dataContext.changesSave().then(function () {
                        var emailRes = ko.observable();
                        return dataContext.emailLeaveCancelled(emailRes, objModel.leaveId()).then(function () {
                            if (emailRes() === true) {
                                app.showMessage("You leave application has been cancelled and your manager has been notified.", "Success", ["Close"]);
                            } else {
                                 app.showMessage("Your leave has been cancelled but there was a problem email the update to your manager.", "Failure", ["Close"]);
                            }
                            return refreshByStatus();

                        }).fail(function () {
                            app.showMessage("Your leave has been cancelled but there was a problem email the update to your manager.", "Failure", ["Close"]);
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
            return activate();
        };

        var updateLeaveApplication = function (selectedLeave) {
            leaveDetails(selectedLeave, true);
        };

        function leaveDetails(selectedLeave, isUpdate) {
            return baseEditor.objEdit(new leaveAppEditor(selectedLeave, isUpdate)).then(function () {
                return refreshByStatus(dataContext.leaveStatusEnum.pending).then(function () {
                    return activate();
                });
            });
        }

        function refreshByStatus() {
            
            canEdit(selectedLeaveStat() == dataContext.leaveStatusEnum.pending);
            disableApprove(selectedLeaveStat() == dataContext.leaveStatusEnum.pending);
            statusSelected(selectedLeaveStat() != undefined);

            var manager = typeof selectedStaff() === "undefined" ? staff().staffId() : staff().staffId();

            return dataContext.getDirectorsLeaveApplicationsByStatus(leaveApplications,
                selectedStaff(), selectedLeaveStat(), manager).then(function () {
               
                for (var i = 0; i < leaveApplications().length; i++) {
                    if (leaveApplications()[i].displayLeaveStart() < leaveApplications()[i].staffMember().cycleStart()) {
                        leaveApplications().splice(i, 1);
                    }
                }

                for (var i = 0; i < leaveApplications().length; i++) {
                    if (leaveApplications()[i].staffMember().isDirector() === false) {
                        leaveApplications().splice(i, 1);
                    }
                }

                sortLeaveApplicationsByName();

                canAppDecl(dataContext.isInRole(userRoles, dataContext.userRoleEnum.manager));
                var data = leaveApplications()[0];
                if (typeof data === 'undefined' || data == null) {
                    filterStaff(null);
                    return;
                }
                sortLeaveApplicationsByName();
                filterStaff(data.staffMember());
              
            });
        }
         
        function staffLeaveSummary() {
            return baseEditor.objEdit(new leavedata(selectedStaff()));
        }

        //Summary Test
        var staffLeaveSummary1 = function () {
            if (selectedStaff()) {
                return baseEditor.objEdit(new leaveSummaryTest(selectedStaff())).then(function() {
                    //history.go(0);
                });
            }
        }

        var specialLeaveAdd = function () {
            //window.location.href = '#/view_specialLeave';
            return baseEditor.objEdit(new specialLeave(selectedStaff())).then(function () {
                
            });
        }
         
        //recursive function
        //This is a work around to get the difference in between days
        function calculateLeaveDays(dayCounter) {
            if (dayCounter === "undefined")
                dayCounter = 1;

            var start = new Date($("#leaveStart").datepicker("getDate"));
            var end = new Date($("#leaveEnd").datepicker("getDate"));
            var startValue = new Date(start.setDate(start.getUTCDate() + dayCounter));
            if (startValue.getFullYear() == end.getFullYear() && startValue.getMonth() == end.getMonth() && startValue.getDate() == end.getDate()) {
                for (var i = 0; i < dayCounter; i++) {
                    var tempDay = new Date($("#leaveStart").datepicker("getDate"));
                    tempDay.setDate(tempDay.getDate() + i);
                    var day = tempDay.getDay();
                    if ((day == 6) || (day == 0))
                        dayCounter--;
                }
                leaveDays(dayCounter);
                return;
            } else {
                dayCounter += 1;
                calculateLeaveDays(dayCounter);
            }
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
                    else
                        Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
                        });

                    if (typeof apps[i].approvedBy1() !== "undefined" && typeof apps[i].approvedBy2() !== "undefined") {
                        apps[i].leaveStatus(dataContext.leaveStatusEnum.approved);
                        apps[i].leaveComments("Mutiple approval - by " + staff().fullName());
                    }
                    dataContext.saveLeaveApplication(apps[i]).then(function () {
                    dataContext.emailLeaveApproved(apps[i].leaveId());
                    count++;
                    });
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

        var vm = {
            activate    : activate,
            viewAttached: viewAttached,
            staff       : staff,
            filterStaff : filterStaff,
            allStaff    : allStaff,
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
            isLeaveApplicationInCurrentLeaveCycle: isLeaveApplicationInCurrentLeaveCycle
        };
        return vm;
    });