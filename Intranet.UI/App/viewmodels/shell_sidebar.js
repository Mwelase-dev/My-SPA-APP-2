define(function () {
    var isAdmin = ko.observable(false);

    //#region Internal Methods
    function activate() {

    }
    //#endregion
    
    //#region Public Members
    var vm = {
        activate: activate,
        isAdmin: isAdmin
    };
    return vm;
    //#endregion
});