define(['services/datacontext'],
    function (dataContext) {

        var contact = ko.observable();

        function activate() {
            var staff = ko.observable();

            return dataContext.getCurrentUser(staff).then(function () {
                var ctx = dataContext.manufactureEntity("StaffContactModel");

                ctx.staffId = staff().staffId;
                ctx.recordStatus = ko.observable(dataContext.recStatus.statusActive);

                contact(ctx);
            });
        }

        function addContact() {

            this.clickSave();

        }

        var vm = function () {
            this.activate = activate;
            this.contact = contact;
            this.addContact = addContact;
        };

        return vm;

    });