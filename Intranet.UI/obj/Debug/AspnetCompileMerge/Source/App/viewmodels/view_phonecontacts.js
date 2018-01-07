define(['services/logger', 'services/datacontext', 'durandal/app', 'viewmodels/editors/baseeditor', 'viewmodels/editors/updatecontact', 'viewmodels/editors/contactadd','services/helpers'],
    function (logger, dataContext, app, baseEditor, contactEditor, contactAdd,helpers) {

        var staff = ko.observableArray();
        var contacts = ko.observableArray();

        function activate() {
            return refreshContacts();
        }

        function refreshContacts() {

            return dataContext.getCurrentUser(staff).then(function () {
                contacts([]);

                return dataContext.getPhoneContacts(contacts, staff().staffId());
            });
        }

        function deletecontact(objModel) {

            Q.resolve(app.showMessage("Are you sure you want to <b>Delete</b> the contact?", "Delete Contact", ['No', 'Yes'])).then(function (data) {

                if (data == "No")
                    return;

                objModel.recordStatus(dataContext.recStatus.statusDelete);
                objModel.entityAspect.setModified();

                return dataContext.changesSave().then(function () {
                    return refreshContacts();
                });


            });
        }

        function updateContact(objMOdel) {

            return baseEditor.objEdit(new contactEditor(objMOdel)).then(function () {
                refreshContacts();
            });
        }

        function addContact() {

            return baseEditor.objEdit(new contactAdd()).then(function () {
                refreshContacts();
            });
        }

        function dialContact(objModel) {
            dataContext.dialPhoneContact(objModel.contactNumber());
        }


        function hangup(objModel) {
            
        }

        

        function cancel() {
            return helpers.returnToPrevPage();
        }

        var vm = {
            activate: activate,

            contacts:contacts,
            staff:staff,

            deleteContact: deletecontact,
            updateContact: updateContact,
            addContact: addContact,
            dialContact: dialContact,
            cancel: cancel,
            hangup: hangup
        };

        return vm;

    });