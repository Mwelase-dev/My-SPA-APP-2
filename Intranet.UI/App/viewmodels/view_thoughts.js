define(['services/datacontext', 'viewmodels/editors/baseeditor'],
    function (datacontext, editor) {
        var isAdmin = ko.observable(false);
        var thoughts = ko.observableArray();
        var userRoles = ko.observableArray();

        //#region Internal Methods
        function activate() {
            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.intranetAdmins));
                return datacontext.getThoughtList(thoughts);
            });
        }
        function objEdit(objModel) {
            return editor.objEdit(objModel).then(function () {
                refreshThoughtList().then(function () {
                    activate();
                });
            });
        }

        function refreshThoughtList() {
            return datacontext.getThoughtList(thoughts);
        }

        function objDelete(objModel) {
            editor.objDelete(objModel).then(function () {
                refreshThoughtList().then(function () {
                    activate();
                });
            });
        }

        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,
            isAdmin: isAdmin,
            thoughts: thoughts,
            edit: objEdit,
            del: objDelete,
            refreshThoughtList: refreshThoughtList
        };
        return vm;
        //#endregion
    });