define(['services/logger', 'services/datacontext', 'viewmodels/editors/baseeditor'],
    function (logger,datacontext, editor) {
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

                logger.log("Is admin = " + isAdmin(), isAdmin, isAdmin, true);

                return itemsRefresh();
            });
        }
        function itemsCurrent() {
            showingCurrent(true);
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusActive, false);
        }
        function itemsArchive() {
            showingCurrent(false);
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusArchive, false);
        }
        function itemsRefresh(status) {
            showingCurrent(true);
            if (status) {
                return datacontext.getAnnouncements(announcements, status, true);
            }
            return datacontext.getAnnouncements(announcements, datacontext.recStatus.statusActive, true);
        }
        function announcementArchive(annObject) {
            return datacontext.promptAndProceed(datacontext.recStatus.statusArchive, annObject, title);
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
