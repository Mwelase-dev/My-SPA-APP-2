define(['services/datacontext'],
    function (dataContext) {

        var contact = ko.observable();

        function activate() {
            var staff = ko.observable();

            return dataContext.getCurrentUser(staff).then(function () {
                var ctx = dataContext.manufactureEntity("StaffContactModel");

                ctx.staffId = staff().staffId();

                contact(ctx);
            });
        }

        function addContact(objModel) {

            alert(objModel);

        }

        var vm = {
            activate: activate,
            contact: contact,
            
            addContact: addContact,
        };

        return vm;

    });