define(['services/datacontext'],
    function (datacontext) {

        var editClockDate = ko.observable(true);

        var clockDate = ko.observable();

        function getDefaultDateTime(date) {
            clockDate(moment(date).format('YYYY MMMM Do'));
        }

        function activate() {
        }

        function viewattached() {
            configureEndDatePicker();
            $("#clockDate").timepicker();
            var clockRecord = this.clockRecord;
            var date = new Date(clockRecord.clockDateTime());
            var hour = date.getHours();
            var minutes = date.getMinutes();

            var defDate = new Date();
            //this is a way around the client adding 2 hours for the time zone.
            //todo -jay: No jay No
            defDate.setHours(hour - 2, minutes, 0);

            $("#clockDate").timepicker('setTime', new Date(defDate));

        }

        function configureEndDatePicker() {

            var dateSelected = this.clockRecord;
            $("#clockDate").timepicker(
                   {
                       beforeShowDay: $.datepicker.noWeekends,
                       dateFormat: 'dd-mm-yy',
                       showOtherMonths: true,
                       selectOtherMonths: true,
                       onClose: function (selectedDate) {
                           var tempSelectedDate = $("#clockDate").datepicker("getDate");

                           //var tempSelectedDate = new Date(selectedDate);
                           $("#leaveEnd").datepicker("option", "minDate", tempSelectedDate);

                           $("#leaveEnd").datepicker("setDate", tempSelectedDate);

                           // a person can take a minum of a day's leave
                           //calculateLeaveDays(1);
                       }
                   }).datepicker("setDate", "00:00");
            $("#clockDate").datepicker({ beforeShowDay: $.datepicker.noWeekends });


            $("#clockNewDate").datepicker(
               {
                   beforeShowDay: $.datepicker.noWeekends,
                   dateFormat: 'dd-mm-yy',
                   showOtherMonths: false,
                   selectOtherMonths: false,
                   onClose: function (selectedDate) {
                       var test = this.clockRecord;
                   }
               }).datepicker("setDate", new Date());
            $("#clockNewDate").datepicker({ beforeShowDay: $.datepicker.noWeekends });
            //$("#clockNewDate").timepicker("setTime", new Date());
        }

        function creatRecord(objModel) {
            var offsiteclockin = objModel.clockRecord.clockDateTime();

            var time = $("#clockDate").val();
            var value = time.split(":");
            //2 hour zime zone

            var hours = Number(value[0]);
            var minutes = Number(value[1]);

            var date = new Date($("#clockNewDate").val().replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            date.setHours(hours + 2);
            date.setMinutes(minutes);

            //offsiteclockin = new Date(offsiteclockin.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
            //offsiteclockin.setHours(offsiteclockin.getHours() + 2);

            var comments = "Reason = " + objModel.clockRecord.comments();

            objModel.clockRecord.comments(comments);
            objModel.clockRecord.recordStatus(datacontext.recStatus.statusActive);
            objModel.clockRecord.clockDateTime(date);
            objModel.clockRecord.originalClockDateTime(date);
            objModel.clockRecord.dataStatus(2);
        

            this.clickSave();
        }

        var vm = function (clockRecord) {

            var currentDate = new Date();
            if (clockRecord.clockDataId() < 0) {
                getDefaultDateTime(currentDate);
            }
            else {
                getDefaultDateTime(clockRecord.clockDateTime());
            }

            this.activate = activate;
            this.viewAttached = viewattached;
            this.clockRecord = clockRecord;
            this.editClockDate = editClockDate;
            this.clockDate = clockDate;
            this.createRecord = creatRecord;
        };
        return vm;
    });