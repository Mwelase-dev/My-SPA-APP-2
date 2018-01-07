define(['durandal/system', 'services/logger', 'durandal/plugins/router', 'config', 'services/datacontext'],
    function (system, logger, router, config, datacontext) {
        var isAdmin = ko.observable(false);
        
        function activate() {
            datacontext.getIsAdmin(isAdmin);
            logger.log('Base-shell module loaded', null, system.getModuleId(shellBase), true);
            
            // Get the menu from the DB
            var routes = [];
            datacontext.getMenuList(routes, routes.length <= 0).then(function () {
                router.map(routes);
                return router.activate(routes[0].url.toString());
            });
        }

        //#region Public Members
        var shellBase = {
            activate : activate,
            router: router,
            isAdmin : isAdmin
        };
        return shellBase;
        //#endregion
    });