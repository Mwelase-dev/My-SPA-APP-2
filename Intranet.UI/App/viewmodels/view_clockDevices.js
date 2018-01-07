define(['services/logger', 'services/datacontext', 'durandal/app', 'viewmodels/editors/baseeditor', 'viewmodels/editors/updateclockdevice', 'viewmodels/editors/clockdeviceadd', 'services/helpers', 'viewmodels/editors/manageprofiles', 'viewmodels/editors/clockdeviceUsersEditor'],
    function (logger, dataContext, app, baseEditor, deviceEditor, clockDeviceAdd, helpers, manageprofiles, clockdeviceUserEditor) {
        var clockDevices = ko.observableArray();

        function activate() {
                return refreshClockDevices();
        }

        function refreshClockDevices() {
                clockDevices([]);

                return dataContext.getClockDevices(clockDevices).then(function () {
                    var test = clockDevices();
                });
        }

        function deleteDevice(objModel) {

            Q.resolve(app.showMessage("Are you sure you want to <b>Delete</b> the device?", "Delete Device", ['No', 'Yes'])).then(function (data) {

                if (data == "No")
                    return;

                objModel.recordStatus(dataContext.recStatus.statusDelete);
                objModel.entityAspect.setModified();

                return dataContext.changesSave().then(function () {
                    return refreshClockDevices();
                });


            });
        }

        function updateDevice(objMOdel) {

            return baseEditor.objEdit(new deviceEditor(objMOdel)).then(function () {
                refreshClockDevices();
            });
        }
         
        function addClockDevice() {

            return baseEditor.objEdit(new clockDeviceAdd()).then(function () {
                refreshClockDevices();
            });
        }

        function cancel() {
            return helpers.returnToPrevPage();
        }

        function manageUsers(objModel) {
            return baseEditor.objEdit(new manageprofiles(objModel)).then(function() {
                refreshClockDevices();
            });
        }

        function setUsers(objModel) {
            return baseEditor.objEdit(new clockdeviceUserEditor(objModel)).then(function () {
                refreshClockDevices();
            });
        }

        var vm = {
            activate: activate,

            clockDevices: clockDevices,

            deleteDevice: deleteDevice,
            updateDevice: updateDevice,
 
            addClockDevice: addClockDevice,

            cancel: cancel,
            manageUsers: manageUsers,
            setUsers: setUsers


        };

        return vm;

    });