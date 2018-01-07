define(['services/logger', 'services/datacontext', 'durandal/app'],
    function (logger, datacontext, app) {

        var printers = ko.observableArray();
        var selecetdPrinter = ko.observable();
        var staff = ko.observable();
        var printerProperties = ko.observableArray();
        var pss = ko.observableArray();

        //#region Internal methods

        function activate() {

            /* var request = [];
 
             var requestModel = {
                 NameSurname: "#txtNameSurname",
             EmailAddress: "#txtEmailAddress",
                 WebsiteAddress: "#txtWebAddress",
                 Motivation: "#txtMotivation",
             };
 
             request.push(requestModel);
             
             requestModel = {
                 NameSurname: $("#txtNameSurname").val(),
                 EmailAddress: $("#txtEmailAddress").val(),
                 WebsiteAddress: $("#txtWebAddress").val(),
                 Motivation: $("#txtMotivation").val()
             };
 
             request.push(requestModel);
 
             return datacontext.orderToner(request);*/

            return datacontext.getCurrentUser(staff).then(function () {
                selecetdPrinter(staff().staffDefaultPrinter());
                return datacontext.getPrinters(printers).then(function () {
                    return getPrinterProperties();
                });

            });
        }

        function cmbPrintersChange() {
            getPrinterProperties();
        }

        function getPrinterProperties() {

            if (selecetdPrinter() != null) {
                printerProperties([]);
                return datacontext.getPrinterProperties(printerProperties, selecetdPrinter());
            }
        }

        function orderToner() {

            var orderInit = {
                staffId: staff().staffId(),
                printerId: selecetdPrinter(),
                orderDate: new Date(),
                orderStatus: datacontext.orderStatus.open,
                recordStatus: datacontext.recStatus.statusActive,
            };


            var canSave = false;
            var order = datacontext.manufactureEntity("TonerOrdersModel", orderInit);

            var orderList = [];

            for (var i = 0; i < printerProperties().length; i++) {

                var prop = printerProperties()[i];

                if (prop.order == true && prop.canBeOrdered) {
                    var todInit = {

                        orderId: order.orderId(),
                        propertyId: prop.colourId,
                        printerId: selecetdPrinter(),
                        orderStatus: datacontext.orderStatus.open,
                        recordStatus: datacontext.recStatus.statusActive
                    };

                    orderList.push(todInit);

                    datacontext.manufactureEntity("TonerOrderDetailsModel", todInit);

                    canSave = true;
                }
            }

            if (canSave) {
                datacontext.changesSave().then(function () {

                    datacontext.orderToner(orderList);

                    Q.resolve(app.showMessage("Toner has been ordered succeefuly", "Success", ['Ok'])).then(function () {
                        getPrinterProperties();
                    });

                });
            }
        }

        function cancel() {
            history.go(-1);
        }

        //#endregion

        var vm =
        {
            activate: activate,

            printers: printers,
            printerProperties: printerProperties,
            selectedPrinter: selecetdPrinter,

            cmbPrintersChange: cmbPrintersChange,

            orderToner: orderToner,
            cancel:cancel,
        };

        return vm;

    });