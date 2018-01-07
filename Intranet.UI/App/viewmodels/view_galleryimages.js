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
        $(document).ready(function () {
            $('#gallery').galleryView({
                panel_width: 460,
                panel_height: 300,
                frame_width: 100,
                frame_height: 70,
                top:0
            });
        });
        //#region Public Members
        var vm = {
            activate      : activate,
            galleryDesc   : galleryDesc,
            galleryImages : galleryImages
        };
        return vm;
        //#endregion
    });