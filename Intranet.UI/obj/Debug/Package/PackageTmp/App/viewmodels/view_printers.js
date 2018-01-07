define(['services/logger', 'services/datacontext', 'durandal/app', 'viewmodels/editors/baseeditor', 'viewmodels/editors/updateprinter', 'viewmodels/editors/printeradd', 'viewmodels/editors/updateprovider', 'viewmodels/editors/printerpropertyadd', 'services/helpers', 'viewmodels/editors/printerUsersEditor'],
    function (logger, dataContext, app, baseEditor, printerEditor, printerAdd, providerEditor, printerpropertyEditor, helpers, usersEditor) {

        var staff = ko.observableArray();
        var serviceProviders = ko.observable();
        var printers = ko.observableArray();
        var selectedServiceProviders = ko.observable();

        function activate() {
            return dataContext.getPrinterServiceProviders(serviceProviders).then(function () {
                var test = serviceProviders();
                return refreshPrinters();
            });
        }

        function refreshPrinters() {

            return dataContext.getCurrentUser(staff).then(function () {
                printers([]);

                return dataContext.getAllPrinters(printers).then(function () {
                    var test = printers();
                });
            });
        }

        function deletePrinter(objModel) {

            Q.resolve(app.showMessage("Are you sure you want to <b>Delete</b> the printer?", "Delete Printer", ['No', 'Yes'])).then(function (data) {

                if (data == "No")
                    return;

                objModel.recordStatus(dataContext.recStatus.statusDelete);
                objModel.entityAspect.setModified();

                return dataContext.changesSave().then(function () {
                    return refreshPrinters();
                });


            });
        }

        function updatePrinter(objMOdel) {

            return baseEditor.objEdit(new printerEditor(objMOdel)).then(function () {
                refreshPrinters();
            });
        }

        function updateProvider(objMOdel) {
            for (var i = 0; i < serviceProviders().length; i++) {
                if (serviceProviders()[i].providerId() === objMOdel.providerId()) {
                    selectedServiceProviders(serviceProviders()[i]);
                    break;
                }
            }
            return baseEditor.objEdit(new providerEditor(selectedServiceProviders())).then(function () {
                refreshPrinters();
            });
        }

        function addPrinter() {

            return baseEditor.objEdit(new printerAdd()).then(function () {
                refreshPrinters();
            });
        }
         
        function cancel() {
            return helpers.returnToPrevPage();
        }

        function setUsers(objMOdel) {
            return baseEditor.objEdit(new usersEditor(objMOdel)).then(function () {
                refreshPrinters();
            });
        }

        var vm = {
            activate: activate,

            printers: printers,
            staff: staff,

            deletePrinter: deletePrinter,
            updatePrinter: updatePrinter,
            addPrinter: addPrinter,

            updateProvider: updateProvider,

            cancel: cancel,
            serviceProviders: serviceProviders,
            setUsers : setUsers


        };

        return vm;

    });