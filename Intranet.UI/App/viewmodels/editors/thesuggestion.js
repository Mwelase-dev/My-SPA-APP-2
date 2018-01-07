define(['services/datacontext', 'services/helpers', 'durandal/app', 'services/helpers'],
    function (datacontext, helpers, app, helper) {
    
        var suggestion = ko.observable();
       

        function activate() {

        }

        
        function init(thesuggestion) {

        }

        function acceptSuggestion(objModel) {
            objModel.suggestion().suggestionStatus("Accepted");
            this.clickSave();
        }

        function save(objModel) {
            objModel.entityAspect.setDeleted();
            return datacontext.changesSave().then(function () {
                this.modal.close();
            });
           
        }
         
        function rejectSuggestion(objModel) {
            objModel.suggestion().suggestionStatus("Rejected");
            this.clickSave();
        }
         
        function close() {
            this.modal.close();
        }


        var vm = function (thesuggestion) {
            init(thesuggestion);
            suggestion(thesuggestion);
            this.activate = activate;
            this.acceptSuggestion = acceptSuggestion;
            this.rejectSuggestion = rejectSuggestion;
            this.close = close;
            this.suggestion = suggestion;
        };
        return vm;
    });