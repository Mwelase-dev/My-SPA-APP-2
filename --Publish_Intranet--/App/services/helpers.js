define(
    function() {

        function calculateLeaveDays(lstart, lend) {

            //todo cater for half day
            var leaveDays = 1;
            var weekendCounter = 0;
            
            //convert incoming parameters to dates
            var start = new Date(lstart);
            var end = new Date(lend);
            
            var startValue = new Date(start);

            var datediff = end.getTime() - start.getTime();
            datediff = (datediff / (24 * 60 * 60 * 1000));

            if (datediff < 0.3333333333333333) {
                return 0;
            }

            if (start.toDateString() == end.toDateString())
                return leaveDays;
            
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

            return leaveDays - weekendCounter;
        }

        function calculateLeaveHours(lstart, lend) {
            var hours = "08:00:00";

            //convert incoming parameters to dates
            var start = new Date(lstart);
            var end = new Date(lend);
            var startValue = new Date(start);

            var datediff = end.getTime() - start.getTime();
            datediff = (datediff / (24 * 60 * 60 * 1000));

            //moment("2015-01-16T12:00:00").format("hh:mm:ss a")
            var startTime = moment(start).format("hh:mm:ss a");
            var endTime = moment(end).format("hh:mm:ss a");

            var timespan = moment.utc(moment(end, "DD/MM/YYYY HH:mm:ss").diff(moment(start, "DD/MM/YYYY HH:mm:ss"))).format("HH:mm:ss");

            return timespan;
        }
         
        function retunToPrevPage(callback) {
            return history.go(-1);
        }
     
        var helper = {
            calculateLeaveDays: calculateLeaveDays,
            calculateLeaveHours : calculateLeaveHours,
            returnToPrevPage:retunToPrevPage,
        };

        return helper;
    });