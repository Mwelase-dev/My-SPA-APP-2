define(['services/logger', 'services/datacontext', 'services/helpers'],
    function (logger, dataContext, helpers) {
        var isloading = ko.observable(false);
        var enableFiltering = ko.observable();
        var staff = ko.observableArray();
        var staffMem = ko.observableArray();
        var authorised = ko.observable(true);
        var selectedStaff = ko.observable();
        var graphScales = ['Week', 'Month', 'Year']; // NEVER change these!!
        var selectedScale = ko.observable(0);
        var graphCounter = ko.observableArray();
        var labelDates = ko.observableArray();

        function activate() {
            if (dataContext.isInRole(authorised(), dataContext.userRoleEnum.manager))
                authorised(true); // And this??
            return dataContext.getCurrentUser(staffMem).then(function () {
                if (authorised())
                    return dataContext.getAllStaff(staff);
                return Q.fcall(function () {
                    selectedStaff(staffMem().staffId());
                    enableFiltering(selectedStaff() != null);
                    staff(staffMem());
                });
            });
        }
        function viewAttached() {
            var dateToday = new Date();
            $("#clockStartDate").datepicker({
                dateFormat: "dd-mm-yy",
                onClose: function () {
                    var tempDate = $("#clockStartDate").datepicker("getDate");
                    $("#clockEndDate").datepicker("option", "minDate", tempDate);
                }
            }).datepicker("setDate", dateToday);
            $("#clockEndDate").datepicker({ dateFormat: "dd-mm-yy", minDate: dateToday }).datepicker("setDate", dateToday);
        }
        function cancel() {
            return helpers.returnToPrevPage();
        }
        function cmbStaffMemberChange() {
            enableFiltering(selectedStaff() != null);
        }
        function getGraphData() {
            isloading(true);
            var dataArray = ko.observableArray();

            var startD = moment($("#clockStartDate").datepicker('getDate')).format('YYYY-MM-DD');
            var endD = moment($("#clockEndDate").datepicker('getDate')).format('YYYY-MM-DD');
            return dataContext.getGraphData(dataArray, selectedStaff(), startD, endD).then(function () {
                processGraphData(new Date(startD), new Date(endD), dataArray, selectedScale().toLowerCase());
                isloading(false);
            });
        }
        function processGraphData(startD, endD, dataArray, scale) {
            var stackCount = 27;
            var graphs = [];

            graphCounter.removeAll();
            startD = moment(startD).startOf(scale);
            endD = moment(endD).endOf(scale);

            //#region Calculate
            var stacks = getGraphStacks(stackCount, scale);

            while (startD <= endD) {
                //#region Days Data
                var seriesNumber = seriesCounter(startD, scale);
                var daysData = findDataForThisDay(startD, dataArray);
                //labelDates.push(startD.toDateString);
                if (daysData) {

                    for (var j = 0; j < daysData.timeKeepingItems.length; j++) { //Will be issue if more than 20!!!!
                        if (j > 0) {

                            if (daysData.leaveType === 1) {
                                stacks[19].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[19].color = '#FFFF00';//Yellow   
                            }
                            if (daysData.leaveType === 2) {
                                stacks[20].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[20].color = '#A52A2A';//Brown
                            }
                            if (daysData.leaveType === 3) {
                                stacks[21].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[21].color = '#FF00FF';//Fuschsia
                            }
                            if (daysData.leaveType === 4) {
                                stacks[22].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[22].color = '#B0C4DE';//Lightsteel Blue
                            }
                            if (daysData.leaveType === 5) {
                                stacks[23].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[23].color = '#8A2BE2';//Blue Violet
                            }
                            if (daysData.leaveType === 6) {
                                stacks[24].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[24].color = '#7FFF00';//Chartreuse
                            }
                            if (daysData.leaveType === 7) {

                                //stacks[25].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                //stacks[25].color = '#FFE4B5';//Moccasin
                                stacks[j].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                //if (daysData.timeKeepingItems[j].dataStatus == 7)
                                //    stacks[j].color = '#FFE4B5';//Moccasin
                            }

                            if (daysData.isPublicHoliday) {
                                stacks[25].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                                stacks[25].color = '#000000';//Black
                                continue;
                            }
                            if (daysData.leaveType === 0) {
                                stacks[j].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];

                            }

                            //if (daysData.leaveType === 7) {
                            //    stacks[25].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                            //    stacks[25].color = '#FFE4B5';//Moccasin
                            //}
                           

                            //if (daysData.IsPublicHoliday) {
                            //    stacks[26].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                            //    stacks[26].color = '#000000';//Black
                            //}

                        }
                        else {
                            stacks[j].data[seriesNumber] = [seriesNumber, calculateTimeDifferences(j, daysData.timeKeepingItems)];
                        }

                    }
                } else {
                    stacks[0].data[seriesNumber] = [seriesNumber, 0];
                }
                //#endregion
                if (graphBreak(startD, scale)) {

                    graphs[graphCounter().length] = stacks; // Assign the weeks data
                    stacks = getGraphStacks(stackCount,scale); // Reset for the new week
                    graphCounter.push("Period ending: " + moment(startD).format('ddd DD MMM YYYY'));
                }
                startD.add("day", 1);
            }
            //#endregion
            assembleGraphs(graphs);
        }

        // Determines if we need to start a new graph
        function graphBreak(date, scale) {
            switch (scale) {
                case 'week': return date.day() == 6;
                case 'month': return date.isSame(moment(date).endOf('month').startOf('day'));
                case 'year': return date.isSame(moment(date).endOf('year').startOf('day'));
            }
            return false;
        }

        // Determines the array element we need to work with
        function seriesCounter(date, scale) {
            switch (scale) {
                case 'week': return date.day();       // Moment objects
                case 'month': return date.date();     // Moment objects
                case 'year': return date.dayOfYear(); // Moment objects
            }
            return false;
        }

        // Looks for data on this date
        function findDataForThisDay(dateToLookFor, dataList) {
            var otherDate = moment(dateToLookFor).format('YYYY-MM-DD');
            for (var i = 0; i < dataList().length; i++) {
                var record = dataList()[i];
                if (record) {
                    var testDate = moment(record.clockDate).format('YYYY-MM-DD');
                    if (testDate == otherDate) {
                        return record;
                    }
                } else return null;
            }
            return null;
        }

        // Time passed between current and previous record.
        function calculateTimeDifferences(index, clockData) {
            var current = moment(clockData[index].date);
            var previous = moment(clockData[index].date).startOf('day');
            if (index > 0) {
                previous = moment(clockData[index - 1].date);
            }
            var minutes = moment.duration(current - previous).asMinutes();

            var h = Math.floor(minutes / 60);
            var m = (minutes - (h * 60)) / 60;
            return h + m;
        }

        // Need to determine the length dynamically. Will need a "Get Max Staffclock record pairs" to determine it from the start.
        function getGraphStacks(length, scale) {

            var stacks = [];
            var stackData = [];
            var colour = '#FF0000';//red
            for (var i = 0; i < length; i++) {
                if (i > 0) {
                    switch (Math.floor(i % 2)) {
                        case 0: colour = '#0000FF'; break;//blue // In the office
                        case 1: colour = '#008000'; break;//green // Before work
                    }
                }
 
                if (scale === 'week') {
                    stacks.push({ data: [[0, 0], [1, 0], [2, 0], [3, 0], [4, 0], [5, 0], [6, 0]], color: colour });
                }
                if (scale === 'month') {
                    for (var j = 0; j < 31; j++) {
                        stackData.push([j, 0]);
                    }
                    stacks.push({ data: stackData, color: colour });
                    stackData = [];
                }
                if (scale === 'year') {
                    for (var l = 0; l < 365; l++) {
                        stackData.push([l, 0]);
                    }
                    stacks.push({ data: stackData, color: colour });
                    stackData = [];
                }
                 
            }

            return stacks;
        }

        // Assembles the graph
        function assembleGraphs(graphList) {
            //var test = labelDates();


            var days = [
                        [0,  "Sun"], [1,   "Mon"], [2,  "Tues"], [3,  "Wed"], [4,  "Thur"], [5,  "Fri"], [6,  "Sat"],
                        [7,  "Sun"], [8,   "Mon"], [9,  "Tues"], [10, "Wed"], [11, "Thur"], [12, "Fri"], [13, "Sat"],
                        [14, "Sun"], [15,  "Mon"], [16, "Tues"], [17, "Wed"], [18, "Thur"], [19, "Fri"], [20, "Sat"],
                        [21, "Sun"], [22,  "Mon"], [23, "Tues"], [24, "Wed"], [25, "Thur"], [26, "Fri"], [27, "Sat"],
                        [28, "Sun"], [29,  "Mon"], [30, "Tues"], [31, "Wed"], [32, "Thur"], [33, "Fri"]
            ];

            var daysM = [
                       [0,  "S"], [1,  "M"], [2,  "T"], [3,  "W"], [4,  "T"], [5,  "F"], [6,  "S"],
                       [7,  "S"], [8,  "M"], [9,  "T"], [10, "W"], [11, "T"], [12, "F"], [13, "S"],
                       [14, "S"], [15, "M"], [16, "T"], [17, "W"], [18, "T"], [19, "F"], [20, "S"],
                       [21, "S"], [22, "M"], [23, "T"], [24, "W"], [25, "T"], [26, "F"], [27, "S"],
                       [28, "S"], [29, "M"], [30, "T"], [31, "W"], [32, "T"], [33, "F"], [34, "S"]
            ];


             
            var options = { xaxis: { mode: "categories", tickLength: 1 }, yaxis: { tickSize: 1 }, series: { bars: { show: true, barWidth: .9, align: "center" }, stack: true } };
            for (var i = 0; i < graphList.length; i++) {
                if (graphList[i].length >= 1) {
                    if (selectedScale() === 'Week') {
                        options.xaxis.ticks = days;
                        //options.xaxis.ticks = labelDateDays();
                    }else if (selectedScale() === 'Month') {
                        options.xaxis.ticks = daysM;
                    }
        
                    var lookfor = "#graph" + i;

                    $.plot(lookfor, graphList[i], options);
                    yAxisLabel(lookfor);
                    xAxisLabels(lookfor);
                }
            }
        }

        //Create y-axis labels
        var yAxisLabel = function (graph) {
            return $("<div class='axisLabel yaxisLabel'></div>")
                .text("Hour")
                .appendTo(graph);
        };
        //Create x-axis labels
        var xAxisLabels = function (graph) {
            return $("<div class='axisLabel xaxisLabel'></div>")
                .text("Day")
                .appendTo(graph);
        };

        //#region Public Members
        var vm = {
            activate: activate,
            viewAttached: viewAttached,
            isloading: isloading,
            enableFiltering: enableFiltering,
            selectedStaff: selectedStaff,
            graphScales: graphScales,
            selectedScale: selectedScale,
            cmbStaffMemberChange: cmbStaffMemberChange,
            getGraphData: getGraphData,
            graphCounter: graphCounter,
            staff: staff,
            authorised: authorised,
            cancel: cancel
        };
        return vm;
        //#endregion
    });