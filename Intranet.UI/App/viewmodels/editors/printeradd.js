define(['services/datacontext', 'viewmodels/editors/printerpropertyadd', 'viewmodels/editors/baseeditor', 'durandal/app'],
    function (dataContext, printerpropertyEditor, baseEditor, app) {

        var printer = ko.observable();
        var serviceProviders = ko.observable();
        var selectedProvider = ko.observable();
        var selectedProviderModel = ko.observable();
        var printerProperties = ko.observableArray();


        function activate() {
            return dataContext.getPrinterServiceProviders(serviceProviders).then(function () {
                var ctx = dataContext.manufactureEntity("PrinterModel");
                ctx.recordStatus = ko.observable(dataContext.recStatus.statusActive);
                printer(ctx);
            });

        }

        function addPrinter() {
            for (var i = 0; i < serviceProviders().length; i++) {
                if (serviceProviders()[i].providerId() === selectedProvider()) {
                    selectedProviderModel(serviceProviders()[i]);
                    printer().printerProvider(selectedProviderModel());
                    break;
                }
            }
            var test = printer();
            this.clickSave();
            var test = printer();
            savePrinterProperties(printer);
        }

        function savePrinterProperties(obj) {

            var printerPropertyId = [];
            for (var i = 0; i < obj().properties.length; i++) {
                printerPropertyId.push(obj().properties[i].propertyId);
            }
            //this.modal.close();
            if (printerPropertyId.length > 0) {
                return dataContext.savePrinter(printerPropertyId, obj().printerId()).then(function () {

                    Q.resolve(app.showMessage("Your have successfuly saved the printer", "Add printer", ['Close'])).then(function (data) {
                        router.navigateTo('#/view_printers');

                    });
                });
            } else {
                Q.resolve(app.showMessage("Please NOTE. The printer is saved, but it has no properties", "Add printer", ['Close'])).then(function (data) {
                });
            }


        }

        function addPrinterProperties(obj) {
            for (var i = 0; i < serviceProviders().length; i++) {
                if (serviceProviders()[i].providerId() === selectedProvider()) {
                    selectedProviderModel(serviceProviders()[i]);
                    printer().printerProvider(selectedProviderModel());
                    break;
                }
            }
            printer().printerProvider(selectedProviderModel());
            printer().providerId(selectedProvider());
            if (printer().printerMake() && printer().model() && printer().serialNumber() && printer().printerDescription() && printer().location() && printer().printerProvider()) {

                dataContext.changesSave().then(function () {
                    return baseEditor.objEdit(new printerpropertyEditor(printer())).then(function () {
                        var test = printer();
                        //savePrinterProperties(printer);
                    });
                });
            } else {
                Q.resolve(app.showMessage("Please fill in all printer fields.", "Missing fields", ['Close'])).then(function (data) {
                    return;
                });

            }

        }

        var vm = function () {
            this.activate = activate;
            this.printer = printer;
            this.addPrinter = addPrinter;
            this.serviceProviders = serviceProviders;
            this.selectedProvider = selectedProvider;
            this.addPrinterProperties = addPrinterProperties;
        };

        return vm;

    });