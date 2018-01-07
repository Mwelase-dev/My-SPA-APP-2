define(['services/datacontext', 'config', 'viewmodels/editors/leavecounter', 'viewmodels/editors/baseeditor', 'services/model', 'durandal/app', 'viewmodels/editors/workhours', 'viewmodels/editors/leaveSummaryTest'],
    function (datacontext, config, leaveEditor, baseEditor, model, app, workHours, leaveSummaryTest) {
        var isAdmin = ko.observable();
        var branches = ko.observableArray();
        var printers = ko.observableArray();
        var staffMembers = ko.observableArray();
        var staffMem = ko.observable();
        var selectedStaff = ko.observable();
        var tempLeaves = ko.observableArray();
        var leaveApplications = ko.observableArray();


        var staffJoinDate = ko.observable();
        var cyclestart = ko.observable();
        var cycleend = ko.observable();
        var sickcyclestart = ko.observable();
        var sickcycleend = ko.observable();

        function viewAttached() {
            //var dateToday = new Date(staffJoinDate());
            //var cyclestartdate = new Date(cyclestart());
            //var cycleenddate = new Date(cycleend());
            //var sickcyclestartdate = new Date(sickcyclestart());
            //var sickcycleenddate = new Date(sickcycleend());

            //$("#staffJoinDate").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", dateToday);
            //$("#CycleStart").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", cyclestartdate);


            //$("#CycleEnd").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", cycleenddate);


            //$("#StaffJoinDate").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", dateToday);


            //$("#SickCycleStart").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", sickcyclestartdate);


            //$("#SickCycleEnd").datepicker({
            //    dateFormat: "yy-mm-dd"
            //}).datepicker("setDate", sickcycleenddate);


        }

        function activate() {
            return datacontext.getAllStaff(staffMembers).then(function () {
                return datacontext.getCurrentUser1(staffMem).then(function () {
                    return datacontext.getStaffbyId(staffMem, staffMem().staffId()).then(function () {
                        var test = staffMem();
                        selectedStaff(staffMem().staffId());
                        return getLeaveByStaffId().then(function() {
                            return datacontext.getDivisionLookUp(branches);
                        });
                    });
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

        function details() {
            if (selectedStaff()) {
                return baseEditor.objEdit(new leaveSummaryTest(selectedStaff())).then(function () {
                    //history.go(0);
                });
            }
        }

        function getLeaveByStaffId() {
            return datacontext.getStaffLeaveApplications(leaveApplications, selectedStaff()).then(function () {
                for (var i = 0; i < leaveApplications().length; i++) {
                    if (leaveApplications()[i].displayLeaveStart() < leaveApplications()[i].staffMember().cycleStart()) {
                        leaveApplications().splice(i, 1);
                    }
                }

                var data = leaveApplications()[0];
                if (typeof data === 'undefined' || data == null) {
                    //filterStaff(null);
                    return;
                }
            });
        }

        //========================================>

        var vm = function (staff, defBranch) {
            this.activate = activate;
            this.viewAttached = viewAttached;
            init(defBranch, staff);
            this.branches = branches;
            this.staff = staffMem;
            this.printers = printers;
            this.staffMembers = staffMembers;
            this.branches = branches;
            this.gotoLeaveDetails = details;
            this.leaveApplications = leaveApplications;
        };
        return vm;
    });