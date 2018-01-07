define(["services/datacontext"],
    function (dtx) {


        function updateProfile(objModel) {
            var test = objModel.updateProfile;
            return;
        }

        var vm = function (leave) {
            this.leave = leave;
            this.updateProfile = updateProfile;
        };
        return vm;
    });