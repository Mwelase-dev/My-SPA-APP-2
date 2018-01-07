define(['services/logger', 'services/datacontext', 'viewmodels/editors/baseeditor', 'durandal/app'],
    function (logger,datacontext, editor,app) {
        var title          = 'Announcement';
        var announcements  = ko.observableArray();
        var showingCurrent = ko.observable(true);
        var isWorking      = ko.observable(false);
        var isAdmin        = ko.observable(false);

        var userRoles = ko.observableArray();
        
        //#region Internal Methods
        function activate() {

            return datacontext.getCurrentUserRoles(userRoles).then(function() {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.intranetAdmins));

                //logger.log("Is admin = " + isAdmin(), isAdmin, isAdmin, true);

                return itemsRefresh();
            });
        }
        function itemsCurrent() {
            showingCurrent(true);
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusActive, false).then(function () {
                itemsRefresh(datacontext.recStatus.statusActive);
            });
        }
        function itemsArchive() {
            showingCurrent(false);
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusArchive, false).then(function() {
                //itemsRefresh(datacontext.recStatus.statusArchive);
            });
        }
        function itemsRefresh(status) {
            showingCurrent(true);
            if (status) {
                return datacontext.getAnnouncements(announcements, status, true);
            }
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusActive, true);
        }
        function announcementArchive(annObject) {
       
            return app.showMessage('Do you want to Archive this Announcement', ['Archive Announcement'],['No', 'Yes']).then(function (result) {
                if (result === "Yes") {

                    annObject.recordStatus(datacontext.recStatus.statusArchive);
                    annObject.entityAspect.setModified();
                    return datacontext.changesSave().then(function () {
                        return itemsCurrent();
                    });
                } else {
                    return;
                }
            });

               // return datacontext.promptAndProceed(datacontext.recStatus.statusArchive, annObject, title);
        }
        //#endregion
        
        //#region Adding/Editing item
        function announcementView(model) {
            return editor.objEdit(model);
        }
        function announcementNew() {
            return editor.objAdd(editor.objModel.entityNames.announcementName).then(function() {
                itemsRefresh(datacontext.recStatus.statusActive, true);
            });
        }
        function announcementDelete(model) {
            return editor.objDelete(model).then(function () { return itemsRefresh(); });
        }
        //#endregion

        //#region Public Members
        var vm = {
            activate            : activate,
            announcements       : announcements,
            itemsCurrent        : itemsCurrent,
            itemsArchive        : itemsArchive,
            itemsRefresh        : itemsRefresh,
            showingCurrent      : showingCurrent,
            
            announcementNew     : announcementNew,
            announcementView    : announcementView,
            announcementArchive : announcementArchive,
            announcementDelete  : announcementDelete,
            isWorking           : isWorking,
            isAdmin             : isAdmin
        };
        return vm;
        //#endregion
    });
