define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/thesuggestion', 'durandal/app', 'viewmodels/editors/leavedata', 'viewmodels/editors/leaveSummaryTest', 'viewmodels/editors/specialLeave', 'services/helpers', 'viewmodels/editors/companyLeaveSummary'],
    function (dataContext, baseEditor, leaveAppEditor, app, leavedata, leaveSummaryTest, specialLeave, helper, companyLeaveSummary) {
        var suggestions = ko.observableArray();
        var  allsuggestions = ko.observableArray();
        function activate() {
            refresh();
        }

        function viewAttached() {

        }
        function refresh() {
            return dataContext.getSuggestions(suggestions).then(function () {
             
            });
        }

        function gotoSuggestionDetails(objMOdel) {
            var test = objMOdel;
            return baseEditor.objEdit(new leaveAppEditor(objMOdel)).then(function () {
                return refresh().then(function () {

                });
            });
        }

        function cancelSuggestion(objModel) {
            return app.showMessage("Are you sure you want to delete this suggestion?", "Confirm Delete", ['Yes', 'No']).then(function (result) {
                if (result === "Yes") {

                    objModel.entityAspect.setDeleted();
                    return dataContext.changesSave().then(function () {
                        refresh();
                    });
                }
            });

        };
        function approveMultiple(objMOdel) {
            var apps = objMOdel.suggestions();
            var count = 0;
            for (var i = 0; i < apps.length; i++) {
                var approved = apps[i].suggestionStatus() == "Pending";
                var stats = apps[i].suggestionStatus() == "Pending";
                if (approved == stats) {
                        apps[i].suggestionStatus("Accepted");
                    count++;
                }
            }

            if (count > 0) {
                return Q.fcall(function () {
                    dataContext.changesSave().then(function () {

                       return refreshAllSuggestions().then(function () {
                        return refresh();
                        });
                    });
                });
            }
        }

        function refreshAllSuggestions() {
            return dataContext.getSuggestions(allsuggestions).then(function () {
                for (var i = 0; i < allsuggestions().length; i++) {
                    if (allsuggestions()[i].suggestionStatus() === "Pending") {
                        unapprovedSuggestion(true);
                        break;
                    }
                }
                for (var j = 0; j < allsuggestions().length; j++) {
                    if (allsuggestions()[j].suggestionStatus() === "Rejected") {
                        allsuggestions().splice(j, 1);
                    }
                    else if (allsuggestions()[j].suggestionStatus() === "Pending") {
                        allsuggestions().splice(j, 1);
                    }

                }
            });
        }

        var vm = {
            activate: activate,
            viewAttached: viewAttached,
            suggestions: suggestions,
            gotoSuggestionDetails: gotoSuggestionDetails,
            cancelSuggestion: cancelSuggestion,
            approveMultiple: approveMultiple
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