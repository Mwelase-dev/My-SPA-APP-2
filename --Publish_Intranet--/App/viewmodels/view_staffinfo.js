define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/leaveapplication', 'durandal/app', 'viewmodels/editors/leavedata'],
    function (dataContext, baseEditor, leaveAppEditor, app, leavedata) {

        var staff = ko.observableArray();
        var userRoles = ko.observableArray();
        var enableFiltering = ko.observable(true);
        var staffMeme = ko.observable();
        var selectedStaff = ko.observable();
        var staffInfo = ko.observable();
        var canFilter = ko.observable(false);

        function activate() {
            return dataContext.getCurrentUserRoles(userRoles).then(function () {
                if (isInRole(dataContext.userRoleEnum.intranetAdmins) || isInRole(dataContext.userRoleEnum.humanResource)) {
                    return dataContext.getAllStaff(staff);
                }
                else if (dataContext.isInRole(userRoles(), dataContext.userRoleEnum.manager)) {
                    return dataContext.getCurrentUser(staffMeme).then(function () {
                        return dataContext.getAllStaff(staff, staffMeme);
                    });
                } else {
                    return Q.fcall(function () {
                        //todo - jay : clean up may cause perfomance issues
                        return dataContext.getCurrentUser(staffMeme).then(function () {
                            selectedStaff(staffMeme().staffClockId());
                            enableFiltering(selectedStaff() != null);
                            staff(staffMeme());
                        });

                    });
                }
            });
        }

        function staffDetails() {
            return dataContext.getStaffbyId(staffInfo, selectedStaff());
        }

        function cmbStaffMemberChange() {
            canFilter(selectedStaff() != null);
        }


        var vm = {
            activate: activate,
            staff: staff,
            staffInfo: staffInfo,

            enableFiltering: enableFiltering,
            canFilter: canFilter,

            cmbStaffMemberChange: cmbStaffMemberChange,
            selectedStaff:selectedStaff,
            staffDetails: staffDetails,
        };

        return vm;

    });