define(['services/datacontext'],
    function (datacontext) {
        var staffId;
        var staff = ko.observable();

        function activate() {
        }

        function init() {
            return datacontext.getStaffLeaveSummary(staff, staffId);
        }

        var vm = function (id) {
            staffId = id;
            init();
            this.staff = staff;
            this.active = activate();
        };

        return vm;
    });