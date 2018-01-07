define(['services/logger', 'services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/clockingrecord', 'viewmodels/editors/createallnewclockrecord', 'viewmodels/editors/createclockrecord', 'durandal/app'],
    function (logger, dataContext, editor, clockRecordEditor, createallnewclockrecord, createclockrecordforspecificday, app) {
        var clockingData = ko.observableArray();
        var staff = ko.observableArray();
        var selectedStaff = ko.observable();
        var isLoading = ko.observable();
        var enableFiltering = ko.observable(false);
        var defStartDate = new Date();
        var overTimeTotal = ko.observable();
        var timeDebtTotal = ko.observable();
        var authorised = ko.observable(true);
        var staffMem = ko.observable();
        var userRoles = ko.observableArray();
        var csvFile = ko.observableArray();
        var staffName = ko.observable();
        var staffSurname = ko.observable();
        var netBalance = ko.observable();

        function init() {
            defStartDate = new Date(defStartDate.setDate(defStartDate.getUTCDate() - 7));
            enableFiltering(staff().length > 1);
        }

        function activate() {
            init();

            //get all the current user's roles
            return dataContext.getCurrentUserRoles(userRoles).then(function () {
                if (isInRole(dataContext.userRoleEnum.intranetAdmins) || isInRole(dataContext.userRoleEnum.humanResource)) {
                    //logger.log("Intranet admin/Human resource",null,userRoles,true);
                    clockingData([]);
                    return dataContext.getAllStaff(staff);
                }
                else if (isInRole(dataContext.userRoleEnum.manager)) {
                    return dataContext.getCurrentUser(staffMem).then(function () {
                        return dataContext.getAllStaff(staff, staffMem).then(function () {
                            logger.log("Manager user", null, userRoles, true);
                        });
                    });
                } else {
                    return Q.fcall(function () {
                        //todo - jay : clean up may cause perfomance issues
                        return dataContext.getCurrentUser(staffMem).then(function () {
                            //logger.log("Normal user", null, userRoles, true);
                            authorised(true);
                            selectedStaff(staffMem().staffClockId());
                            staffName(staffName());
                            staffSurname(staffSurname());

                            enableFiltering(selectedStaff() != null);
                            staff(staffMem());
                        });

                    });
                }
            });
        }

        function viewAttached() {
            $("#clockStartDate").datepicker({
                dateFormat: "dd-mm-yy",
                onClose: function () {
                    var tempDate = $("#clockStartDate").datepicker("getDate");
                    $("#clockEndDate").datepicker("option", "minDate", tempDate);
                }
            }).datepicker("setDate", defStartDate);
            $("#clockEndDate").datepicker({ dateFormat: "dd-mm-yy", minDate: defStartDate }).datepicker("setDate", Date());
        }

        function isInRole(role) {
            return dataContext.isInRole(userRoles(), role);
        }

        var getTimeKeepingData = function () {

            isLoading(true);
            enableFiltering(false);

            var startDate = null;
            var endDate = null;

            var staffId = selectedStaff().staffClockId();
            var start = $("#clockStartDate").datepicker('getDate');
            var end = $("#clockEndDate").datepicker('getDate');


            var name = selectedStaff().staffName();
            var surname = selectedStaff().staffSurname();

            startDate = moment(start).format('YYYY-MM-DD');
            endDate = moment(end).format('YYYY-MM-DD');


            //start and end dates should be inclusive
            //breeze does not have a greater or equal to option
            // var startValue = new Date(startDate);
            //startValue = new Date(startValue.getFullYear(), startValue.getMonth(), startValue.getDate() - 1);
            return dataContext.getTimeKeepingData(clockingData, staffId, name, surname, startDate, endDate).then(function () {
                isLoading(false);
                enableFiltering(true);
                calculateTotals();
            });
        };

        var cmbStaffMemberChange = function () {
            enableFiltering(selectedStaff != null);
            var test = selectedStaff();
            staffName(staffName());
            staffSurname(staffSurname());
        };

        function calculateTotals() {
            var overtTimeMinutes = 0;
            var timeDebtMinutes = 0;
            for (var i = 0; i < clockingData().length; i++) {
                var tkiList = clockingData()[i].timeKeepingItems;
                for (var j = 0; j < tkiList.length; j++) {
                    overtTimeMinutes += tkiList[j].overTimeInMinutes;
                    timeDebtMinutes += tkiList[j].timeDebtInMinutes < 0 ? tkiList[j].timeDebtInMinutes * -1 : tkiList[j].timeDebtInMinutes;
                }
            }
            overTimeTotal(Math.floor(overtTimeMinutes / 60) + " hour(s) " + (overtTimeMinutes - ((Math.floor(overtTimeMinutes / 60)) * 60)) + " minute(s)");
            timeDebtTotal(Math.floor(timeDebtMinutes / 60) + " hour(s) " + (timeDebtMinutes - ((Math.floor(timeDebtMinutes / 60)) * 60)) + " minute(s)");

            var overtimeHours = Math.floor(overtTimeMinutes / 60);
            var overtimeMinutes = overtTimeMinutes - ((Math.floor(overtTimeMinutes / 60)) * 60);

            var timedebtHours = Math.floor(timeDebtMinutes / 60);
            var timedebtMinutes = timeDebtMinutes - ((Math.floor(timeDebtMinutes / 60)) * 60);

            var resultHours = overtimeHours - timedebtHours;
            var resultMinutes = overtimeMinutes - timedebtMinutes;

            var result = resultHours + " hour(s) " + resultMinutes + " minute(s)";
            netBalance(result);
        }

        function aprroveClockRecord(objModel) {
            var clockRecord = ko.observable();
            return dataContext.getClockRecord(clockRecord, objModel.clockDataId)
                         .then(function () {
                             clockRecord().approved(true);
                             dataContext.changesSave();
                         });
        }

        function editClockRecord(objModel) {
            var clockRecord = ko.observable();
            if (objModel.clockDataId === 0) {
                return Q.resolve(app.showMessage("This clock record was added as a result of leave, you cannot edit this record", "Uneditable record", ['Close'])).then(function (data) {
                });
            }
            return dataContext.getClockRecord(clockRecord, objModel.clockDataId)
                .then(function () {
                    return editor.objEdit(new clockRecordEditor(clockRecord()))
                       .then(function () {
                           getTimeKeepingData();
                       });
                });
        }

        function addClockRecord(objModel) {
            if (objModel.clockDataId === 0) {
                return Q.resolve(app.showMessage("This clock record was added as a result of leave, you cannot delete this record", "Locked record", ['Close'])).then(function (data) {
                });
            }
            var clockModel = dataContext.manufactureEntity("StaffClockModel");
            clockModel.staffId(objModel.staffId);
            clockModel.clockDateTime(new Date(objModel.date));
            return editor.objEdit(new createclockrecordforspecificday(clockModel)).then(function () {
                getTimeKeepingData();
            });
        }

        var clockme = function () {
            var clockModel = dataContext.manufactureEntity("StaffClockModel");
            clockModel.staffId(selectedStaff().staffId());
            //clockModel.clockDateTime(new Date(objModel.date));
            return editor.objEdit(new createallnewclockrecord(clockModel)).then(function () {
                getTimeKeepingData();
            });
        }
        var clockmeNow = function () {
            var clockModel = dataContext.manufactureEntity("StaffClockModel");
            clockModel.staffId(selectedStaff().staffId());

            var dateNow = new Date();


            var hours = dateNow.getHours();
            var minutes = dateNow.getMinutes();

            var date = new Date(dateNow);
            date.setHours(hours + 2);
            date.setMinutes(minutes);

            //offsiteclockin = new Date(offsiteclockin.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            //offsiteclockin.setHours(offsiteclockin.getHours() + 2);

            clockModel.comments("Online clock in");
            clockModel.recordStatus(dataContext.recStatus.statusActive);
            clockModel.clockDateTime(date);
            clockModel.originalClockDateTime(date);
            clockModel.dataStatus(0);


            return dataContext.changesSave().then(function () {
                return Q.resolve(app.showMessage("Thank you, You have clocked in.", "Clock in successful", ['Close'])).then(function (data) {
                    getTimeKeepingData();
                });
            });
        }

        function deleteClockRecord(objModel) {
            if (objModel.clockDataId === 0) {
                return Q.resolve(app.showMessage("This clock record was added as a result of leave, you cannot delete this record, the leave must be canceled to delete this record", "Locked record", ['Close'])).then(function (data) {
                });
            } else {
                return app.showMessage("Are you sure you want to delete this clock record for: \n" + objModel.displayDate + "\n at " + objModel.time, "Confirm Delete", ['Yes', 'No']).then(function (result) {
                    if (result === "Yes") {
                        var clockRecord = ko.observable();
                        return dataContext.getClockRecord(clockRecord, objModel.clockDataId).then(function () {
                            clockRecord().dataStatus(8);
                            clockRecord().entityAspect.setModified();
                            return dataContext.changesSave().then(function () {
                                return getTimeKeepingData();
                            });
                        });
                    } else {
                    }
                });
            }
        }

        function cancel() {
            return history.go(-1);
        }

        function download(strData, strFileName, strMimeType) {
            var D = document, A = arguments, a = D.createElement("a"),
                 d = A[0], n = A[1], t = A[2] || "application/pdf";

            var newdata = "data:" + strMimeType + ";base64," + escape(strData);

            //build download link:
            a.href = newdata;

            if ('download' in a) {
                a.setAttribute("download", n);
                a.innerHTML = "downloading...";
                D.body.appendChild(a);
                setTimeout(function () {
                    var e = D.createEvent("MouseEvents");

                    e.initMouseEvent("click", true, false, window, 0, 0, 0, 0, 0, false, false, false, false, 0, null
                );
                    a.dispatchEvent(e);
                    D.body.removeChild(a);
                }, 66);

                return true;
            };

        }

        function exportToFile() {
            var startDate = null;
            var endDate = null;

            var staffId = selectedStaff();
            var start = $("#clockStartDate").datepicker('getDate');
            var end = $("#clockEndDate").datepicker('getDate');

            startDate = moment(start).format('YYYY-MM-DD');
            endDate = moment(end).format('YYYY-MM-DD');
            dataContext.exportMarkup(csvFile, staffId.staffClockId(), startDate, endDate).then(function (result) {
                if (result) {
                    download(csvFile().$value, "ClockData", 'application/vnd.ms-excel');
                }
            });
        }

        function reportToCsv(parameters) {
            var htmlElement = document.getElementById("contactsDiv").innerHTML;
            var markup = {
                Markup: clockingData()
            }
            dataContext.exportMarkup(csvFile, clockingData()).then(function (result) {
                if (result) {
                    download(csvFile().$value, "ClockData", 'text/txt');
                }
            });
        }

        //#region Public Members
        var vm = {
            activate: activate,
            viewAttached: viewAttached,
            clockingData: clockingData,
            staff: staff,
            cmbStaffMemberChange: cmbStaffMemberChange,
            getTimeKeepingData: getTimeKeepingData,
            selectedStaff: selectedStaff,
            aprroveClockRecord: aprroveClockRecord,
            enableFiltering: enableFiltering,
            isloading: isLoading,
            authorised: authorised,
            timeTakenTotal: timeDebtTotal,
            overTimeTotal: overTimeTotal,
            editClockRecord: editClockRecord,
            addClockRecord: addClockRecord,
            deleteClockRecord: deleteClockRecord,
            cancel: cancel,
            exportToFile: exportToFile,
            clockme: clockmeNow,
            netBalance: netBalance,
            test: clockme
        };
        return vm;
    });