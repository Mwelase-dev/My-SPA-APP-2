define(["services/datacontext", "services/model"],
    function (dtx, model) {
        var staffList           = ko.observableArray();
        var suggestion          = ko.observable();
        var staff               = ko.observable();
        var suggestionFollowers = ko.observableArray();
        var suggestionText      = ko.observable();
        var suggestionSubject = ko.observable();
        var fgfg = ko.observable();
        //var itemToAdd = ko.observableArray()
        var sugTest;

        var test = ko.observableArray();
       
        var getSuggestionFollowers = function () {
            return dtx.getStaffSuggestionFollowers(test);
        };

        function activate() {
            
            suggestion(dtx.manufactureEntity(model.entityNames.suggestionName));
            suggestion().suggestionStatus("Pending");

            dtx.getCurrentUser(staff).then(function () {
                suggestion().staffId(staff().staffId());
            });

            getSuggestionFollowers();
            staffList.suggestionFollowers = suggestionFollowers();

            return dtx.getAllStaff(staffList);
        };

        function init(model) {
            if (model) {
                suggestionSubject(model.suggestionSubject());
                suggestionText(model.suggestionText());
            }
            fgfg(model);
        }

        var save = function () {
            var suggList = [];
            //suggestionSubscription
            suggestion().suggestionStatus = "Pending";
            var test1                        = suggestion();
            var test3 = suggestionFollowers();
            var suggId;
            if (fgfg()) {
                suggId = fgfg().suggestionId();
                for (var j = 0; j < fgfg().suggestionFollowers().length; j++) {
                    suggList.push({ suggestionId: fgfg().suggestionFollowers()[j].suggestionID(), staffID:fgfg().suggestionFollowers()[j].staffID() });
                    
                }
            } else {
                suggId = suggestion().suggestionId();
            }
           
            suggestion().suggestionFollowers = suggestionFollowers();
            suggestion().suggestionSubject   = suggestionSubject();
            suggestion().suggestionText = suggestionText();

            for (var i = 0; i < suggestion().suggestionFollowers.length; i++) {
                var d = {
                    suggestionId: suggId,
                    staffID     : suggestion().suggestionFollowers[i]
                }
                suggList.push(d);
            }
            var test = suggList;
            var data = {
                staffId             : staff().staffId(),
                suggestionId        : suggId,
                suggestionSubject   : suggestion().suggestionSubject, 
                suggestionDate      : suggestion().suggestionDate, 
                suggestionText      : suggestion().suggestionText,
                Votes               : suggestion().votes,
                suggestionFollowers : suggList,
                staff: suggestion().staff,
                suggestionStatus :"Pending"
            }

            dtx.suggestionSubscription(data);
            this.modal.close();
        };

        var vm = function (model) {
            this.activate            =  activate;
            init(model);                
            this.staffList           =  staffList;
            this.suggestion          =  model;
            this.save          =  save;
            this.suggestionFollowers =  suggestionFollowers;
            this.suggestionSubject   =  suggestionSubject;
            this.suggestionText      =  suggestionText;

        };
        return vm;
    });