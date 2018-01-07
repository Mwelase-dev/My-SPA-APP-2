define(['services/datacontext', 'config'],
    function (datacontext, config) {
        var model = ko.observable();

        function activate() {
        }

        function viewAttached() {
            var startDate = new Date(model().startPeriod());
            var endDate   = new Date(model().endPeriod());
            $("#startDate").datepicker({
                dateFormat: "dd-mm-yy",
                onClose: function () {
                    var tempDate = $("#startDate").datepicker("getDate");
                    $("#endDate").datepicker("option", "minDate", tempDate);
                }
            }).datepicker("setDate", startDate);
            $("#endDate").datepicker({ dateFormat: "dd-mm-yy", minDate: startDate }).datepicker("setDate", endDate);
        }

        function init(objModel) {
            model(objModel);
        }

        function save(objModel) {
            var tempStart = new Date($("#startDate").datepicker("getDate")).setHours(2, 0, 0, 0);
            objModel.model().startPeriod(new Date(tempStart));

            var tempEnd = new Date($("#endDate").datepicker("getDate")).setHours(2, 0, 0, 0);
            objModel.model().endPeriod(new Date(tempEnd));

            this.modal.close();
        }

        function cancelEdit() {
            var obj = this.model();
            if (obj) {
                obj.entityAspect.rejectChanges();
            }
            this.modal.close();
        }

        var vm = function (objModel) {
            init(objModel);
            this.model = model;
            this.activate = activate;
            this.viewAttached = viewAttached;
            this.save = save;
            this.cancelEdit = cancelEdit;
        };

        return vm;
    });