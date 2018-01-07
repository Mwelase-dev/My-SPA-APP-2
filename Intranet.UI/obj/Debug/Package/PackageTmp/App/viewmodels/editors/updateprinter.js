define(['services/datacontext', 'viewmodels/editors/printerpropertyadd', 'viewmodels/editors/baseeditor', 'durandal/app'],
    function (datacontext, printerpropertyEditor, baseEditor,app) {
        var serviceProviders = ko.observable();
        var selectedProvider = ko.observable();
        var printerProperties = ko.observableArray();

        function activate() {
            return datacontext.getPrinterServiceProviders(serviceProviders).then(function () {
            });
        }

        function init(printer) {
            selectedProvider(printer.providerId());
        }

        function updatePrinterProperties(obj) {

            return datacontext.getPropertiesOfPrinter(printerProperties, obj.printer.printerId()).then(function () {
                obj.printer.properties = printerProperties();
                var tempProperties = ko.observable(obj.printer);
                obj.printer.properties =printerProperties();
                return baseEditor.objEdit(new printerpropertyEditor(tempProperties())).then(function () {
                    var test = tempProperties();
                    //printerProperties(baseEditor.objEdit(new printerpropertyEditor()));
                });
            });
        }

        function updateProp(obj) {

            this.clickSave();
            this.modal.close();
            var printerPropertyId = [];
            for (var i = 0; i < obj.printer.properties.length; i++) {
                printerPropertyId.push(obj.printer.properties[i].propertyId);
            }
            return datacontext.savePrinter(printerPropertyId, obj.printer.printerId()).then(function () {

                Q.resolve(app.showMessage("Your have successfuly updated the printer", "Add printer", ['Close'])).then(function (data) {
                    router.navigateTo('#/view_printers');

                });
            });
        }
        var vm = function (printer) {
            init(printer);
            this.selectedProvider = selectedProvider();
            this.activate = activate;
            this.printer = printer;
            this.serviceProviders = serviceProviders;
            this.updatePrinterProperties = updatePrinterProperties;
            this.updateProp = updateProp;
        };

        return vm;

    });