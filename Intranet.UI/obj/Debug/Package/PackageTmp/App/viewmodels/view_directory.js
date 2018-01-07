define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/branch', 'viewmodels/editors/staff', 'signalr', '/signalr/hubs'],
    function (datacontext, editor, branchEditor, staffEditor, signalr,theHub) {
        var branches = ko.observableArray();
        var isAdmin = ko.observable();
        var userRoles = ko.observableArray();
        var currentuser = ko.observable();

        var phoneStatus = ko.observable();
        //#region Internal Methods
        function activate() {
            return datacontext.getCurrentUser1(currentuser).then(function () {
                connctHub();
                return itemsRefresh();
            });
        }

        function compositionComplete() {
           
        }

        function itemsRefresh() {

            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                isAdmin(datacontext.isInRole(userRoles(), datacontext.userRoleEnum.humanResource));
                return datacontext.getBranches(branches).then(function () {
                    var test = branches();
                    // phoneStatus(branches().branchDivisions().divisionStaff().staffPhoneStatus);
                });
            });
        }

        //#region Company CRUD
        function companyAdd() {
           
            return editor.objEdit(new branchEditor(datacontext.manufactureEntity(editor.objModel.entityNames.branchName),
                branches)).then(function () {
                    return itemsRefresh();
                });
        }
        function companyEdit(objModel, event) {
            //Prevent the Accordion from expanding
            event.stopPropagation();
            return editor.objEdit(new branchEditor(objModel)).then(function () {
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
                    var test = tempSTaff();
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
        
        function connctHub() {
            ////debugging ==> $.connection.hub.url = "http://localhost:37992/signalr"; 
            ////live ==> $.connection.hub.url = "http://intranet/signalr"; 
           ////Pe Domain 192.168.16.2
           
            $.connection.hub.url = "http://intranet/signalr";
            // Declare a proxy to reference the hub.
            var phonestatus = $.connection.phoneHub;
            // Create a function that the hub can call to broadcast messages.
            phonestatus.client.addmessage = function (name, message,staffid) {
                // Add the message to the page.
                $('#phonestatus').text(message);
                 
                updatePhoneStatus(message, staffid);
            };

            // Start the connection.
            $.connection.hub.start().
                done(function () { console.log('Now connected, connection ID=' + $.connection.phoneHub.connection.id); })
               .fail(function () { console.log('Could not connect'); });
        }

        function updatePhoneStatus(message, staffid) {
            var tempSTaff = ko.observable();
            return datacontext.getStaffbyId(tempSTaff, staffid).then(function () {
                return save(tempSTaff, message).then(function () {
                    return itemsRefresh();
                });
            });
        }

        //This is not needed as the object is updated and then a signal r message is sent. 
        //But I will leave this here incase somoething went wrong at database level and the phone status update failed.
        function save(objModel, message) {
            objModel.staffPhoneStatus = message;
            this.clickSave();
            return;
        }
        //This is not needed as the object is updated and then a signal r message is sent.
        //But I will leave this here incase somoething went wrong at database level and the phone status update failed.

        //#region Public Members
        var vm = {
            activate: activate,
            compositionComplete: compositionComplete,
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
            staffTellExt: staffTellExt,
            phoneStatus: phoneStatus
        };
        return vm;
        //#endregion
    });