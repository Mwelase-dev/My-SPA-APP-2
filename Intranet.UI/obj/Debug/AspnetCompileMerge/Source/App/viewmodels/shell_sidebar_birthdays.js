define(['services/datacontext'],
    function (datacontext) {
        var birthdays = ko.observableArray();
        
        //#region Internal Methods
        function activate() {
            return datacontext.getBirthdays(birthdays);
        }
        //#endregion
        
        //#region Public Members
        var vm = {
            activate  : activate,
            birthdays : birthdays
        };
        return vm;
        //#endregion
    });