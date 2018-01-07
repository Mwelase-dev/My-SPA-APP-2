define(['services/datacontext'],
    function (dataContext) {

        function calculateLeaveDaysForLeaveApplication(lstart, lend) {
            var days = ko.observable(0);
            var hours = ko.observable();
            var result = "";
            var start = new Date(lstart);
            var end = new Date(lend);


            if (start.toDateString() != end.toDateString()) {
                //todo cater for half day
                var leaveDays = 0;
                var weekendCounter = 0;
                var startValue = new Date(start);

                while (startValue.toDateString() != end.toDateString()) {
                    leaveDays += 1;
                    //check if its a weekend
                    var dayOfWeek = startValue.getDay();
                    if (dayOfWeek == 6 || dayOfWeek == 0)
                        weekendCounter += 1;

                    //add a day to the start days
                    var t = moment(startValue);
                    var tr = moment(startValue).add('days', 1);
                    startValue = new Date(moment(startValue).add('d', 1));
                }
                days(leaveDays - weekendCounter);
                result += days();
            }
            result += "T ";
            var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

            //if (timespan > "08:00:00")
            //    return "08:00:00";
            hours(timespan);
            result += hours();

            if (hours() >= "08:00:00") {
                hours(0);
                var test = ko.observable();
                test(days());
                if (test() === 0) {
                    test(test() + 1);
                    //revisedLeaveDays([]);
                    days(test());

                } else if (test() > 0) {
                    test(test() + 1);
                    days(test());
                }
            }
            result = days() + "T" + hours();
            return result;
        }

        function calculateLeaveDaysWithHours(lstart, lend, disabledDays) {
            //todo cater for half day
            var dateendTester = new Date(lend);
            var datesterTester = new Date(lstart);
            if (dateendTester < datesterTester) {
                return 0;
            }
            if (dateendTester.getFullYear() != 1900) {
                if (datesterTester.getFullYear() != 1900) {
                    var leaveDays = 0;
                    var weekendCounter = 0;

                    //convert incoming parameters to dates
                    var start = new Date(lstart);

                    var end = new Date(lend);

                    //var disabledDays = ko.observableArray();
                    var publicHolidayCounter = 0;


                    //return dataContext.getPublicHolidays(disabledDays).then(function () {
                    var endD = new Date(end);
                    var startD = new Date(start);
                    if (endD.getDate() !== startD.getDate()) {
                        while (endD.toDateString() != startD.toDateString()) {
                            var date = new Date(startD);
                            var m = date.getMonth(), d = date.getDate(), y = date.getFullYear();
                            for (var i = 0; i < disabledDays.length; i++) {
                                var holidayDate = new Date(disabledDays[i]);
                                var m2 = holidayDate.getMonth();
                                var d2 = holidayDate.getDate();
                                var y2 = holidayDate.getFullYear();
                                if ((y + '-' + (m + 1) + '-' + d) === (y2 + '-' + (m2 + 1) + '-' + d2)) {
                                    publicHolidayCounter++;
                                }
                            }
                            startD = new Date(moment(startD).add('d', 1));
                        }
                    }

                    var startValue = new Date(start);

                    var datediff = end.getTime() - start.getTime();
                    datediff = (datediff / (24 * 60 * 60 * 1000));

                    if (datediff < 0.3333333333333333) {
                        return 0;
                    }

                    //if (start.toDateString() == end.toDateString()) {
                    //    return leaveDays;
                    //}

                    while (startValue.toDateString() != end.toDateString()) {
                        leaveDays += 1;

                        //check if its a weekend
                        var dayOfWeek = startValue.getDay();
                        if (dayOfWeek == 6 || dayOfWeek == 0)
                            weekendCounter += 1;

                        //add a day to the start days
                        var t = moment(startValue);
                        var tr = moment(startValue).add('days', 1);
                        startValue = new Date(moment(startValue).add('d', 1));
                    }

                    var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

                    if (timespan >= "08:00:00")
                        leaveDays += 1;
                    return leaveDays - weekendCounter - publicHolidayCounter;
                    //});
                }
            }
        }

        function calculateLeaveHoursHours(lstart, lend) {
            //convert incoming parameters to dates
            var start = new Date(lstart);
            var end = new Date(lend);

            var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

            if (timespan >= "08:00:00")
                return 0;

            timespan = timespan.substring(0, 5);
            return timespan;
        }


        function calculateLeaveDays(lstart, lend) {
            //todo cater for half day
            var leaveDays = 0;
            var weekendCounter = 0;
            var holidayCounter = 0;

           //var str = lstart.replace(/-/g, '/');  // replaces all occurances of "-" with "/"
           // var dateObject = new Date(str);

           //var str2 = lend.replace(/-/g, '/');  // replaces all occurances of "-" with "/"
           // var dateObject2 = new Date(str2);



            var start = new Date(lstart);
            var end = new Date(lend);
            var startValue = new Date(start);

            while (startValue.toDateString() != end.toDateString()) {
                leaveDays += 1;

                //check if it's public holdiday

                var dayOfWeek = startValue.getDay();
                if (dayOfWeek == 6 || dayOfWeek == 0)
                    weekendCounter += 1;

                //add a day to the start days
                var t = moment(startValue);
                var tr = moment(startValue).add('days', 1);
                startValue = new Date(moment(startValue).add('d', 1));
            }

            var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");
            if (timespan >= "08:00:00")
                leaveDays += 1;
            return leaveDays - weekendCounter;
            //});
        }

        function calculateLeaveHours(lstart, lend) {
            //convert incoming parameters to dates
            var start = new Date(lstart);
            var end = new Date(lend);

            var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

            if (timespan >= "08:00:00")
                return 0;

            return timespan;
        }

        function refineDaysAndHours(obj) {
            if (obj === undefined || obj === null)
                return obj;

            var revisedLeaveHours = ko.observable();
            var revisedLeaveDays = ko.observable();

            //for (var i = 0; i < obj.length; i++) {

            var staffHours = ko.observableArray();

            //return dataContext.staffWorkHours(staffHours, obj.staffId()).then(function () {
            revisedLeaveHours(obj.displayLeaveHours());
            revisedLeaveDays(obj.displayLeaveDays());
            //var timespan = moment.utc(moment(staffHours()[0].displayEndTime(), "HH:mm:ss").diff(moment(staffHours()[0].displayStartTime(), "HH:mm:ss"))).format("HH:mm:ss");

            if (revisedLeaveHours() === "08:00:00") {
                revisedLeaveHours(0);
                var test = ko.observable();
                test(revisedLeaveDays());
                if (test() === 0) {
                    test(test() + 1);
                    //revisedLeaveDays([]);
                    revisedLeaveDays(test);

                } else if (test() > 0) {
                    test(test() + 1);
                    revisedLeaveDays(test());
                }
            }

            obj.displayLeaveHours = revisedLeaveHours();
            obj.displayLeaveDays = revisedLeaveDays();
            //}
            return obj;
            //});
        }

        function retunToPrevPage(callback) {
            return history.go(-1);
        }

        var helper = {
            calculateLeaveDays: calculateLeaveDays,
            calculateLeaveHours: calculateLeaveHours,
            returnToPrevPage: retunToPrevPage,
            refineDaysAndHours: refineDaysAndHours,
            calculateLeaveDaysForLeaveApplication: calculateLeaveDaysForLeaveApplication,
            calculateLeaveDaysWithHours: calculateLeaveDaysWithHours,
            calculateLeaveHoursHours: calculateLeaveHoursHours
        };

        return helper;
    });