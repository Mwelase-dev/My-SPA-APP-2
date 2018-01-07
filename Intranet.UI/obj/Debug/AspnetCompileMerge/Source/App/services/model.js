define(['config', 'services/helpers'],
    function (config, helper) {
        //#region Client side helpers
        // Entity names dictionary
        var entityNames = {
            menuName                     : 'Menu',
            announcementName             : 'AnnouncementModel',
            branchName                   : 'BranchModel',
            branchDivisionName           : 'DivisionModel',
            staffMemberName              : 'StaffModel',
            suggestionName               : 'SuggestionModel',
            staffClockDataName           : "StaffClockModel",
            thoughtName                  : 'ThoughtModel',
            linkCategory                 : 'LinkCategoryModel',
            linkName                     : 'LinkModel',
            staffLeaveName               : 'StaffLeaveModel',
            clockDataName                : 'StaffClockModel',
            printerPropertiesPrinterName : 'PrinterPropertiesPrinter',
            staffSuggestionVotesName     : 'StaffSuggestionVotesModel',
            staffLeaveCounterName        : 'StaffLeaveCounterModel',
            StaffHoursModelName          : 'StaffHoursModel'
        };
        //#endregion

        // Helper Functions
        function getCTorName(modelObject) {
            var funcNameRegex = /function (.{1,})\(/;
            var results = (funcNameRegex).exec((modelObject).constructor.toString());
            var typeName = (results && results.length > 1) ? results[1] : "";
            return typeName;
        }
        function addDefaultExtension(modelObject) {
            modelObject.recordStatus = ko.observable('Active');
            // Required to point the viewlocator into the right direction
            modelObject.getView = function () { return config.editorViews + getCTorName(modelObject); };
            return modelObject;
        }

        //#region Model Extensions
        // Client side extensions for Announcements
        function announcement() {
            // We need this for Editing purposes as the viewLocator is trying to resolve the ctor name when trying to locate the view for the object
            return this;
        }
        function announcementInitializer(modelObject) {
            addDefaultExtension(modelObject);

            modelObject.announcementDate = ko.observable(new Date());
            modelObject.isVisible = ko.observable(true);
            modelObject.isArchived = ko.observable(modelObject.recordStatus === 'Archive');
            modelObject.displayDate = ko.computed(function () {
                var start = modelObject.announcementDate();
                var value = (start && moment(start).isValid()) ? moment(start).format('DD MMM YYYY') : '[Unknown]';
                return value;
            });
        }

        // Client side extensions for Branches
        function branch() {
            this.DisplayTotalCallCost = ko.observable();
            return this;
        }
        function branchInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        // Client side extensions for Branch Divisions
        function division() {
            this.DisplayTotalCallCost = ko.observable();
            return this;
        }
        function divisionInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        // Client side extensions for StaffBrief
        function staff() {
            this.StaffDob = ko.observable();
            this.StaffBirthday = ko.observable();
            this.DisplayTotalCallCost = ko.observable();
            this.Roles = ko.observable();
            this.AnnualDaysTaken = ko.observable();
            this.SickDaysTaken = ko.observable();
            this.LeaveDaysAccumulated = ko.observable();
            this.DaysDue = ko.observable();
            this.DisplayDaysDue = ko.observable();
            this.CurrentLeaveCycle = ko.observable();
            this.SickDaysAvilable = ko.observable();
            this.StaffLeaveIncrement = ko.observable();
            this.AnnualLeaveHoursAccumulated = ko.observable();
            this.AnnualDays = ko.observable();
            //LM:additions
            this.StaffJoinDate = ko.observable();
            this.CurrentCycleNumber = ko.observable();
            this.CycleEnd = ko.observable();
            this.CycleStart = ko.observable();
            this.CycleDays = ko.observable();
            this.LeaveDaysAccumulated = ko.observable();
            this.AnnualDaysTaken = ko.observable();
            this.DaysDue = ko.observable();
            this.SickCycleEnd = ko.observable();
            this.StaffClockData = ko.observable();
            this.SickCycleDays = ko.observable();
            return this;
        }
        


        function staffInitializer(modelObject) {
            addDefaultExtension(modelObject);
            modelObject.fullName = ko.computed(function () {
                return modelObject.staffName() + ' ' + modelObject.staffSurname();
            });
            modelObject.isToday = ko.computed(function () {
                var start = modelObject.StaffBirthday();
                var value = (start && moment(start).isValid()) ? moment(start).format('YYYY-MM-DD') : '[Unknown]';
                var a = new Date(value);
                var b = new Date();
                return a.getDate() === b.getDate();
            });
            modelObject.fullName = ko.computed(function () {
                return modelObject.staffName() + ' ' + modelObject.staffSurname();
            });
            modelObject.displayDate = ko.computed(function () {
                var start = modelObject.StaffBirthday();
                var value = (start && moment(start).isValid()) ? moment(start).format('DD MMM') : '[Unknown]';
                return value + ' - ' + modelObject.fullName();
            });
            modelObject.displayStaffJoinDate = ko.computed(function () {
                var start = modelObject.staffJoinDate();

                if (typeof start === "undefined" || start == null) {
                    return '[Unkowm';
                }

                return (start && moment(start).isValid()) ? moment(start).format(config.localDateformat) : '[Unknown]';
            });

            modelObject.displayCurrentLeaveCycle = ko.computed(function () {
                var start = modelObject.CurrentLeaveCycle();

                if (typeof start === "undefined" || start == null) {
                    return '[Unkowm';
                }

                return (start && moment(start).isValid()) ? moment(start).format(config.localDateformat) : '[Unknown]';

            });
        }

        //Client side extensions for Thoughts
        function thought() {
            return this;
        }
        function thoughtInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        // Client side extensions for Suggestions
        function suggestion() {
            this.suggestionDate = ko.observable(new Date());
             this.staffId = ko.observable();
            return this;
        }
        function suggestionInitializer(modelObject) {
            addDefaultExtension(modelObject);
            modelObject.displayDate = ko.computed(function () {
                var start = modelObject.suggestionDate();
                var value = (start && moment(start).isValid()) ? moment(start).format('DD MMM YYYY') : '[Unknown]';
                return value;
            });
            modelObject.staffList = ko.observableArray();
        }

        function suggestionVotes() {
            return this;
        }
        function suggestionVotesInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        // Client side extensions for Link Categories
        function category() {
            return this;
        }
        function categoryInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        // Client side extensions for Link Categories
        function link() {
            return this;
        }
        function linkInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }
        //#endregion

        //Client side extension for staffClockingData
        function clockData() {
            this.IsLeaveRecord = ko.observable();
            return this;
        }
        function clockDataInitializer(modelObject) {
            modelObject.displayDate = ko.computed(function () {
                var start = modelObject.clockDateTime();
                var value = (start && moment(start).isValid()) ? moment(start).format('DD MMM YYYY, HH:mm') : '[Unknown]';
                return value;
            });
        }

        //client side extension for printer properties printer
        function printerPropertiesPrinter() {
            this.Order = ko.observable(false);
            return this;
        }
        function printerPropertiesPrinterInitializer(modelObject) {
            addDefaultExtension(modelObject);
        }

        //staff leave model
        function staffLeave() {
            this.approve = ko.observable(false);
        }
        function staffLeaveInitializer(modelObject) {

            modelObject.displayLeaveStart = ko.computed(function () {
                var start = new Date(modelObject.leaveDateStart());
                return (start && moment(start).isValid()) ? moment(start).format(config.localDateformat) : '[Unknown]';
            });

            modelObject.displayLeaveEnd = ko.computed(function () {
                var start = new Date(modelObject.leaveDateEnd());
                return (start && moment(start).isValid()) ? moment(start).format(config.localDateformat) : '[Unknown]';
            });

            modelObject.displayLeaveDays = buildLeaveDays(modelObject.leaveDateStart(), modelObject.leaveDateEnd());

            modelObject.displayLeaveHours = buildLeaveHours(modelObject.leaveDateStart(), modelObject.leaveDateEnd());

            modelObject.displayLeaveStatus = ko.computed(function () {
                var status = modelObject.leaveStatus();

                switch (status) {
                    case 1:
                        return "Approved";
                    case 2:
                        return "Pending";
                    case 3:
                        return "Declined";
                    case 4:
                        return "Cancelled";
                    default:
                        return "Invalid";
                }
            });

            modelObject.displayLeaveType = ko.computed(function () {
                var status = modelObject.leaveType();

                switch (status) {
                    case 1:
                        return "Annual";
                    case 2:
                        return "Sick";
                    case 3:
                        return "Study";
                    case 4:
                        return "Family Responsibility";
                    case 5:
                        return "Annual Unpaid";
                    case 6:
                        return "Working Off Site";
                    case 7:
                        return "Special";
                    default:
                        return "Invalid";
                }
            });

        }

        function staffLeaveCounter() {
            this.canRemove = ko.observable(true);
        }
        function staffLeaveCounterInitializer(modelObject) {
            modelObject.displayStart = ko.computed(function () {
                var start = new Date(modelObject.startPeriod());
                return (start && moment(start).isValid()) ? moment(start).format(config.localDateformat) : '[Unknown]';
            });
            modelObject.displayEnd = ko.computed(function () {
                var end = new Date(modelObject.endPeriod());
                return (end && moment(end).isValid()) ? moment(end).format(config.localDateformat) : '[Unknown]';
            });
            modelObject.canRemove(modelObject.recordStatus() == "Active");
        }

        //Staff Hours model
        function staffHours() {
            this.displayDay = ko.observable();
            this.DayHoursRequired = ko.observable();
        }
        function staffHoursInitializer(modelObject) {
            modelObject.displayDay = ko.computed(function () {
                return convertDayIdToDay(modelObject.dayId());
            });
            modelObject.displayStartTime = ko.computed(function () {
                return moment(modelObject.dayTimeStart()).isValid() ? moment(modelObject.dayTimeStart()).format("HH:mm") : '[Unknown]';
            });
            modelObject.displayEndTime = ko.computed(function () {
                return moment(modelObject.dayTimeEnd()).isValid() ? moment(modelObject.dayTimeEnd()).format("HH:mm") : '[Unknown]';
            });
            modelObject.canEdit = ko.computed(function () {
                return (modelObject.recordStatus() == "Active");
            });

        }

        function convertDayIdToDay(id) {
            switch (id) {
                case 0:
                    return "Sunday";
                case 1:
                    return "Monday";
                case 2:
                    return "Tuesday";
                case 3:
                    return "Wednesday";
                case 4:
                    return "Thursday";
                case 5:
                    return "Friday";
                case 6:
                    return "Saturday";
                default:
                    return "Undefined";
            }
        }

        function buildLeaveDays(leaveStart, leaveEnd) {
            return helper.calculateLeaveDays(leaveStart, leaveEnd);
        }

        function buildLeaveHours(leaveStart, leaveEnd) {
            return helper.calculateLeaveHours(leaveStart, leaveEnd);
        }

        // Internal Methods
        function configureMetadataStore(metadataStore) {
            metadataStore.registerEntityTypeCtor(entityNames.announcementName, announcement, announcementInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.branchName, branch, branchInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.branchDivisionName, division, divisionInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.staffMemberName, staff, staffInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.suggestionName, suggestion, suggestionInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.clockDataName, clockData, clockDataInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.thoughtName, thought, thoughtInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.linkCategory, category, categoryInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.linkName, link, linkInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.StaffHoursModelName, staffHours, staffHoursInitializer);

            //printer/toner
            metadataStore.registerEntityTypeCtor(entityNames.printerPropertiesPrinterName, printerPropertiesPrinter, printerPropertiesPrinterInitializer);

            //staff suggestion votes
            metadataStore.registerEntityTypeCtor(entityNames.staffSuggestionVotesName, suggestionVotes, suggestionVotesInitializer);

            //Staff leave application initialiser
            metadataStore.registerEntityTypeCtor(entityNames.staffLeaveName, staffLeave, staffLeaveInitializer);
            metadataStore.registerEntityTypeCtor(entityNames.staffLeaveCounterName, staffLeaveCounter, staffLeaveCounterInitializer);

        }
        //#endregion

        //#region Public methods
        var model = {
            configureMetadataStore: configureMetadataStore,
            entityNames: entityNames
        };
        return model;
        //#endregion
    });