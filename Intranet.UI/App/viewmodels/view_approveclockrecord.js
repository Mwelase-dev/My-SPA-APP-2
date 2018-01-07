
define(['services/logger', 'services/datacontext', 'durandal/app', 'services/helpers', 'viewmodels/editors/createallnewclockrecord', 'viewmodels/editors/baseeditor'],
    function (logger, datacontext, app, helpers, createclockrecord, editor) {
        //view variables
        var staff = ko.observableArray();
        var clockRecords = ko.observableArray();
        var selectedStaff = ko.observable();
        var enableFiltering = ko.observable(true);
        var userRoles = ko.observableArray();
        var staffMem = ko.observableArray();
        var allClockRecords = ko.observableArray();
        var enableClockIn = ko.observable(false);
        var allStaff = ko.observableArray();

        var isLoading = ko.observable(false);

        var isPartOfAssistants = ko.observable(false);
        var isPartOfManagers = ko.observable(false);
        var isPartOfHr = ko.observable(false);
        var isNormal = ko.observable(false);
        var staffToSelect = ko.observableArray();
        var managerandstaff = ko.observableArray();
        var managerandstaffbeneath = ko.observableArray();
        function getallunapprovedclockrecords() {
          
            var test = allClockRecords();
            return datacontext.getUnApprovedClockRecordsAll(allClockRecords).then(function() {
         
            });
        }

        function recordRefresh() {
            allClockRecords.removeAll();
            if (selectedStaff()) {
                return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff()).then(function() {
                    var test = allClockRecords();
                });
            }
        }

        function sortStaff() {
            staff().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

        //#region Internal Methods
        function activate() {
            //recordRefresh();
            allClockRecords.removeAll();
            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                if (datacontext.isInRole(userRoles(), datacontext.userRoleEnum.manager))
                    return datacontext.getCurrentUser(staffMem).then(function () {
                        //return datacontext.getAllStaff(staff, staffMem).then(function () {
                        isPartOfManagers(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.manager));
                        isPartOfAssistants(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.assistants));
                        isPartOfHr(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.humanResource));
                        isNormal(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.humanResource) || datacontext.isInRole(userRoles(), datacontext.userRoleEnum.assistants));


                        if (isPartOfHr()) {
                            staffToSelect([]);
                            return datacontext.allStaff(allStaff).then(function () {
                                //sortStaff();
                                staffToSelect(allStaff());
                                var test2 = staff();
                            });

                        } else if (isPartOfManagers()) {
                            staffToSelect([]);
                            return datacontext.getStaffByManager(managerandstaffbeneath, staffMem).then(function () {
                                staffToSelect(managerandstaffbeneath());
                            });
                        } else if (isPartOfAssistants()) {
                            staffToSelect([]);
                            return datacontext.getStaffbyId(managerandstaff, staff().staffManager1Id()).then(function () {
                                staffToSelect.push(managerandstaff());
                                managerandstaff(staff());
                                managerandstaff(managerandstaff());
                                staffToSelect.splice(1, 0, staff());
                                var test = staffToSelect();
                                return datacontext.getStaffbyId(manager, staff().staffManager1Id()).then(function () {
                                    managerandstaff(manager());
                                });
                            });
                        }


                            //return getallunapprovedclockrecords();
                        //});
                    });
                else return datacontext.getAllStaff(staff);
            });
        }

        function getunapprovedclockrecords() {
            allClockRecords([]);
            isLoading(true);
            if (selectedStaff() != null) {
                enableClockIn(true);
                return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff()).then(function () {
                    var test = allClockRecords();
                    isLoading(false);
                });
            } else {

                return getallunapprovedclockrecords();
            }
        }

        function aprroveclockrecord(objModel) {
            var offsiteclockin = objModel.clockDateTime();

            var date = new Date(offsiteclockin);

            //2 hour zime zone

            var hours = date.getHours();
            var minutes = date.getMinutes();
            var clockRecord = ko.observable();


            date.setHours(hours + 2);
            date.setMinutes(minutes);


            return datacontext.getClockRecord(clockRecord, objModel.clockDataId()).then(function () {
                clockRecord().dataStatus(1);
                clockRecord().entityAspect.setModified();
                clockRecord().clockDateTime(date);

                return datacontext.changesSave().then(function () {
                    clockRecord().entityAspect.setDetached();
                    return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff());
                });
            });
        }
         
        function approveMultiple(objModel) {
            var apps = objModel.getallunapprovedclockrecordsonload();
            for (var i = 0; i < apps.length; i++) {

                for (var j = 0; j < apps[i].staffClockData().length; j++) {

                    var offsiteclockin = apps[i].staffClockData()[j].clockDateTime();

                    var approved = apps[i].staffClockData()[j].approve() == true;
                    var stats = apps[i].staffClockData()[j].dataStatus() == datacontext.clockRecordStatusEnum.pending;

                    if (approved == stats) {

                        apps[i].staffClockData()[j].dataStatus(1);
                        apps[i].staffClockData()[j].entityAspect.setModified();
                    }
                }

            }
            datacontext.changesSave().then(function () {
                app.showMessage("Clock amendment approved. An approval email has been sent to the applicant", 'Success', ['Close']);
                return getunapprovedclockrecords();
            });
        }

        function deleteRecord(objModel) {
            return app.showMessage("Are you sure you want to delete this clock record?", "Confirm Delete", ['Yes', 'No']).then(function (result) {
                if (result === "Yes") {

                    objModel.entityAspect.setDeleted();
                    objModel.clockRecord().dataStatus(8);
                    objModel.clockRecord().entityAspect.setModified();
                    return datacontext.changesSave().then(function() {
                        isLoading(true);
                        return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff()).then(function() {
                            isLoading(false);
                        });
                    });
                }
            });

            };

        function denyclockrecord(objModel) {
            var dataHolder = ko.observable();
            var clockRecord = ko.observable();
            return datacontext.getClockRecord(clockRecord, objModel.clockDataId()).then(function () {
                if (clockRecord().dataStatus() === 111112) {//denying a new record
                    clockRecord().dataStatus(3);
                    clockRecord().entityAspect.setDeleted();
                } else {
                    clockRecord().dataStatus(3);//denying an existing record
                    clockRecord().entityAspect.setModified();
                }

                var comments = clockRecord().comments();
                comments += ". Update to clock record has been denied";
                clockRecord().comments(comments);

                return datacontext.changesSave().then(function () {
                    clockRecord().entityAspect.setDetached();
                    return datacontext.emailClockRecDenied(clockRecord().clockDataId(), dataHolder).then(function () {
                        app.showMessage("The staff member will be notified via email.", "Denied", ["Close"]);
                        return activate();
                    });
                });
            });
        }

        function cmbStaffMemberChanged() {
            enableFiltering(selectedStaff() != null);
            enableClockIn(selectedStaff() != null || selectedStaff() != undefined);
            var test = selectedStaff();
        }

        function cancel() {
            return helpers.returnToPrevPage();
        }

        var clockme = function () {
            var clockModel = datacontext.manufactureEntity("StaffClockModel");
            clockModel.staffId(selectedStaff());
            //clockModel.clockDateTime(new Date(objModel.date));
            return editor.objEdit(new createclockrecord(clockModel)).then(function () {
                //getTimeKeepingData();
            });
        }
        //#endregion

        function selectAll() {
            //$("#checkAll").change(function () {
            //    $("input:checkbox").prop('checked', $(this).prop("checked"));
            //});

            $(document).ready(function () {
                $('.check:button').toggle(function () {
                    $('input:checkbox').attr('checked', 'checked');
                    $(this).val('uncheck all');
                }, function () {
                    $('input:checkbox').removeAttr('checked');
                    $(this).val('check all');
                });
            });
        }
        //$('.selectall').click(function () {
        //    if ($(this).is(':checked')) {
        //        $('div input').attr('checked', true);
        //    } else {
        //        $('div input').attr('checked', false);
        //    }
        //});
      
        //#region Public Members
        var vm = {
            activate: activate,

            denyclockrecord: denyclockrecord,
            aprroveclockrecord: aprroveclockrecord,
            cmbStaffMemberChanged: cmbStaffMemberChanged,

            staff: staff,
            selectedStaff: selectedStaff,

            enableFiltering: enableFiltering,
            staffClockInfo: clockRecords,
            getunapprovedclockrecords: getunapprovedclockrecords,


            getallunapprovedclockrecordsonload: allClockRecords,
            getallunapprovedclockrecords: getallunapprovedclockrecords,
            cancel: cancel,
            clockme: clockme,
            enableClockIn: enableClockIn,
            deleteRecord: deleteRecord,
            isloading: isLoading,
            approveMultiple: approveMultiple,
            selectAll: selectAll,
            staffToSelect: staffToSelect,

        };
        return vm;
        //#endregion
    });