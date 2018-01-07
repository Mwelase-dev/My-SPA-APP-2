//Comment Added by Quentin
define(['services/logger', 'durandal/system', 'services/model', 'config', 'services/entitymapper', 'durandal/app'],
    function (logger, system, model, config, entityMapper, app) {
        var entityQuery = breeze.EntityQuery;
        var manager = configureBreezeManager();

        //#region Helpers
        function configureBreezeManager() {
            breeze.NamingConvention.camelCase.setAsDefault();
            //#region For NHibernate (No Metadata)
            /*
            var dataService = new breeze.DataService({
                serviceName: config.remoteServiceName,
                hasServerMetadata: false
            });
            var ms = new breeze.MetadataStore();

            var mgr = new breeze.EntityManager({
                dataService: dataService,
                metadataStore: ms
            });
            */
            //#endregion
            //#region For Entity Framework

            var mgr = new breeze.EntityManager(config.remoteServiceName);
            //#endregion
            model.configureMetadataStore(mgr.metadataStore);
            return mgr;
        }
        function queryFailed(error) {
            var msg = 'Error getting data. ' + error.message;
            logger.logError(msg, error, system.getModuleId(datacontext), true);
        }
        function log(msg, data, showToast) {
            logger.log(msg, data, system.getModuleId(datacontext), showToast);
        }
        function formatLog(dataArray, section) {
            log('Retrieved ' + dataArray.results.length + ' ' + section + ' item(s)', dataArray, true);
        }
        function executeQuery(query, onSuccess) {
            return manager.executeQuery(query)
                .then(onSuccess)
                .fail(queryFailed);
        }
        function changesSave() {
            return manager.saveChanges().then(saveSucceeded).fail(saveFailed);
        }
        function changesCancel() {
            if (manager.hasChanges()) {
                manager.rejectChanges();
                log('Changes cancelled', null, true);
            }
        }
        function saveSucceeded(saveResult) {
            log('Saved data successfully', saveResult, true);
        }
        function saveFailed(error) {
            var msg = 'Save failed: ' + error.message;
            logger.logError(msg, error, system.getModuleId(datacontext), true);
            error.message = msg;
            throw error;
        }
        function manufactureEntity(entityTypeName, inititalValues, generateKey) {

            var metadataStore = manager.metadataStore;
            var entityType = metadataStore.getEntityType(entityTypeName);
            var newEntity = entityType.createEntity(inititalValues);

            var generateEnityKey = typeof generateKey !== 'undefined' ? generateKey : true;
            if (generateEnityKey)
                manager.generateTempKeyValue(newEntity);

            manager.addEntity(newEntity);
            return newEntity;
        }
        function promptAndProceed(status, object, typeName) {
            var msg = typeName ? typeName : 'item';
            return app.showMessage('Do you want to ' + status + ' this ' + msg + '?', status + ' ' + msg, ['No', 'Yes']).then(function (result) {
                if (result === "Yes") {
                    object.entityAspect.setDeleted();
                    object.recordStatus(status);
                    if (object.hasOwnProperty('isVisible')) {
                        object.isVisible(false);
                    }
                    object.entityAspect.setModified();
                    return changesSave();
                }
                return changesCancel();
            });
        }
        //#endregion
        //==================================================================>

        //==================================================================>
        //#region Data Access Methods

        //#region Menus
        var getMenuList = function (routesArray, forceRemote) {
            var query = entityQuery.from('Menus');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.menuName);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                if (routesArray) {
                    for (var i = 0; i < data.results.length; i++) {
                        var t = {
                            url: data.results[i].menuTemplate(),
                            name: data.results[i].menuName(),
                            muduleId: 'viewmodels/' + data.results[i].menuTemplate(),
                            visible: true
                        };
                        routesArray.push(t);
                    }

                    //TODO: Need better names here!
                    var q = {
                        url: 'view_suggestion/:id',
                        name: 'view_suggestion',
                        muduleId: 'viewmodels/view_suggestion',
                        visible: false
                    };
                    routesArray.push(q);

                    var s = {
                        url: 'view_staffclockgraph',
                        name: 'view_staffclockgraph',
                        muduleId: 'viewmodels/view_staffclockgraph',
                        visible: false
                    };
                    routesArray.push(s);

                    var p = {
                        url: 'view_galleryimages/:name',
                        name: 'view_galleryimages',
                        muduleId: 'viewmodels/view_galleryimages',
                        visible: false
                    };
                    routesArray.push(p);

                    var timekeeping = {
                        url: 'view_timekeeping_sheet',
                        name: 'view_timekeeping_sheet',
                        muduleId: 'viewmodels/view_stafftimekeeping_sheet',
                        visible: false
                    };
                    routesArray.push(timekeeping);

                    var approveClockRecord = {
                        url: 'view_approveclockrecord',
                        name: 'view_approveclockrecord',
                        muduleId: 'viewmodels/view_approveclockrecord',
                        visible: false
                    };
                    routesArray.push(approveClockRecord);

                    var webAccess = {
                        url: 'view_requestwebaccess',
                        name: 'view_requestwebaccess',
                        muduleId: 'view_requestwebaccess',
                        visible: false,
                    };
                    routesArray.push(webAccess);

                    var contacts = {
                        url: 'view_phonecontacts',
                        name: 'view_phonecontacts',
                        muduleId: 'viewmodels/view_phonecontacts',
                        visible: false,
                    };
                    routesArray.push(contacts);

                    var addContact = {
                        url: 'view_contactadd',
                        name: 'view_contactadd',
                        muduleId: 'viewmodels/view_contactadd',
                        visible: false,
                    };
                    routesArray.push(addContact);

                    var phoneusage = {
                        url: 'view_phoneusage',
                        name: 'view_phoneusage',
                        muduleId: 'viewmodels/view_phoneusage',
                        visible: false,
                    };
                    routesArray.push(phoneusage);

                    var companyphoneusage = {
                        url: 'view_companyphoneusage',
                        name: 'view_companyphoneusage',
                        muduleId: 'viewmodels/view_companyphoneusage',
                        visible: false,
                    };
                    routesArray.push(companyphoneusage);

                    var staffCDRSummary =
                    {
                        url: 'view_staffcdrsummary',
                        name: 'view_staffcdrsummary',
                        muduleId: 'viewmodels/view_staffcdrsummary',
                        visible: false
                    };
                    routesArray.push(staffCDRSummary);

                    var toner = {
                        url: 'view_ordertoner',
                        name: 'view_ordertoner',
                        muduleId: 'viewmodels/view_ordertoner',
                        visible: false
                    };
                    routesArray.push(toner);

                    var tonerOrders = {
                        url: 'view_tonerorders',
                        name: 'view_tonerorders',
                        muduleId: 'viewmodels/view_tonerorders',
                        visible: false
                    };
                    routesArray.push(tonerOrders);

                    var r = {
                        url: 'view_thoughts',
                        name: 'view_thoughts',
                        muduleId: 'viewmodels/view_thoughts',
                        visible: false
                    };
                    routesArray.push(r);

                    var la = {
                        url: 'view_leaveapplication',
                        name: 'view_leaveapplication',
                        muduleId: 'viewmodels/view_leaveapplication',
                        visible: false
                    };
                    routesArray.push(la);

                    var mla = {
                        url: 'view_leaveapplications',
                        name: 'view_leaveapplications',
                        muduleId: 'viewmodels/view_leaveapplications',
                        visible: false
                    };
                    routesArray.push(mla);

                    var staffInfo = {
                        url: 'view_staffinfo',
                        name: 'view_staffinfo',
                        moduleId: 'viewmodels/view_staffinfo',
                        visible: false
                    };
                    routesArray.push(staffInfo);

                    var staffInfo = {
                        url: 'view_graphs',
                        name: 'view_graphs',
                        moduleId: 'viewmodels/view_graphs',
                        visible: false
                    };
                    routesArray.push(staffInfo);
                }
                formatLog(data, 'menu');
                return;
            }
        };
        //#endregion
        //#region Announcements
        var getAnnouncements = function (dataArray, status, forceRemote) {
            var query = entityQuery.from('announcements').where('recordStatus', 'equals', status);
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.announcementName);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                //var list = entityMapper.mapDtosToEntities(manager, data.results, model.entityNames.announcementName, 'announcementId');
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'announcement');
            }
        };
        //#endregion
        //==================================================================>

        //==================================================================>
        //#region Other
        var getBranches = function (dataArray, forceRemote) {
            var query = entityQuery.from('BranchList').where('recordStatus', 'equals', "Active");
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.branch);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                    dataArray.sort();
                }
                formatLog(data, 'branches');
            }
        };
        getBranchlookup = function (dataArray) {
            var query = entityQuery.from('BranchLookUp');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
                formatLog(data, 'BranchLookUp');

                return;
            }
        };

        getLinkCategories = function (dataArray) {
            //var catLinks = manager.getEntities('LinkModel'); // all catLinks in cache
            //catLinks.forEach(function (entity) { manager.detachEntity(entity); });

            var query = entityQuery.from('LinkCategories');

            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.linkCategory);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'LinkCategories');
            }
        };

        ///////////////////////////////////


        ///////////////////////////////////


        /////
        //getLinks = function (dataArray, active) {
        //    var query = entityQuery.from('Links')
        //    .where('recordStatus', '==', active)
        //        .orderBy('linkDesc');

        //    //if (query != null) {
        //    //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.links);
        //    //}
        //    return executeQuery(query, querySucceeded);

        //    function querySucceeded(data) {
        //        //var list = entityMapper.mapDtosToEntities(manager, data.results, model.entityNames.linkCategory, 'categoryId');
        //        var list = data.results;
        //        if (dataArray) {
        //            dataArray(list);
        //        }
        //        formatLog(data, 'linkcategories');
        //    }
        //};
        ////


        getGalleries = function (dataArray, forceRemote) {
            var query = entityQuery.from('GalleryList');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.branch);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'galleries');
            }
        };
        getGalleryImages = function (dataArray, galleryName, forceRemote) {
            var query = entityQuery.from('GetGalleryImageList').withParameters({ galleryName: galleryName });
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.branch);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'gallery images');
            }
        };
        getDivisions = function (dataArray, branchId) {
            var query;
            //todo jay: try to see if i can query the in memory data
            if (branchId != null) {
                query = entityQuery.from('DivisionList')
                    .where('branchId', '==', branchId);
            } else {
                query = entityQuery.from('DivisionList')
                    .where('brnachId', '==', branchId);
            }

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };
        getDivisionStaff = function (dataArray, divisionsId) {
            var query = entityQuery.from('Staff')
                .where('divisionId', '==', divisionsId);

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };
        //#endregion
        //==================================================================>

        //==================================================================>
        //#region Staff
        getIsAdmin = function (variable, forceRemote) {
            var query = entityQuery.from('CurrentUserIsAdmin');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results[0];
                if (variable) {
                    variable(list);
                }
                formatLog(data, 'admin rights');
            }
        };
        getBirthdays = function (dataArray, forceRemote) {
            var query = entityQuery.from('staffBirthdays');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.staffBriefName);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'birthdays');
            }
        };
        var getSuggestions = function (dataArray, forceRemote) {
            var query = entityQuery.from('StaffSuggestions')
                .expand("Votes,SuggestionFollowers");
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.staffSuggestions);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataArray) {
                    dataArray(list);
                }
                formatLog(data, 'suggestions');
            }
        };

        getSuggestion = function (keyValue) {
            return manager.getEntityByKey(model.entityNames.suggestionName, keyValue);
        };
        var getStaffSuggestionFollowers = function (dataHolder) {
            var query = entityQuery.from("StaffSuggestionFollowers");
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                var list = data.results;
                if (dataHolder) {
                    dataHolder(list);
                }
            }
        };

        //second parameter is used to filter the staff by managers
        //it also includes your logged in person
        getAllStaff = function (dataArray, staffMember) {
            var query = entityQuery.from('Staff')
                .expand("LeaveCounters,staffLeaveData")
                .orderBy("staffName");


            if (staffMember != null) {
                var pred = breeze.Predicate.create("staffManager1Id", "==", staffMember().staffId())
                    .or("staffId", "==", staffMember().staffId());
                // var pred = Predicate.create("staffManager1Id", "==", staffMember().staffManager1Id())
                //      .or("staffId", "==", staffMember().staffId());
                query = query.where(pred);
                //query = query.where(pred);
            }
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                //logger.log("Staff members have been loaded", data.results, 'datacontext', true);

                dataArray(data.results);
            }
        };

        staffByManager = function (dataArray, staffMember) {
            var query = entityQuery.from('StaffByManager')
                .orderBy("staffName");

            if (staffMember != null) {
                var pred = breeze.Predicate.create("staffManager1Id", "==", staffMember().staffId())
                    .or("staffId", "==", staffMember().staffId());
                query = query.where(pred);
            }
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };

        getDivisionLookUp = function (dataArray) {
            var query = entityQuery.from('DivisionLookUp');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
                formatLog(data, 'branche look up');

                return;
            }
        };

        getCurrentUser1 = function (dataArray) {
            var query = entityQuery.from("CurrentStaff")
                .expand("StaffHoursData");

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results[0]);
            }
        };

        getCurrentUser = function (dataArray) {
            var query = entityQuery.from("CurrentStaff");

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results[0]);
            }
        };
        getStaffbyId = function (dataArray, staffId) {
            var query = entityQuery.from('Staff')
                .where("staffId", "==", staffId)
                .expand("PhoneDetails,StaffLeaveData,LeaveCounters,StaffHoursData");

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results[0]);
            }
        };
        getStaffDayWorkHours = function (staffId, dayId) {

            return manager.getEntityByKey(model.entityNames.StaffHoursModelName, [staffId, dayId]);
        };
        //#endregion
        //==================================================================>

        //==================================================================>
        //#region clocking data
        getGraphData = function (dataArray, staffId, startD, endD) {
            var query = entityQuery.from('GetGraphData')
                .withParameters({ id: staffId, startDate: startD, endDate: endD });
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
                formatLog(data, 'graph data has been read from the DB');
                return true;
            }
        };

        getTimeKeepingData = function (dataArray, staffId, start, end) {
            var query = entityQuery.from('TimeKeepingData')
                .withParameters({ id: staffId, startDate: start, endDate: end });
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
                logger.log("clocking data successfuly read", dataArray, 'datacontext', true);
            }
        };
        getClockRecord = function (clockRecord, clockDataId) {
            var query = entityQuery.from('StaffClockModelData')
                .where('clockDataId', '==', clockDataId);
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                //todo - jay: do some validation to make sure only one record is read from the database
                var res = data.results[0];
                clockRecord(res);
            }
        };
        getUnApprovedClockRecords = function (clockRecords, staffId) {
            clockRecords.removeAll();

            var query = entityQuery.from('StaffClockModel')
                .where('staffId', '==', staffId)
                .orderBy('staffId');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                clockRecords(data.results);
            }
        };

        getUnApprovedClockRecordsAll = function (allClockRecords) {
            allClockRecords.removeAll();

            var query = entityQuery.from('StaffClockModelAll');
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                allClockRecords(data.results);
            }
        };

        emailClockRecDenied = function (clockId, results) {
            var query = entityQuery.from("EmailClockRecDenied")
                .withParameters({ id: clockId });

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                results(data.results[0]);
                if (results) {
                    results(data.results[0]);
                }
            }
        };

        var getStaffUnApprovedRecords = function (dataHolder, staffId, clockRecordId) {
            var query = entityQuery.from('StaffClockModel')
                .where('staffId', '==', staffId)
                .where('clockDataId', '!=', clockRecordId)
                .orderBy('staffId');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataHolder(data.results);
            }
        };

        //#endregion
        //==================================================================>

        //#region Contacts
        getPhoneContacts = function (dataArray, staffId) {
            var query = entityQuery.from("StaffContacts")
                .where("RecordStatus", '==', recStatus.statusActive)
                .where("staffMember.staffId", '==', staffId)
                .expand("StaffMember");

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };
        getPhoneContact = function (data, contactId) {
            var query = entityQuery.from("StaffContacts")
                .where("ContactId", "==", contactId);

            return executeQuery(query, querySucceeded);

            function querySucceeded(result) {
                data(result.results[0]);
            }
        };
        dialPhoneContact = function (contactId) {
            jQuery.support.cors = true;
            $.ajax({
                url: 'api/breezedata/DialContact?phoneNumber=' + contactId,
                type: 'Get',
                contentType: "application/json;charset=utf-8",
                success: function (data) {
                    return;
                },
                error: function () {
                    alert("faile to request access");
                }
            });
        };

        //#endregion
        //==================================================================>

        //#region Thoughts
        function getRandomThought(modelData, forceRemote) {
            var query = entityQuery.from('thoughtRandom');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.thoughtName);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                //var list = entityMapper.mapDtosToEntities(manager, data.results, model.entityNames.thoughtName, 'thoughtId');
                var list = data.results;
                if (modelData) {
                    modelData(list[0]);
                }
                formatLog(data, 'random thought');
            }
        }

        function getThoughtList(dataList, forceRemote) {
            var query = entityQuery.from('thoughtList');
            //if (!forceRemote) {
            //    query = query.using(breeze.FetchStrategy.FromLocalCache).toType(model.entityNames.thoughtName);
            //}
            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                //var list = entityMapper.mapDtosToEntities(manager, data.results, model.entityNames.thoughtName, 'thoughtId');
                var list = data.results;
                if (dataList) {
                    dataList(list);
                }
                formatLog(data, 'thoughts');
            }
        }

        //#endregion
        //==================================================================>

        //#region Request Web access
        function requestWebAccess(webaccessrequest) {
            jQuery.support.cors = true;
            $.ajax({
                url: 'api/breezedata/RequestWebAccess',
                type: 'POST',
                data: JSON.stringify(webaccessrequest),
                contentType: "application/json;charset=utf-8",
                success: function (data) {
                },
                error: function () {
                    alert("faile to request access");
                }
            });
        }

        //#endregion
        //==================================================================>

        //#region Phone usage
        getStaffCDR = function (dataArray, startDate, endDate) {
            var query = entityQuery.from('GetStaffPhoneRecords')
                .withParameters({ startDate: new Date(startDate).toDateString(), endDate: new Date(endDate).toDateString() });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {

                dataArray(data.results);

            }

        };
        getCompanyCDR = function (dataArray, startDate, endDate, companyId, divisionId, staffId, carrier) {

            var query = entityQuery.from('GetCompanyCDR')
                .withParameters({
                    startDate: new Date(startDate).toDateString(),
                    endDate: new Date(endDate).toDateString(),
                    companyId: companyId,
                    divisionId: divisionId,
                    staffId: staffId,
                    carrier: carrier
                });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {

                // clearInvolvedEntities();

                dataArray(data.results);
            }

        };
        getStaffCDRSummary = function (dataArray, startDate, endDate, extention) {
            var query = entityQuery.from('GetStaffCDRSummary')
                .withParameters({ startDate: new Date(startDate).toDateString(), endDate: new Date(endDate).toDateString(), extension: extention });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }

        };
        ///this is used in the cdr pop up
        GetCDRSummary = function (dataArray, startDate, endDate, extension, destination) {
            var query = entityQuery.from('GetCDRSummary')
                .withParameters({ startDate: new Date(startDate).toDateString(), endDate: new Date(endDate).toDateString(), extension: extension, destination: destination });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };
        //#endregion
        //==================================================================>

        //#region Printer/ Toner order
        getPrinters = function (dataArray) {
            var query = entityQuery.from('Printers')
                .orderBy('serialNumber');

            //PrinterDescription

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };

        getPrinterProperties = function (dataArray, printerId) {

            var query = entityQuery.from('GetPrinterProperties')
                .withParameters({ printerId: printerId });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }

        };

        getPrinter = function (dataArray, printerId) {

            var query = entityQuery.from('Printers')
                .where('printerId', '==', printerId);

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results[0]);
            }
        };

        getPrinterServiceProviders = function (dataArray) {
            var query = entityQuery.from('PrinterServiceProviders');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };

        getCurrentOrders = function (dataArray) {
            var query = entityQuery.from('GetCurrentOrders');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };

        getTonerOrderDetail = function (dataArray, odId) {
            var query = entityQuery.from('OrderDetailsModels')
                .where('DetailsId', '==', odId);

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results[0]);
            }

        };

        orderToner = function (orders) {
            jQuery.support.cors = true;
            $.ajax({
                url: 'api/breezedata/OrderTonerEmail',
                type: 'POST',
                data: JSON.stringify(orders),
                contentType: "application/json;charset=utf-8",
                success: function (data) {
                },
                error: function () {
                    alert("faile to request access");
                    alert("failed to request access");
                }
            });
        }
        //#endregion

        //Post: Staff members notified
        var suggestionSubscription = function (followers) {
            var suggestionData = JSON.stringify(followers);
            jQuery.support.cors = true;
            $.ajax({
                type: "POST",
                url: "api/breezedata/SuggestionSubscription1",
                data: suggestionData,
                traditional: true,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {

                    return data;
                },
                error: function (error) {

                }
            });
        };


        //#region UserRoles
        //getUserRoles = function (dataArray, staffId) {
        //    var query = entityQuery.from('UserRoles')
        //        .expand("role")
        //        .where('staffId', '==', staffId);
        //
        //    return executeQuery(query, querySucceeded);
        //
        //    function querySucceeded(data) {
        //        dataArray(data.results);
        //    };
        //};

        UserInRoles = function (authorised, roleEnum) {
            var query = entityQuery.from('CurrentUserInRole')
                .withParameters({ roleId: roleEnum });

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                authorised(data.results[0]);
            };
        };

        ///get all the current users roles
        ///returns a list/array or user roles
        getCurrentUserRoles = function (dataArray) {
            var query = entityQuery.from('CurrentUserRoles');

            return executeQuery(query, querySucceded);

            function querySucceded(data) {
                dataArray(data.results.length > 0 ? data.results : []);
            }
        };

        //this is more like a helper method
        isInRole = function (userRoles, role) {
            var valid = $.inArray(role, userRoles) > -1;

            return valid;
        };
        //#endregion

        //Staff leave applications
        leaveTypes = function (dataArray) {
            var query = entityQuery.from('Leavetypes');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };

        getStaffLeaveApplications = function (dataArray, staffId) {
            var query = entityQuery.from("StaffLeave")
                .where("staffId", "==", staffId).expand("StaffMember");

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                dataArray(data.results);
            }

        };

        getStaffLeaveApplicationsByStatus = function (dataArray, staffId, status, manag) {
            var query = entityQuery.from("StaffLeave").expand("StaffMember");
            if (typeof status !== "undefined") {
                var statusPred = breeze.Predicate.create("leaveStatus", '==', status);
                query = query.where(statusPred);
            }
            if (typeof staffId !== "undefined") {
                var staffPred = breeze.Predicate.create("staffId", '==', staffId);
                query = query.where(staffPred);
            }
            if (manag != null) {
                var pred = breeze.Predicate.create("StaffMember.StaffManager1Id", "==", manag).or("StaffMember.StaffManager2Id", "==", manag);
                query = query.where(pred);
            }
            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                dataArray(data.results);
            }
        };

        getSimultaniousLeaveApps = function (dataArray, leaveId) {
            var query = entityQuery.from("StaffSimulLeaveApps")
                .withParameters({ leaveId: leaveId });

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                var t = data.results;

                dataArray(t);
            }
        };

        getLeaveStatuses = function (dataArray) {
            var query = entityQuery.from('LeaveStatuses');

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                dataArray(data.results);
            }
        };

        var getStaffLeaveSummary1 = function (dataArray, staffId) {
            var query = entityQuery.from("StaffLeave1")
                .withParameters({ staffId: staffId });

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                dataArray(data.results);
            }
        };
        getStaffLeaveSummary = function (staffMember, staffId) {
            var query = entityQuery.from("Staff")
                .where("staffId", "==", staffId)
                .expand("StaffLeaveData,LeaveCounters,LeaveSummary");

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                return staffMember(data.results[0]);
            }
        };
        getCurrentUserWithLeaveData = function (staff) {
            var query = entityQuery.from("CurrentStaff")
                .expand("StaffLeaveData,LeaveCounters,StaffHoursData,LeaveSummary");

            return executeQuery(query, onSuccess);

            function onSuccess(data) {
                return staff(data.results[0]);
            }
        };


        getleavesummary = function (dataArray) {
            var query = entityQuery.from('StaffLeaveSummaryData');

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };


        getleavedata = function (dataArray) {
            var query = entityQuery.from('LeaveDataSummary')
                .expand("StaffHoursData,StaffLeaveData,LeaveSummary");

            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                dataArray(data.results);
            }
        };


        getnumberofapprovedleavedays = function (numberofapprovedleavedays, selectedStaffId) {
            var query = entityQuery.from('StaffLeaveDaysApproved')
                .where("staffId", "==", selectedStaffId)
                .expand("LeaveCounters,staffLeaveData")
                .orderBy("staffName");


            return executeQuery(query, querySucceeded);

            function querySucceeded(data) {
                numberofapprovedleavedays(data.results);
            }

        };

        //Leave messaging
        emailLeaveApplication = function (leaveId) {

            return actionLeaveEmail(leaveId, "EmailLeaveApplication", onSuccess);

            function onSuccess(data) {
                return data.results[0];
            }
        };
        emailLeaveCancelled = function (dataRes, leaveId) {
            return actionLeaveEmail(leaveId, "EmailLeaveCancelled", onSuccess);

            function onSuccess(data) {
                return data.results[0];
            }
        };
        emailLeaveApproved = function (leaveId) {
            actionLeaveEmail(leaveId, "EmailLeaveApproved", onSuccess);

            function onSuccess(data) {
                return data.results[0];
            }
        };
        emailLeaveUpdated = function (leaveId) {
            return actionLeaveEmail(leaveId, "emailLeaveUpdated", null);
        };

        function actionLeaveEmail(leaveId, apiAction, onSuccess) {
            var query = entityQuery.from(apiAction)
                .withParameters({ leaveId: leaveId });


            return executeQuery(query, onSuccess);
        }

        function exportMarkup(csvFile, staffId, start, end) {
            return $.ajax({
                type: 'GET',
                url: '/api/breezedata/ExportClockDataToExcel',/*ExportClockDataToExcel*/
                data: { id: staffId, startDate: start, endDate: end },
                traditional: true,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    return csvFile(data);

                },
                error: function (errorData) {
                    toastr.error(errorData.responseText);
                }
            });
        }

        function deleteteLeaveClockRecord(clockDataId) {
            return $.ajax({
                type: 'GET',
                url: '/api/breezedata/DeleteteLeaveClockRecord',
                data: { id: clockDataId },
                traditional: true,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    return;

                },
                error: function (errorData) {
                    toastr.error(errorData.responseText);
                }
            });
        }

        function updateClockRecord(clockRecord) {
            var dataObject = JSON.stringify(clockRecord);
            return $.ajax({
                type: 'POST',
                url: '/api/breezedata/UpdateClockRecord',/*ExportClockDataToExcel*/
                data: dataObject,
                traditional: true,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (data) {

                },
                error: function (errorData) {
                    toastr.error(errorData.responseText);
                }
            });
        }


        function getClockDataForLeaveClockRecord(clockRecord, staffId, start, end) {
            var query = entityQuery.from('TimeKeepingDataForLeaveClockRecord')
                     .withParameters({ id: staffId, startDate: start, endDate: end });
            return executeQuery(query, querySucceeded);
            function querySucceeded(data) {
                clockRecord(data.results[0].timeKeepingItems[0]);
            }
        }


        function dial(ownExt,dialingExt) {
        
            return $.ajax({
                type: 'GET',
                url: '/api/breezedata/Dial',
                data: { ownExt: ownExt, dialingExt: dialingExt },
                traditional: true,
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    return;
                },
                error: function (errorData) {
                    toastr.error(errorData.responseText);
                }
            });
        }


        

        //#region Enums
        // Object Status
        var recStatus = {
            statusActive: 'Active', // Hard coded here to match DB
            statusDelete: 'Deleted', // Hard coded here to match DB
            statusArchive: 'Archive' // Hard coded here to match DB
        };

        //Will match Enum on client
        var orderStatus =
        {
            open: 1,
            closed: 2
        };

        //this should match the backend enum
        var userRoleEnum =
        {
            humanResource: 1,
            manager: 2,
            intranetAdmins: 3
        };

        //should match backend enum
        var suggestiontype =
        {
            voteUp: 1,
            voteDown: 2
        };

        var leaveStatusEnum =
        {
            invalid: 0,
            approved: 1,
            pending: 2,
            declined: 3,
            cancelled: 4,
        };

        var leaveTypeEnum =
        {
            annual: 1,
            sick: 2,
            study: 3,
            family: 4,
            unpaid: 5,
            offsite: 6,
            special : 7
        };

        var voteTypeEnum =
        {
            up: 1,
            down: 2
        };
        //#endregion

        //-- Public exposed object with Methods --
        var datacontext = {
            /*-- Infrastructure ----------- --*/
            orderStatus: orderStatus,
            recStatus: recStatus,
            changesSave: changesSave,
            changesCancel: changesCancel,
            promptAndProceed: promptAndProceed,
            manufactureEntity: manufactureEntity,
            getMenuList: getMenuList,
            suggestiontype: suggestiontype,

            /*-- Enumarations --------------- --*/
            voteTypeEnum: voteTypeEnum,
            leaveStatusEnum: leaveStatusEnum,
            leaveTypeEnum: leaveTypeEnum,
            userRoleEnum: userRoleEnum,

            /*-- Announcements ------------ --*/
            getAnnouncements: getAnnouncements,

            /*-- Other data ---- ---------- --*/
            getBranchlookup: getBranchlookup,
            getBranches: getBranches,
            getLinkCategories: getLinkCategories,
            getGalleries: getGalleries,
            getGalleryImages: getGalleryImages,
            getDivisions: getDivisions,
            getDivisionStaff: getDivisionStaff,
            //getLinks : getLinks,

            /*-- Staff -------------------- --*/
            getAllStaff: getAllStaff,
            staffByManager : staffByManager,
            getIsAdmin: getIsAdmin,
            getBirthdays: getBirthdays,
            getSuggestions: getSuggestions,
            getSuggestion: getSuggestion,
            getCurrentUser: getCurrentUser,
            getDivisionLookUp: getDivisionLookUp,
            getCurrentUser1: getCurrentUser1,
            getStaffbyId: getStaffbyId,
            getStaffDayWorkHours: getStaffDayWorkHours,
            /*-- Clocking data --------------------------- --*/
            getGraphData: getGraphData,
            getTimeKeepingData: getTimeKeepingData,
            getUnApprovedClockRecords: getUnApprovedClockRecords,
            getClockRecord: getClockRecord,
            emailClockRecDenied: emailClockRecDenied,

            /*-- Phone contacts --------------- */
            getPhoneContacts: getPhoneContacts,
            getPhoneContact: getPhoneContact,
            dialPhoneContact: dialPhoneContact,

            /*-- Thoughts ----------------- --*/
            getRandomThought: getRandomThought,
            getThoughtList: getThoughtList,

            /*-- Request web access --------------- --*/
            requestWebAccess: requestWebAccess,

            /*-- Phone usage ----------------------- --*/
            getStaffCDR: getStaffCDR,
            getCompanyCDR: getCompanyCDR,
            getStaffCDRSummary: getStaffCDRSummary,
            GetCDRSummary: GetCDRSummary,

            /*-- Printer/Toner Order ---------------- --*/
            getPrinters: getPrinters,
            getPrinterProperties: getPrinterProperties,
            getPrinter: getPrinter,
            getPrinterServiceProviders: getPrinterServiceProviders,
            getCurrentOrders: getCurrentOrders,
            getTonerOrderDetail: getTonerOrderDetail,
            orderToner: orderToner,

            /*-- Roles ------------------------------ --*/
            //getUserRoles: getUserRoles,
            UserInRoles: UserInRoles,
            getCurrentUserRoles: getCurrentUserRoles,
            isInRole: isInRole,

            /*--Leave application --------------------------------- --*/
            leaveTypes: leaveTypes,
            emailLeaveApplication: emailLeaveApplication,
            getStaffLeaveApplications: getStaffLeaveApplications,
            getSimultaniousLeaveApps: getSimultaniousLeaveApps,
            getStaffLeaveApplicationsByStatus: getStaffLeaveApplicationsByStatus,
            getLeaveStatuses: getLeaveStatuses,
            emailLeaveCancelled: emailLeaveCancelled,
            emailLeaveApproved: emailLeaveApproved,
            emailLeaveUpdated: emailLeaveUpdated,
            getStaffLeaveSummary: getStaffLeaveSummary,
            getCurrentUserWithLeaveData: getCurrentUserWithLeaveData,
            //added by Mwelase
            getUnApprovedClockRecordsAll: getUnApprovedClockRecordsAll,
            getleavesummary: getleavesummary,
            getnumberofapprovedleavedays: getnumberofapprovedleavedays,
            getleavedata: getleavedata,
            getStaffUnApprovedRecords: getStaffUnApprovedRecords,
            suggestionSubscription: suggestionSubscription,
            getStaffSuggestionFollowers: getStaffSuggestionFollowers,
            getStaffLeaveSummary1: getStaffLeaveSummary1,
            exportMarkup: exportMarkup,
            updateClockRecord: updateClockRecord,
            getClockDataForLeaveClockRecord: getClockDataForLeaveClockRecord,
            deleteteLeaveClockRecord: deleteteLeaveClockRecord,
            dial: dial
        };
        return datacontext;
    });