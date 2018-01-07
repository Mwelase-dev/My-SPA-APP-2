define(['services/logger', 'services/datacontext', 'durandal/app','services/helpers'],
    function (logger, datacontext, app,helpers) {

        var orders = ko.observableArray();
        var order = ko.observable();
        var authorised = ko.observable(false);
        var userRoles = ko.observableArray();

        //#region Internal methods

        function activate() {
            refresh();
        }

        function closeOrder(objectModel) {
            return datacontext.getTonerOrderDetail(order, objectModel.orderDetailsId).then(function() {
                order().orderStatus(datacontext.orderStatus.closed);
                return datacontext.changesSave().then(function() {
                    return refresh();
                });
            });
        }

        function refresh() {
            var roles = datacontext.userRoleEnum.intranetAdmins;
            return datacontext.getCurrentUserRoles(userRoles).then(function() {
                if (datacontext.isInRole(userRoles(), roles)) {
                    return datacontext.getCurrentOrders(orders).then(function() {
                        authorised(true);
                    });
                } else
                    authorised(false);
            });
        }
        
        function cancel() {
            return helpers.returnToPrevPage();
        }

        //#endregion

        var vm =
        {
            activate: activate,

            orders: orders,

            closeOrder: closeOrder,
            cancel:cancel,

            authorised: authorised,
        };

        return vm;

    });