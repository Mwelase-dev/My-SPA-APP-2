define(['services/logger', 'services/datacontext', 'viewmodels/editors/baseeditor', 'viewmodels/editors/cdrhistory'],
    function (logger, dataContext, editor, cdrhistory) {
        var phonerecords = ko.observableArray();
        var staff        = ko.observable();
        var staffMem     = ko.observable();
        var userRoles    = ko.observableArray();
        var totLength    = ko.observable();
        var totCost      = ko.observable();
        var canSearch    = ko.observable(false);
        var searching    = ko.observable(false);
        var authorised   = ko.observable(true);
        var summary      = ko.observable();
        var extension    = ko.observable();
        var defStart     = new Date();
        var defEnd       = new Date();
        defStart.setDate(defStart.getDate() - 7);

        function activate() {
            // get the current user roles
            return dataContext.getCurrentUserRoles(userRoles).then(function() {
                if (isInRole(dataContext.userRoleEnum.humanResource)) {
                    return dataContext.getAllStaff(staff);
                }
                else if (isInRole(dataContext.userRoleEnum.manager)) {
                    return dataContext.getCurrentUser(staffMem).then(function () {
                        return dataContext.getAllStaff(staff, staffMem);
                    });
                } else {
                    return Q.fcall(function () {
                        //todo - jay : clean up may cause perfomance issues
                        return dataContext.getCurrentUser(staffMem).then(function () {
                            extension(staffMem().staffClockId());
                            authorised(extension() != null);
                            staff(staffMem());
                        });
                    });
                }
            });
        }

        function isInRole(role) {
            return $.inArray(role, userRoles()) > -1;
        }

        function viewattached() {
            $("#startDate").datepicker({
                dateFormat: "dd-mm-yy",
                onClose: function () {
                    var tempDate = $("#startDate").datepicker("getDate");
                    $("#endDate").datepicker("option", "minDate", tempDate);
                }
            }).datepicker("setDate", defStart);
            $("#endDate").datepicker({ dateFormat: "dd-mm-yy", minDate: defStart }).datepicker("setDate", defEnd);
        }
        
        function getStaffCDRSummary() {
            var startDate = new Date($("#startDate").datepicker("getDate"));
            var endDate   = new Date($("#endDate").datepicker("getDate"));

            canSearch(false);
            searching(true);

            return dataContext.getStaffCDRSummary(phonerecords, new Date(startDate).toString(), new Date(endDate).toString(), extension()).then(function () {
                buildSummary();
                canSearch(true);
                searching(false);
            });
        }

        function cmbStaffChanged() {
            canSearch(extension() != null);
        }

        function buildSummary() {
            var totalSec = 0, totalCost = 0, averageCallCost = 0, averageCallLength = 0;
            for (var i = 0; i < phonerecords().length; i++) {
                var record = phonerecords()[i];
                totalSec  += record.callDuration;
                totalCost += record.totalCallCost;
            }
            totLength(totalSec);
            totCost(totalCost);
            summary("Total Call Length (Sec) : " + totalSec + ", Call Cost : R" + totalCost.toFixed(2));
        }

        function expandCDR(objModel) {
            preventClick();
            var startDate    = new Date($("#startDate").datepicker("getDate"));
            var endDate      = new Date($("#endDate").datepicker("getDate"));
            var phoneRecords = ko.observableArray();
            return dataContext.GetCDRSummary(phoneRecords, startDate, endDate, extension(), objModel.destination).then(function () {
                    editor.objEdit(new cdrhistory(phoneRecords()));
                });

            function preventClick() {
                var event = $(document).click(function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                });

                // disable right click
                $(document).bind('contextmenu', function (e) {
                    e.stopPropagation();
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    return false;
                });
            }

            function enableClick() {
                $(document).unbind('click');
                $(document).unbind('contextmenu');
            }
        }

        var vm = {
            activate: activate,
            viewAttached: viewattached,

            summary: summary,
            extension: extension,

            phonerecords: phonerecords,
            staff: staff,

            canSearch: canSearch,
            searching: searching,
            authorised: authorised,

            search: getStaffCDRSummary,
            expandCDR: expandCDR,
            cmbStaffChanged: cmbStaffChanged,

        };

        return vm;

    });