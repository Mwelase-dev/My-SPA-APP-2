define(['services/datacontext'],
    function (datacontext) {
        var originalClockRecord = ko.observable();
        var clockModels = ko.observableArray();
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
            $("#clockDate").datetimepicker('setTime', new Date(defDate));
        }

        function init(ori) {
            originalClockRecord(ori);
        }

        function saveOriginal() {
            var ori= ko.observable();
            var ctx = datacontext.manufactureEntity("StaffClockModel");
            ctx.recordStatus = ko.observable(datacontext.recStatus.statusActive);
            ori(ctx);
            
            var date = new Date(originalClockRecord().clockDateTime());
            

            ori().dataStatus(8);
            ori().clockDateTime(date);
            ori().originalClockDateTime(date);
            //ori().clockDataId(originalClockRecord().clockDataId());
            ori().comments(originalClockRecord().comments());
            ori().isLeaveRecord(originalClockRecord().isLeaveRecord());
            ori().staff(originalClockRecord().staff());
            ori().staffId(originalClockRecord().staffId());
            //ori().displayDate(originalClockRecord().displayDate);

            clockModels.push(ori()); //datacontext.changesSave();
            //this.clickSave();
            //return datacontext.updateButKeepOriginalClockRecord(originalClockRecord()).then(function() {

            //});
        }

        function updateClockRecord(objModel) {
            saveOriginal();

            var test2 = originalClockRecord();
            var time = $("#clockDate").val();
            var value = time.split(":");
            //2 hour zime zone
            var hours = Number(value[0]);
            var minutes = Number(value[1]);

            var date = new Date(objModel.clockRecord.clockDateTime());
            date.setHours(hours+2);
            date.setMinutes(minutes);
        
            var prevDate = new Date(objModel.clockRecord.clockDateTime());

            var test = "";

            var comments = "Previous '" + (prevDate.getHours() - 2) + ":" + prevDate.getMinutes() + "'\n";
            comments += "New '" + (hours) + ":" + minutes + "'\n";
            comments += "Reason = " + objModel.clockRecord.comments();

            objModel.clockRecord.comments(comments);
            objModel.clockRecord.clockDateTime(date);
            objModel.clockRecord.originalClockDateTime(date);
            objModel.clockRecord.dataStatus(2);

            clockModels.push(objModel.clockRecord);
            this.clickSave();
            //datacontext.changesSave();
            //this.close();
            //var ctx = datacontext.manufactureEntity("StaffClockModel");
            //ctx.recordStatus = ko.observable(datacontext.recStatus.statusActive);
            //printer(ctx);
            //objModel.clockRecord.clockDateTime(originalClockRecord().clockDateTime());
            //objModel.clockRecord.originalClockDateTime(originalClockRecord().clockDateTime());
            //objModel.clockRecord.dataStatus(8);
             
            //this.clickSave();
        }

        /*
         function updateClockRecord(objModel) {

            var time = $("#clockDate").val().split(" ");
            var value = time[1].split(":");
            //2 hour zime zone
            var hours = Number(value[0]) + 2;
            var minutes = Number(value[1]);

            var date = new Date(time[0]);

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
        */

        var vm = function (clockRecord) {
            init(clockRecord);
            this.activate = activate;
            this.viewAttached = viewattached;
            this.clockRecord = clockRecord;
            this.updateclockrecord = updateClockRecord;
            //this.clockDateTime = clockDateTime;
        };
        return vm;
    });