define(['services/datacontext', 'viewmodels/editors/baseeditor', 'services/model', 'viewmodels/editors/suggestion'],
    function (datacontext, editor, servicemodel, suggestion1) {

        var suggestion = ko.observable();
        var isAdmin = ko.observable(false);
        var userRoles = ko.observableArray();
        var staff = ko.observable();
        var vote;
        var canEdit = ko.observable(false);

        var upVotes = ko.observable(0);
        var downVotes = ko.observable(0);

        var canVoteUp = ko.observable(false);
        var canVoteDown = ko.observable(false);

        //Tags
        var staffList = ko.observableArray();
        var itemToAdd = ko.observable("");
        var allItems = ko.observableArray();
        var selectedItems = ko.observableArray();

        //#region Internal Methods
        function activate(ctx) {
            return datacontext.getCurrentUser(staff).then(function () {
                return refresh(ctx.id);
            });
        }
        function voteUp() {
            //memeber vote default data
            saveVote(datacontext.suggestiontype.voteUp);
        }
        function voteDown() {
            saveVote(datacontext.suggestiontype.voteDown);
        }

        function saveVote(voteType) {
            //memeber vote default data

            if (vote == null) {
                var item = {
                    suggestionID: suggestion().suggestionId(),
                    staffID: staff().staffId(),
                    staffComments: " ",
                    voteType: voteType,
                    staffVoteDate: new Date(),
                    recordStatus: datacontext.recStatus.statusActive
                };
                datacontext.manufactureEntity(servicemodel.entityNames.staffSuggestionVotesName, item, false);
            } else {
                vote.voteType(voteType);
            }

            return datacontext.changesSave().then(function () {
                return refresh(suggestion().suggestionId());
            });
        }

        function suggestionEdit1(model) {
            return datacontext.getAllStaff(staffList).then(function () {
                model.staffList = staffList();
                editor.objEdit(model);
            });
        }
        function suggestionEdit(model) {

            return datacontext.getAllStaff(staffList).then(function () {
                model.staffList = staffList();
                editor.objEdit(new suggestion1(model));
            });
        }

        function suggestionDelete(model) {
            editor.objDelete(model).then(function () { return refresh(model.suggestionId()); });
        }
        var addStaff = function () {
            var value = itemToAdd();
            if (value) {
                if (value != "" && allItems.indexOf(value) < 0) {//check for duplicates here
                    allItems.push(value);
                }
            }
        };

        //Pre: A list of targeted staff members
        //Post: Each staff member has been notified via email.
        var notifyFollowers = function () {
            var followers = [];
            for (var i = 0; i < allItems().length; i++) {
                var data = {
                    fullName: allItems()[i].fullName,
                    staffEmail: allItems()[i].staffEmail
                }
                followers.push(data);
            }
            var chck = staff.valueHasMutated();
            //add current staff here.
            //followers.push({ staffName: staff().staffName, staffSName: staff().staffSName, staffEmail: staff().staffEmail, suggestion: { suggestionId: suggestion().suggestionId() } });
            var check = followers;
            var dataString = JSON.stringify(ko.toJS(followers), null, 2);
            datacontext.suggestionSubscription(dataString);
            allItems("");
        };

        //Pre: A wrongly selected staff member on the list
        //Post: Staff selected and removed from list
        var removeSelected = function () {
            allItems.removeAll(selectedItems());
            selectedItems([]);
        };

        function refresh(suggestId) {
            suggestion();
            var t;
            return datacontext.getCurrentUserRoles(userRoles).then(function () {
                return Q.fcall(function () {
                    return suggestion(datacontext.getSuggestion(suggestId));
                }).then(function (sug) {
                    //get a user specific vote
                    vote = getStaffVote();

                    //if the user has not voted then he/she can vote either way
                    upVotes(0);
                    downVotes(0);
                    var votes = suggestion().votes();

                    for (var i = 0; i < votes.length; i++) {

                        if (votes[i].voteType() == datacontext.voteTypeEnum.up)
                            upVotes(upVotes() + 1);

                        if (votes[i].voteType() == datacontext.voteTypeEnum.down)
                            downVotes(downVotes() + 1);
                    }


                    if (vote == null) {
                        canVoteUp(true);
                        canVoteDown(true);
                    } else {
                        canVoteDown(vote.voteType() == datacontext.voteTypeEnum.up);
                        canVoteUp(vote.voteType() == datacontext.voteTypeEnum.down);
                    }

                    isAdmin(staff().staffId() == suggestion().staffId());
                });
            });

        }

        function getStaffVote() {

            var votes = suggestion().votes();

            if (votes < 1)
                return null;

            for (var i = 0; i < votes.length; i++) {

                if (votes[i].suggestionID() == suggestion().suggestionId() && votes[i].staffID() == staff().staffId())
                    return votes[i];
            }

        }

        //#endregion

        //#region Public Members
        var vm = {
            activate: activate,
            suggestion: suggestion,
            voteUp: voteUp,
            voteDown: voteDown,
            suggestionEdit: suggestionEdit,
            suggestionDelete: suggestionDelete,
            isAdmin: isAdmin,

            canVoteUp: canVoteUp,
            canVoteDown: canVoteDown,

            upVotes: upVotes,
            downVotes: downVotes,

            staffList: staffList,
            itemToAdd: itemToAdd,
            allItems: allItems,
            selectedItems: selectedItems,
            removeSelected: removeSelected,
            addStaff: addStaff,
            notifyFollowers: notifyFollowers
        };
        return vm;
        //#endregion
    });