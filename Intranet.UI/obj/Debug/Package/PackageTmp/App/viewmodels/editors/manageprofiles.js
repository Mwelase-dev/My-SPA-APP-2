define(["services/datacontext", "viewmodels/editors/baseeditor", "viewmodels/editors/profile", "durandal/app"],
    function (dtx,baseEditor,profileEditor,app) {
        var registeredUsers = ko.observable();
        function init(device) {
            return dtx.getRegisteredUsers(registeredUsers, device.clockDeviceId());
        }
         
        function activate() {
            return;
        }

        function editProfile(objModel) {
            return baseEditor.objEdit(new profileEditor(objModel)).then(function () {
            });
        }

        function deleteProfile(objModel) {
            Q.resolve(app.showMessage("Are you sure you want to <b>Delete</b> the profile?", "Delete Profile", ['No', 'Yes'])).then(function (data) {

                if (data == "No")
                    return;

                objModel.recordStatus(dtx.recStatus.statusDelete);
                objModel.entityAspect.setModified();

                return dtx.changesSave().then(function () {
                });


            });
        }

        var vm = function (id) {
            init(id);
            this.registeredUsers = registeredUsers;
            this.editProfile = editProfile;
            this.deleteProfile = deleteProfile;
        };
        return vm;
    });