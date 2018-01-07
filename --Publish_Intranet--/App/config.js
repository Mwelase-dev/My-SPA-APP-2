define(function () {
    toastr.options.timeOut = 4000;
    toastr.options.positionClass = 'toast-bottom-right';
    
    var siteName          = 'NVFH Intranet';
    var startModule       = 'view_announcements';
    var remoteServiceName = 'api/breezedata';
    var editorViews       = 'views/editors/';
    var localDateFormat   = 'YYYY/MM/DD';

    return {
        siteName          : siteName,
        startModule       : startModule,
        remoteServiceName : remoteServiceName,
        editorViews       : editorViews,
        localDateformat   : localDateFormat,
    };
});