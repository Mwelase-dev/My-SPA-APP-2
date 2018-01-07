define(['services/datacontext', 'config', 'viewmodels/editors/leavecounter', 'viewmodels/editors/baseeditor', 'services/model', 'durandal/app', 'viewmodels/editors/workhours'],
    function (datacontext, config, leaveEditor, baseEditor, model, app, workHours) {
        var isAdmin = ko.observable();
        var branches = ko.observableArray();
        var printers = ko.observableArray();
        var staffMembers = ko.observableArray();
        var staffMem = ko.observable();

        function activate() {
            return datacontext.getAllStaff(staffMembers).then(function () {
                return datacontext.getCurrentUser1(staffMem).then(function () {
                    return datacontext.getDivisionLookUp(branches);
                });
            });
        }

        function init(defBranch, staff) {
            staffMem(staff);
            return datacontext.getIsAdmin(isAdmin).then(function () {
                return datacontext.getPrinters(printers).then(function () {
                    if (typeof defBranch === "undefined")
                        return datacontext.getBranchlookup(branches);
                    var data = [];
                    data.push(defBranch);
                    return branches(data);
                });
            });
        }


        //========================================>

        var vm = function (staff, defBranch) {
            this.activate = activate;
            init(defBranch, staff);
            this.branches = branches;
            this.staff = staffMem;
            this.printers = printers;
            this.staffMembers = staffMembers;
            this.branches = branches;
        };
        return vm;
    });