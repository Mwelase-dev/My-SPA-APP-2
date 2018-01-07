define(["services/datacontext"],
    function (dtx) {
        var staffId;
        var staff = ko.observableArray();
        var staffSummaryData = ko.observableArray();
        var staffSummaryData1 = ko.observableArray();
        var list = ko.observableArray();
        var staffLeaveSummaryData = ko.observableArray();
        var leaveApplications = ko.observableArray();

        function init() {
            return dtx.getStaffLeaveSummary1(staff, staffId).then(function () {
                staffLeaveSummaryData([]);
                getOverlap();
                staffLeaveSummaryData(getItemsByYear());
                //selectedStaff(staffId);
                return getLeaveByStaffId().then(function() {
                });
            });
        }

        function getLeaveByStaffId() {
            return dtx.getStaffLeaveApplications(leaveApplications, staffId).then(function () {
                for (var i = 0; i < leaveApplications().length; i++) {
                    if (leaveApplications()[i].displayLeaveStart() < leaveApplications()[i].staffMember().cycleStart()) {
                        leaveApplications().splice(i, 1);
                    } else if (leaveApplications()[i].leaveStatus() === 4) {
                            leaveApplications().splice(i, 1);
                        }
                }

                var data = leaveApplications()[0];
                if (typeof data === 'undefined' || data == null) {
                    filterStaff(null);
                    return;
                }
            });
        }


        var getOverlap = function () {
            for (var i = 0; i < staff().length; i++) {
                for (var j = 0; j < staff()[i].data.length; j++) {
                    var data = {
                        year: staff()[i].data[j].year,
                        leaveType: staff()[i].data[j].leaveType,
                        leaveAllocation: staff()[i].data[j].leaveAllocation,
                        leaveTaken: staff()[i].data[j].leaveTaken,
                        leaveCarriedOver: staff()[i].data[j].leaveCarriedOver,
                        leaveRunningTotal: calcRunningTotal(staff()[i].data[j].leaveType, staff()[i].data[j].leaveCarriedOver)
                    };
                    if (data.year === staff()[i].year) {
                        staffSummaryData.push(data);
                    }
                }
                list.push({ year: staff()[i].year, data: staffSummaryData() });
            }
            getItemsByYear();
        };

        var getItemsByYear = function () {
            var myList = ko.observableArray();
            var leaveSummaries = staffSummaryData();
            for (var i = 0; i < leaveSummaries.length; i++) {
                myList.push(new onYearSummary(leaveSummaries[i].year, leaveSummaries[i]));
            }
            return transformArr(myList());
        }

        var onYearSummary = function (year, dataList) {
            var self = this;
            self.year = year;
            self.data = dataList;
        }

        var transformArr = function (org) {
            var newArr = [];
            var years = {};
            var newItem, i, j, cur;
            for (i = 0, j = org.length; i < j; i++) {
                cur = org[i];
                if (!(cur.year in years)) {
                    years[cur.year] = { year: cur.year, myData: [] }
                    newArr.push(years[cur.year]);
                }
                years[cur.year].myData.push(cur.data);
            }
            return newArr;
        };
          
        var calcRunningTotal = function (leaveType, leaveCarriedOver) {
            var newOne = {
                leaveType: leaveType,
                leaveCarriedOver: leaveCarriedOver
            }
            for (var i = 0; i < staffSummaryData1().length; i++) {
                if (staffSummaryData1()[i].leaveType === newOne.leaveType) {
                    staffSummaryData1()[i].leaveCarriedOver += newOne.leaveCarriedOver;
                    return staffSummaryData1()[i].leaveCarriedOver;
                }
            }
            staffSummaryData1.push(newOne);
            return newOne.leaveCarriedOver;
        }
         
        function activate() {
            staff([]);
            staffSummaryData([]);
            staffSummaryData1([]);
            staffSummaryData1([]);
            list([]);
            staffLeaveSummaryData([]);
            return;
        }

        var vm = function (id) {
            staffId = id;
            init();

            this.active = activate();
            this.staffLeaveSummaryData = staffLeaveSummaryData;
            this.leaveApplications = leaveApplications;
        };
        return vm;
    });