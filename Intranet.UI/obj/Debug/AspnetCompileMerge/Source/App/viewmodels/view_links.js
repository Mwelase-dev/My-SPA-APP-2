define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/link', 'durandal/plugins/router'],
    function (datacontext, editor, linkeditor, router) {
        var categories = ko.observableArray();
        var links = ko.observableArray();
        var isAdmin = ko.observable(false);
        var userRoles = ko.observableArray();

        function activate() {

            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.intranetAdmins));
                //return datacontext.getLinks(links, datacontext.recStatus.statusActive).then(function () {
                //return datacontext.getLinkCategories(categories);
                return datacontext.getLinkCategories(categories, datacontext.recStatus.statusActive, true);
            });
        }


        //#region Categories
        function categorAdd() {
            event.stopPropagation();
            return editor.objAdd(editor.objModel.entityNames.linkCategory).then(function () {
                activate();
            });
        }

        function categorEdit(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            return editor.objEdit(objModel).then(function () {
                activate();
            });
        }

        function categorDelete(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            editor.objDelete(objModel).then(function () {
                activate();


            });
        }
        //#endregion


        //#region Links
        function linkAdd() {
            // We need the object here before we can pass it into the generic editor
            return linkEdit(datacontext.manufactureEntity(editor.objModel.entityNames.linkName)).then(function () {
                return activate();
            });
        }

        function linkEdit(objModal) {
            return editor.objEdit(new linkeditor(objModal, categories)).then(function () {
                return activate();
            });
        }

        function linkDelete(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            editor.objDelete(objModel).then(function () {
                activate();


            });

        }
        //#endregion


        //#region Public Members
        var vm = {
            activate: activate,
            categories: categories,
            links: links,
            isAdmin: isAdmin,
            catAdd: categorAdd,
            catEdit: categorEdit,
            catDelete: categorDelete,
            linkAdd: linkAdd,
            linkEdit: linkEdit,
            linkDelete: linkDelete
        };
        return vm;
        //#endregion

    });