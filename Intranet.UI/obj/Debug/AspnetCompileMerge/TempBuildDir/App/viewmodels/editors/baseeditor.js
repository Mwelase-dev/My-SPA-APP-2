define(['services/datacontext', 'durandal/app', 'services/model'],
    function (datacontext, app, objModel) {
        var isWorking = ko.observable();
        var title = '';
        var isTempSave = ko.observable(false);
        var errorList = ko.observableArray();

        //#region Adding/Editing item
        function objAdd(entityName, parentList, tempSave, initialValues) {

            isTempSave(tempSave == true);

            var model = datacontext.manufactureEntity(entityName, initialValues);
            if (parentList != undefined) {
                parentList.push(model);
            }
            return objEdit(model);
        }


        function objEdit(model) {
            // Helper
            model.clickSave = function (onSuccess) {
                //   isWorking(true);

                if (isTempSave() == true) {
                    isWorking(false);
                    isTempSave(false);
                    //onSuccess();
                    return model.modal.close();
                } else {
                    //Post: for Entity validation using Breeze
                    if (model.entityAspect != undefined) {
                        if (model.entityAspect.hasValidationErrors == false) {
                            return datacontext.changesSave().then(function () {
                                isWorking(false);
                                Q.resolve(onSuccess).then(function () {
                                    return model.modal.close();

                                    //HERE MUST REFRESH
                                });
                            });
                        } else {
                            errorList(model.entityAspect.getValidationErrors());
                            if (errorList) {
                                for (var i = 0; i < errorList().length; i++) {
                                    toastr.error(errorList()[i].errorMessage);
                                }
                            }
                        }
                    }
                    return datacontext.changesSave().then(function () {
                        isWorking(false);
                        Q.resolve(onSuccess).then(function () {
                            return model.modal.close();

                            //HERE MUST REFRESH
                        });
                    });
                }
            };
            // Helper
            model.clickCancel = function () {
                isWorking(true);
                datacontext.changesCancel();
                isWorking(false);
                return model.modal.close();
            };
            return app.showModal(model);
        }

        function objDelete(model) {
            return datacontext.promptAndProceed(datacontext.recStatus.statusDelete, model, title);
        }
        //#endregion

        //#region Public Members
        var vm = {
            objAdd: objAdd,
            objEdit: objEdit,
            objDelete: objDelete,
            objModel: objModel
        };
        return vm;
        //#endregion
    });