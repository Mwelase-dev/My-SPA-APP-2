define(['services/datacontext', 'services/model', 'durandal/app'],
    function (datacontext, model, app) {
        var staffId;
        var staff = ko.observable();
        var leaveTypes = ko.observableArray();
        var canSave = ko.observable(true);
        var staffList = ko.observableArray();
        var selectedStaff = ko.observableArray();
        var leaveApplications = ko.observableArray();
        var leaveApplication = ko.observable();

        var initialLeaveDays;
        var initSickLEaveDays;

        function sortStaff() {
            staffList().sort(function (a, b) {
                return a.fullName() < b.fullName() ? -1 : 1;
            });
        };

        function activate() {
            return datacontext.allStaff(staffList).then(function () {
                //sortStaff();

                return datacontext.getCurrentUserWithLeaveData(staff).then(function () {
                    initialLeaveDays = staff().DaysDue();
                    initSickLEaveDays = staff().SickDaysAvilable();
                    var leaveInit = {
                        leaveStatus: datacontext.leaveStatusEnum.pending,
                        staffId: staff().staffId(),
                        leaveRequestDate: new Date(),
                        recordStatus: datacontext.recStatus.statusActive,
                    };

                    return datacontext.leaveTypes(leaveTypes).then(function () {
                        return canSave(true);
                    });
                });
            });
        }

        // ggggg  gggggg  gggggggg  ggggggg  ggggggggggggg
        /*
        $("#clockDate").timepicker();
            var clockRecord = this.clockRecord;
            var date = new Date(clockRecord.clockDateTime());
            var hour = date.getHours();
            var minutes = date.getMinutes();
            var defDate = new Date();
            defDate.setHours(hour - 2, minutes, 0);
        */
        // aaaaa  aaaaa aaaaa aaaaa aaaaaa aaaaaaaaaaaaaaa

        function getDefaultDateTime() {
            start(moment(date).format('YYYY MMMM Do'));
            end(moment(date).format('YYYY MMMM Do'));
        }

        function approveMultiple() {

            if (selectedStaff().length > 0) {
                if ($("#motivation").val() != "") {

                    var start = $("#start").val();
                    start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
                    start.setHours(start.getHours() + 2);
                    start.setMinutes(start.getMinutes());



                    var end = $("#end").val();
                    end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));
                    end.setHours(end.getHours() + 2);
                    end.setMinutes(end.getMinutes());

                    var comm = $("#motivation").val();

                    for (var x = 0; x < selectedStaff().length; x++) {
                        var leaveInit = {
                            leaveStatus: datacontext.leaveStatusEnum.approved,
                            staffId: selectedStaff()[x].staffId(),
                            leaveRequestDate: new Date(),
                            recordStatus: datacontext.recStatus.statusActive,
                        };
                        leaveApplication(datacontext.manufactureEntity(model.entityNames.staffLeaveName, leaveInit, true));
                        leaveApplications.push(leaveApplication());
                    }

                    var apps = leaveApplications();
                    var count = 0;

                    for (var i = 0; i < apps.length; i++) {
                        var approved = apps[i].approve() == true;
                        var stats = apps[i].leaveStatus() == datacontext.leaveStatusEnum.pending;
                        if (approved === stats) {
                            //check what manager the user is
                            //if manager is both manager 1 and manager 2
                            if (apps[i].staffMember().staffManager1Id() == staff().staffId() && apps[i].staffMember().staffManager2Id() == staff().staffId()) {
                                apps[i].approvedBy1(staff().staffId());
                                apps[i].approvedBy2(staff().staffId());
                            } else if (apps[i].staffMember().staffManager1Id() == staff().staffId())
                                apps[i].approvedBy1(staff().staffId());
                            else if (apps[i].staffMember().staffManager2Id() == staff().staffId())
                                apps[i].approvedBy2(staff().staffId());
                            else
                                Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
                                });

                            if (typeof apps[i].approvedBy1() !== "undefined" && typeof apps[i].approvedBy2() !== "undefined") {
                                apps[i].leaveStatus(datacontext.leaveStatusEnum.approved);
                                apps[i].leaveType(datacontext.leaveTypeEnum.special);
                                apps[i].leaveComments(comm);
                                apps[i].leaveDateStart(start);
                                apps[i].leaveDateEnd(end);
                            }
                            datacontext.emailLeaveApproved(apps[i].leaveId());
                            count++;
                        }
                    }

                    if (count > 0) {
                        return Q.fcall(function () {
                            datacontext.changesSave().then(function () {

                                app.showMessage("Leave application approved.", 'Success', ['Close']);
                                //Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
                                //});
                            });
                        });
                    }
                } else {
                    Q.resolve(app.showMessage("A reason for applying for special leave is compulsory for record purposes", "Special Leave Application", ['Close'])).then(function (data) {
                        router.navigateTo('#/view_staffmenu');
                        return;
                    });
                }
            } else {
                Q.resolve(app.showMessage("Atleast one person must be selected inorder for this special leave to be added.", "Special Leave Application", ['Close'])).then(function (data) {
                    this.modal.close();
                });

            }
        }





















        //function approveMultiple() {

        //    if (selectedStaff().length > 0) {
        //        if ($("#motivation").val() != "") {

        //            var start = $("#start").val();
        //            start = new Date(start.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

        //            var end = $("#end").val();
        //            end = new Date(end.replace(/(\d{2})-(\d{2})-(\d{4})/, "$2/$1/$3"));

        //            var comm = $("#motivation").val();

        //            for (var x = 0; x < selectedStaff().length; x++) {
        //                var leaveInit = {
        //                    leaveStatus: datacontext.leaveStatusEnum.approved,
        //                    staffId: selectedStaff()[x].staffId(),
        //                    leaveRequestDate: new Date(),
        //                    recordStatus: datacontext.recStatus.statusActive,
        //                };
        //                leaveApplication(datacontext.manufactureEntity(model.entityNames.staffLeaveName, leaveInit, true));
        //                leaveApplications.push(leaveApplication());
        //            }

        //            var apps = leaveApplications();
        //            var count = 0;

        //            for (var i = 0; i < apps.length; i++) {
        //                var approved = apps[i].approve() == true;
        //                var stats = apps[i].leaveStatus() == datacontext.leaveStatusEnum.pending;
        //                if (approved === stats) {
        //                    //check what manager the user is
        //                    //if manager is both manager 1 and manager 2
        //                    if (apps[i].staffMember().staffManager1Id() == staff().staffId() && apps[i].staffMember().staffManager2Id() == staff().staffId()) {
        //                        apps[i].approvedBy1(staff().staffId());
        //                        apps[i].approvedBy2(staff().staffId());
        //                    } else if (apps[i].staffMember().staffManager1Id() == staff().staffId())
        //                        apps[i].approvedBy1(staff().staffId());
        //                    else if (apps[i].staffMember().staffManager2Id() == staff().staffId())
        //                        apps[i].approvedBy2(staff().staffId());
        //                    else
        //                        Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function() {
        //                        });

        //                    if (typeof apps[i].approvedBy1() !== "undefined" && typeof apps[i].approvedBy2() !== "undefined") {
        //                        apps[i].leaveStatus(datacontext.leaveStatusEnum.approved);
        //                        apps[i].leaveType(datacontext.leaveTypeEnum.special);
        //                        apps[i].leaveComments(comm);
        //                        apps[i].leaveDateStart(start);
        //                        apps[i].leaveDateEnd(end);
        //                    }
        //                    datacontext.emailLeaveApproved(apps[i].leaveId());
        //                    count++;
        //                }
        //            }

        //            if (count > 0) {
        //                return Q.fcall(function() {
        //                    datacontext.changesSave().then(function() {

        //                        app.showMessage("Leave application approved.", 'Success', ['Close']);
        //                        //Q.resolve(app.showMessage("Unable to approve leave application", "Failure", ['Close'])).then(function () {
        //                        //});
        //                    });
        //                });
        //            }
        //        } else {
        //            Q.resolve(app.showMessage("A reason for applying for special leave is compulsory for record purposes", "Special Leave Application", ['Close'])).then(function(data) {
        //                router.navigateTo('#/view_staffmenu');
        //                return;
        //            });
        //        }
        //    } else {
        //        Q.resolve(app.showMessage("Atleast one person must be selected inorder for this special leave to be added.", "Special Leave Application", ['Close'])).then(function (data) {
        //            this.modal.close();
        //        });

        //    }
        //}

        function viewAttached() {
            setDatetimepicker();
        }

        function selectDeselect() {
            var listbox = document.getElementById("staffnames");

            for (var i = 0; i < listbox.options.length; i++) {
                if (listbox.options[i].selected === true) {
                    listbox.options[i].selected = false;
                } else {
                    listbox.options[i].selected = true;
                }
            }
        }

        var setDatetimepicker = function () {
            //$("#start").datetimepicker({
            //    //pickDate: false
            //});
            //$("#end").datetimepicker({
            //    //pickDate: false
            //});
            configureStartDatePicker();
            configureEndDatePicker();
        };

        function configureStartDatePicker() {
            //$("#leaveStart").datetimepicker({
            //    beforeShowDay: $.datepicker.noWeekends,
            //    });
            $("#start").datetimepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd-mm-yy',
                    showOtherMonths: false,
                    selectOtherMonths: false,
                    onClose: function (selectedDate) {
                        var tempSelectedDate = $("#start").datepicker("getDate");

                        //var tempSelectedDate = new Date(selectedDate);
                        $("#end").datepicker("option", "minDate", tempSelectedDate);
                        $("#end").datepicker("setDate", tempSelectedDate);

                        // a person can take a minum of a day's leave
                        calculateLeaveDays(1);
                    }
                }).datepicker("setDate", new Date());
            $("#start").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        function configureEndDatePicker() {
            //$("#leaveEnd").datetimepicker({
            //    beforeShowDay: $.datepicker.noWeekends,
            //    });
            $("#end").datetimepicker(
                {
                    beforeShowDay: $.datepicker.noWeekends,
                    dateFormat: 'dd-mm-yy',
                    onClose: function (selectedDate) {
                        calculateLeaveDays(1);
                    }
                }
            ).datepicker("setDate", new Date());
            $("#end").datepicker("option", "minDate", new Date());
            $("#end").datepicker({ beforeShowDay: $.datepicker.noWeekends });
        }

        var vm = function (id) {
            staffId = id;
            this.staff = staff;
            this.active = activate();
            this.leaveTypes = leaveTypes;
            this.viewAttached = viewAttached;
            this.staffList = staffList;

            this.selectDeselect = selectDeselect;
            this.leaveApplications = leaveApplications,
            this.selectedStaff = selectedStaff;
            this.approveMultiple = approveMultiple;

            this.getDefaultDateTime = getDefaultDateTime;
        };

        return vm;
    });