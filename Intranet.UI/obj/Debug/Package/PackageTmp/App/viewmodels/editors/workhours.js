define(['services/datacontext'],
    function (datacontext) {
        var initStart, initEnd;
        var work        = ko.observable();
        var editing     = ko.observable(true);
        var selectedDay = ko.observable();
        var canSelDay   = ko.observableArray(editing == true);
        //days of the week
        var days = [
         { Day: "Sunday", Id: 0 },
         { Day: "Monday", Id: 1 },
         { Day: "Tuesday", Id: 2 },
         { Day: "Wednesday", Id: 3 },
         { Day: "Thursday", Id: 4 },
         { Day: "Friday", Id: 5 },
         { Day: "Saturday", Id: 6 }
        ];

        function init(data, isEdit) {
            editing(isEdit);
            work(data);
            if (isEdit) {
                selectedDay(work().dayId());
            } else {
                selectedDay(1);
            }
            initStart = work().displayStartTime();
            initEnd = work().displayEndTime();
        }

        function active() {

        }

        function viewAttached() {
            $("#endTime").timepicker();
            $("#startTime").timepicker();

            $("#endTime").val(initEnd);
            $("#startTime").val(initStart);
        }

        function save(objModel) {
            var wh = objModel.workHours;
            if (!editing()) {
                var day = selectedDay();
                var stId = objModel.workHours().staffId();
                var tempWh = ko.observable(datacontext.getStaffDayWorkHours(stId, day));
                if (tempWh() != null) {
                    wh = tempWh;
                    datacontext.getStaffDayWorkHours(stId, 100).entityAspect.setDeleted();
                }
                wh().recordStatus(datacontext.recStatus.statusActive);
            }
            wh().dayId(selectedDay());

            var startValue = $("#startTime").val().split(":");
            var endValue = $("#endTime").val().split(":");

            //2 hour zime zone
            //This a way around to the day time savings
            //===================================>
            var startHour = Number(startValue[0]);
            var startMinutes = Number(startValue[1]);

            var endHour = Number(endValue[0]);
            var endMinute = Number(endValue[1]);
            //===================================>

            //Set the actual time selected by the user
            //====================================>
            var tempEnd = new Date();
            wh().dayTimeEnd(new Date(tempEnd.getFullYear(), tempEnd.getMonth(), tempEnd.getDate(), endHour, endMinute, 0, 0).setHours(endHour));

            var tempDate = new Date();
            wh().dayTimeStart(new Date(tempDate.getFullYear(), tempDate.getMonth(), tempDate.getDate(), startHour, startMinutes, 0, 0));

            this.modal.close();
            //====================================>
        }

        function cancelEdit() {
            if (work()) {
                work().entityAspect.rejectChanges();
            }
            this.modal.close();
        }

        function onDaySelect() {

        }

        var vm = function (wh, isEdit) {

            init(wh, isEdit);

            this.active = active;
            this.viewAttached = viewAttached;
            this.workHours = work;
            this.editing = editing;
            this.canSelDay = canSelDay;
            this.days = days;
            this.save = save;
            this.cancelEdit = cancelEdit;
            this.onDaySelect = onDaySelect;
            this.selectedDay = selectedDay;
        };
        return vm;
    });