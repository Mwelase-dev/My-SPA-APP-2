require.config({
    paths: { "text": "durandal/amd/text" }
});

define(function (require) {
    var system      = require('durandal/system');
    var app         = require('durandal/app');
    var router      = require('durandal/plugins/router');
    var viewLocator = require('durandal/viewLocator');
    var logger      = require('services/logger');
    var config      = require('config');
    
    // Enable-Disable Logging
    system.debug(true);
    
    // Fire up the up (with Promise)
    app.title = config.siteName;
    //app.adaptToDevice();
    app.start().then(function () {
        viewLocator.useConvention();
        
        router.useConvention();
        app.setRoot('viewmodels/shell_base');
        
        router.handleInvalidRoute = function(route, params) {
            logger.logError('No route found', route, true);
        };
    });
});