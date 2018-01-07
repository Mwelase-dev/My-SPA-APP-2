define(["services/datacontext"],
    function (dtx) {


        function updateProfile(objModel) {
            var test = objModel.updateProfile;
            return;
        }

        var vm = function (userProfile) {
            this.userProfile = userProfile;
            this.updateProfile = updateProfile;
        };
        return vm;
    });