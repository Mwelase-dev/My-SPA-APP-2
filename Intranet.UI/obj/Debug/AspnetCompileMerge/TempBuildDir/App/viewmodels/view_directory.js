define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/branch', 'viewmodels/editors/staff'],
    function (datacontext, editor, branchEditor, staffEditor) {
        var branches = ko.observableArray();
        var isAdmin = ko.observable();
        var userRoles = ko.observableArray();
        var currentuser = ko.observable();
        //#region Internal Methods
        function activate() {
            return datacontext.getCurrentUser1(currentuser).then(function () {
                return itemsRefresh();
            });
        }
        function itemsRefresh() {

            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.humanResource));
                return datacontext.getBranches(branches).then(function () {
                    var test = branches();
                });
            });
        }

        //#region Company CRUD
        function companyAdd() {
            return editor.objEdit(new branchEditor(datacontext.manufactureEntity(editor.objModel.entityNames.branchName), branches)).then(function () {
                return itemsRefresh();
            });
        }
        function companyEdit(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            return editor.objEdit(new branchEditor(objModel)).then(function () {
                return itemsRefresh();
            });
        }
        function companyDelete(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            return editor.objDelete(objModel).then(function () {
                return itemsRefresh();
            });
        }
        //#endregion

        //#region Division CRUD
        function divisionEdit(objModel) {
            return editor.objEdit(objModel).then(function () {
                return itemsRefresh();
            });
        }
        function divisionDelete(objModel) {
            return editor.objDelete(objModel).then(function () {
                return itemsRefresh();
            });
        }
        //#endregion

        //#region Staff CRUD
        function staffEdit(objModel) {

            var tempSTaff = ko.observable();

            return datacontext.getStaffbyId(tempSTaff, objModel.staffId()).then(function () {
                return editor.objEdit(new staffEditor(tempSTaff())).then(function () {
                    return itemsRefresh();
                });
            });
        }
        function staffDelete(objModel) {
            return editor.objDelete(objModel).then(function () {
                //return itemsRefresh();
            });
        }

        function staffTellExt(selectedStaffTel) {
            return datacontext.dial(currentuser().staffTellExt(), selectedStaffTel.staffTellExt());
        };

        //#endregion
        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,
            branches: branches,
            itemsRefresh: itemsRefresh,
            isAdmin: isAdmin,

            branchAdd: companyAdd,
            branchEdit: companyEdit,
            branchDelete: companyDelete,

            divisionEdit: divisionEdit,
            divisionDelete: divisionDelete,

            staffEdit: staffEdit,
            staffDelete: staffDelete,
            staffTellExt: staffTellExt
        };
        return vm;
        //#endregion
    });