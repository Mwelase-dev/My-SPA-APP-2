define(['services/datacontext'],
    function(datacontext) {

        function activate() {
            
        }

        var vm = function (serviceProvider) {
            this.activate = activate;
            this.serviceProvider = serviceProvider;
        };

        return vm;

    });