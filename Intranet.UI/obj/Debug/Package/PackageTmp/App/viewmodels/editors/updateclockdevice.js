define(['services/datacontext', 'viewmodels/editors/printerpropertyadd', 'viewmodels/editors/baseeditor'],
    function (datacontext, printerpropertyEditor, baseEditor) {

        function activate() {
            
        }
         
        var vm = function (clockDevice) {
            this.activate = activate;
            this.clockDevice = clockDevice;
        };

        return vm;

    });