define(['services/datacontext', 'viewmodels/editors/baseeditor'],
    function (datacontext, editor) {
        var aRandomThought = ko.observable();
        var isAdmin        = ko.observable(false);
        var userRoles = ko.observableArray();

        //#region Internal Methods
        function activate() {
            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.intranetAdmins));
                return datacontext.getRandomThought(aRandomThought);
            });
        }
        //#endregion

        //#region Adding/Editing item
        function thoughtAdd() {
            return editor.objAdd(editor.objModel.entityNames.thoughtName);
        }
        function thoughtEdit(objModel) {
            return editor.objEdit(objModel);
        }
        function thoughtDelete(objModel) {
            return editor.objDelete(objModel);
        }
        //#endregion
        
        //#region Public Members
        var vm = {
            activate       : activate,
            isAdmin        : isAdmin,
            aRandomThought : aRandomThought,
            thoughtAdd     : thoughtAdd,
            thoughtEdit    : thoughtEdit,
            thoughtDelete  : thoughtDelete
        };
        return vm;
        //#endregion
    });