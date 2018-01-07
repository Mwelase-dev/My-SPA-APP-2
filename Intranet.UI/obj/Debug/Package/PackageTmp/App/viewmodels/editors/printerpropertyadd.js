define(['services/datacontext'],
    function (dataContext, printerpropertyEditor, baseEditor) {
        var printer = ko.observable();
        var printerProperties = ko.observableArray();
        var selectedPrinterProperties = ko.observableArray();
        var selectedPrinterPropertiesModels = ko.observableArray();
        var printerPropertiesPrinterModels = ko.observableArray();
        var printerPropertiesPrinterModel = ko.observable();
        var propertiesOfPrinter = ko.observableArray();

        function activate() {
            return getPrinterProperties().then(function () {
                selectedPrinterProperties([]);
               
            });
        }

        function getPrinterProperties() {
            return dataContext.getAllPrinterProperties(printerProperties).then(function () {
                var test = printerProperties();
            });
        }

        function init(print) {
            printer(print);

            for (var i = 0; i < print.properties.length; i++) {
                propertiesOfPrinter.push(print.properties[i].propertyId());
            }
            var test = propertiesOfPrinter();
        }
         
        function addPrinterProperties(objModel) {
            var print = objModel.thePrinter;

            selectedPrinterProperties(propertiesOfPrinter());
            //Build PrinterPropertiesPrinterModel
            for (var k = 0; k < selectedPrinterProperties().length; k++) {
                var ppm = {
                    printerId: print().printerId(),
                    propertyId: selectedPrinterProperties()[k],
                    printer: print(),
                    property: printerPropertiesPrinterModel()
                }
              printerPropertiesPrinterModel(ppm);
                printerPropertiesPrinterModels.push(printerPropertiesPrinterModel());
                ppm = {

                };
            }
            print().properties = printerPropertiesPrinterModels();
            selectedPrinterProperties([]);
            this.modal.close();
        }

        function closeWindow() {
            propertiesOfPrinter([]);
            this.modal.close();
        }
         
        var vm = function (print) {
            init(print);
            this.activate = activate;
            this.addPrinterProperties = addPrinterProperties;
            this.printerProperties = printerProperties;
            this.selectedPrinterProperties = selectedPrinterProperties;
            this.thePrinter = printer;
            this.propertiesOfPrinter = propertiesOfPrinter;
            this.closeWindow = closeWindow;
        };

        return vm;

    });