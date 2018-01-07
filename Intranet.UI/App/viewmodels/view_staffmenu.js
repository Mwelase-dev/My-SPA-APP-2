define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/staffDetails'],
    function (datacontext, editor, staffEditor) {
        var staffMeme = ko.observable();

        var tonerOrders = ko.observable(false);
        var companyPhoneUsage = ko.observable(false);
        var approveClockRecords = ko.observable(false);
        var staff = ko.observable();
        var userRoles = ko.observableArray();

        var manager = ko.observable(false);
        var intranetAdmin = ko.observable(false);
        var hr = ko.observable(false);

        //#region Internal Methods
        function activate() {

            return datacontext.getCurrentUserRoles(userRoles).then(function () { //the roles tht the user has 
                setUserRoles();
            });

        }
        //#endregion

        function sendUnApprovedClockRecords() {
            datacontext.sendUnApprovedClockRecords();
        }

        function sendReminderAboutMissedClockIn() {
            datacontext.sendReminderAboutMissedClockIn();
        }

        function applyLEaveForEveryone() {
            datacontext.applyLEaveForEveryone();
        }

        function isUserInRole(role) {

            return $.inArray(role, userRoles()) > -1;

        };

        function setUserRoles() {

            if (userRoles().length < 1)
                return;
             
            manager(isUserInRole(datacontext.userRoleEnum.manager));
            intranetAdmin(isUserInRole(datacontext.userRoleEnum.intranetAdmins));
            hr(isUserInRole(datacontext.userRoleEnum.humanResource));

            //approveClockRecords(isUserInRole(datacontext.userRoleEnum.manager) || isUserInRole(datacontext.userRoleEnum.humanResource));
            //tonerOrders(isUserInRole(datacontext.userRoleEnum.intranetAdmins));
        }


        function staffEdit() {

            var tempSTaff = ko.observable();

            return datacontext.getCurrentUser(staffMeme).then(function () {
                return editor.objEdit(new staffEditor(staffMeme())).then(function () {
                });
            });
        }

        //#region Public Members
        var vm = {
            activate: activate,

            //menu rules (WIP)
            tonerOrders: tonerOrders,
            companyPhoneUsage: companyPhoneUsage,
            approveClockRecords: approveClockRecords,

            manager: manager,
            intranetAdmin: intranetAdmin,
            hr: hr,
            staffEdit: staffEdit,

            SendUnApprovedClockRecords: sendUnApprovedClockRecords,
            SendReminderAboutMissedClockIn: sendReminderAboutMissedClockIn,
            ApplyLEaveForEveryone: applyLEaveForEveryone
         
        };
        return vm;
        //#endregion
    });