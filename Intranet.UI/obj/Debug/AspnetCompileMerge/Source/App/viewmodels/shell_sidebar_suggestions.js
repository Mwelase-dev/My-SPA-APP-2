define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/suggestion'],
    function (datacontext, editor, suggestionEditor) {
        var suggestions = ko.observableArray();

        //#region Internal Methods
        function activate() {
            return refresh();
        }

        function add() {
            var staff = ko.observable();
            return datacontext.getCurrentUser(staff).then(function () {
                return editor.objEdit(new suggestionEditor());
            });
        }

        function refresh() {
            return datacontext.getSuggestions(suggestions);
        }

        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,
            suggestions: suggestions,
            suggestionAdd: add
        };
        return vm;
        //#endregion
    });