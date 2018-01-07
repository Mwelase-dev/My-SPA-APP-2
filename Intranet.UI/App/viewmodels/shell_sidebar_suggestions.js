define(['services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/suggestion'],
    function (datacontext, editor, suggestionEditor) {
        var suggestions = ko.observableArray();
        var unapprovedSuggestion = ko.observable(false);
        var suggestionsToLeaveOut = ko.observableArray();
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
            return datacontext.getSuggestions(suggestions).then(function () {
                for (var i = 0; i < suggestions().length; i++) {
                    if (suggestions()[i].suggestionStatus() === "Pending") {
                        unapprovedSuggestion(true);
                        break;
                    }
                }
                for (var j = 0; j < suggestions().length; j++) {
                    if (suggestions()[j].suggestionStatus() === "Rejected") {
                        suggestions().splice(j, 1);
                        j = 0;
                    }
                    else if (suggestions()[j].suggestionStatus() === "Pending") {
                        suggestions().splice(j, 1);
                        j = 0;
                    }
                }
            });
        }

        //var img = document.getElementById('blinking_image');

        //var interval = window.setInterval(function () {
        //    if (img.display == 'hidden') {
        //        img.style.visibility = 'visible';
        //    } else {
        //        img.style.visibility = 'hidden';
        //    }
        //}, 1000);

        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,
            suggestions: suggestions,
            suggestionAdd: add,
            unapprovedSuggestion: unapprovedSuggestion,

        };
        return vm;
        //#endregion
    });