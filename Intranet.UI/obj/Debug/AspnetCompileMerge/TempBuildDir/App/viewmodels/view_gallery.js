define(['services/datacontext'],
    function(datacontext) {
        var galleries = ko.observableArray();

        //#region Internal Methods
        function activate() {
            return datacontext.getGalleries(galleries);
        }
        //#endregion

        //#region Public Members
        var vm = {
            activate  : activate,
            galleries : galleries
        };
        return vm;
        //#endregion
    });