define(['services/datacontext'],
    function(datacontext) {

        function activate() {
            
        }

        var vm = function(phoneContact) {
            this.activate = activate;
            this.contact = phoneContact;};

        return vm;

    });