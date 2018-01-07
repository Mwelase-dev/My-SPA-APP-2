define(['services/datacontext'],
    function(datacontext) {
        galleryDesc   = ko.observable();
        galleryImages = ko.observableArray();

        //#region Internal Methods
        function activate(ctx) {
            galleryDesc(ctx.name);
            return datacontext.getGalleryImages(galleryImages, ctx.name);
        }
        //#endregion

        //#region Public Members
        var vm = {
            activate      : activate,
            galleryDesc   : galleryDesc,
            galleryImages : galleryImages
        };
        return vm;
        //#endregion
    });