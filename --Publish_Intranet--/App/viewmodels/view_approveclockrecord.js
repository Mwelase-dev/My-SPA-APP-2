
define(['services/logger', 'services/datacontext', 'durandal/app'],
    function (logger, datacontext, app) {

        //view variables
        var staff = ko.observableArray();
        var clockRecords = ko.observableArray();
        var selectedStaff = ko.observable();
        var enableFiltering = ko.observable(true);
        var userRoles = ko.observableArray();
        var staffMem = ko.observableArray();
        var allClockRecords = ko.observableArray();

        function getallunapprovedclockrecords() {
            return datacontext.getUnApprovedClockRecordsAll(allClockRecords);
        }

        function recordRefresh() {
            if (selectedStaff()) {
                return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff());
            }
        }

        //#region Internal Methods
        function activate() {
            recordRefresh();
            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                if (datacontext.isInRole(userRoles(), datacontext.userRoleEnum.manager))
                    return datacontext.getCurrentUser(staffMem).then(function () {
                        return datacontext.getAllStaff(staff, staffMem).then(function () {
                            return getallunapprovedclockrecords();
                        });
                    });
                else return datacontext.getAllStaff(staff);
            });
        }

        function getunapprovedclockrecords() {
            if (selectedStaff() != null) {
                return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff());
            } else {
                return getallunapprovedclockrecords();
            }
        }

        function aprroveclockrecord(objModel) {
            var clockRecord = ko.observable();
            return datacontext.getClockRecord(clockRecord, objModel.clockDataId()).then(function () {
                clockRecord().dataStatus(1);
                clockRecord().entityAspect.setModified();

                return datacontext.changesSave().then(function () {
                    clockRecord().entityAspect.setDetached();
                    return datacontext.getUnApprovedClockRecords(allClockRecords, selectedStaff());
                });
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
        }

        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,

            denyclockrecord: denyclockrecord,
            aprroveclockrecord: aprroveclockrecord,
            cmbStaffMemberChanged: cmbStaffMemberChanged,

            staff: staff,
            selectedStaff: selectedStaff,

            enableFiltering: enableFiltering,
            staffClockData: clockRecords,
            getunapprovedclockrecords: getunapprovedclockrecords,


            getallunapprovedclockrecordsonload: allClockRecords,
            getallunapprovedclockrecords: getallunapprovedclockrecords
        };
        return vm;
        //#endregion
    });