define(['services/datacontext'],
    function (datacontext) {

        function activate() {
        }

        function viewattached() {
            $("#clockDate").timepicker();
            var clockRecord = this.clockRecord;
            var date    = new Date(moment(clockRecord.clockDateTime()));

            var hour    = date.getHours();
            var minutes = date.getMinutes();
            var defDate = new Date();
            //this is a way around the client adding 2 hours for the time zone.
            //todo -jay: No jay No
            defDate.setHours(hour -2, minutes, 0);
            $("#clockDate").timepicker('setTime', new Date(defDate));
        }

        function updateClockRecord(objModel) {

            var value = $("#clockDate").val().split(":");
            //2 hour zime zone
            var hours = Number(value[0]) + 2;
            var minutes = Number(value[1]);

            var date = new Date(objModel.clockRecord.clockDateTime());

            date = Date.parse(new Date(date.setHours(hours, minutes, 00)));
            
            var prevDate = new Date(objModel.clockRecord.clockDateTime());

            var comments = "Previous '" + (prevDate.getHours() - 2) + ":" + prevDate.getMinutes() + "'\n";
            comments += "New '" + (hours - 2) + ":" + minutes + "'\n";
            comments += "Reason = " + objModel.clockRecord.comments();

            objModel.clockRecord.comments(comments);
            //objModel.clockRecord.clockDate(date);
            objModel.clockRecord.clockDateTime(date);
            objModel.clockRecord.dataStatus(2);

            this.clickSave();
        }

        var vm = function (clockRecord) {
            this.activate = activate;
            this.viewAttached = viewattached;
            this.clockRecord = clockRecord;
            this.updateclockrecord = updateClockRecord;
            //this.clockDateTime = clockDateTime;
        };
        return vm;
    });