define(['services/datacontext', 'viewmodels/editors/printerpropertyadd', 'viewmodels/editors/baseeditor'],
    function (dataContext, printerpropertyEditor, baseEditor) {

        var clockDevice = ko.observable();

        function activate() {
                    var ctx = dataContext.manufactureEntity("ClockDeviceModel");
                    ctx.recordStatus = ko.observable(dataContext.recStatus.statusActive);
                    clockDevice(ctx);

        }

        function addClockingDevice() {
            var test = clockDevice();
            this.clickSave();
        }

      
        var vm = function () {
            this.activate = activate;
            this.clockDevice = clockDevice;
            this.addClockingDevice = addClockingDevice;
        };

        return vm;

    });