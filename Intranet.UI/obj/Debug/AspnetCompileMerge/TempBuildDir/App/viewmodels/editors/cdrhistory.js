define([],
    function () {
        function activate() {

        }

        function calculateTotals(phoneRecords) {
            var totalSec = 0, totalCost = 0;
            for (var i = 0; i < phoneRecords.length; i++) {
                var record = phoneRecords[i];
                totalSec += record.callDuration;
                totalCost += record.callCost;
            }
            return "Total Call Length (Sec) : " + totalSec + ", Call Cost : R " + totalCost.toFixed(2);
        }

        var vm = function (phoneRecords) {
            this.activate = activate;
            this.phoneRecords = phoneRecords;
            this.summary = ko.observable(calculateTotals(phoneRecords));
            this.title = ko.observable('Call history for "' + phoneRecords[0].displayDestination + '"');
        };
        return vm;
    });