//#define NHibernate

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Breeze.WebApi;
using Intranet.Models;

namespace Intranet.UI.Controllers
{
#if NHibernate
    public class ContextProvider : IntranetNhContext
#else
    public class ContextProvider : IntranetEfContext
#endif
    {
        // Override here for validation etc.
        protected override Dictionary<Type, List<Breeze.WebApi.EntityInfo>> BeforeSaveEntities(
            Dictionary<Type, List<Breeze.WebApi.EntityInfo>> saveMap)
        {

            return base.BeforeSaveEntities(saveMap);
        }

        protected override bool BeforeSaveEntity(Breeze.WebApi.EntityInfo entityInfo)
        {
            if (entityInfo.Entity is AnnouncementModel)
            {
                var announcementModel = (AnnouncementModel)entityInfo.Entity;
                if (announcementModel != null)
                {
                    announcementModel.AnnouncementDate = DateTime.Now;
                }
            }
            if (entityInfo.Entity is StaffModel)
            {
                var staffModel = (StaffModel)entityInfo.Entity;
                if (staffModel != null)
                {
                    staffModel = BreezeDataController.ValidateStaffModel(staffModel);
                }
            }
            else if (entityInfo.Entity is StaffLeaveModel)
            {
                var leave = (StaffLeaveModel)entityInfo.Entity;
                if (leave != null)
                {
                    leave = BreezeDataController.ValidateLeaveApplication(leave);
                }
            }
            else if (entityInfo.Entity is StaffLeaveCounterModel)
            {
                StaffLeaveCounterModel leaveCounter = (StaffLeaveCounterModel)entityInfo.Entity;
                if (leaveCounter != null)
                {
                    leaveCounter = BreezeDataController.ValidateLeaveCounterModel(leaveCounter);
                }

            }

            //else if (entityInfo.Entity is StaffClockModel)
            //{
            //    StaffClockModel clockData = (StaffClockModel)entityInfo.Entity;
            //    if (clockData != null)
            //    {
            //        clockData = BreezeDataController.ValidateClockModel(clockData);
            //    }

            //}

            return base.BeforeSaveEntity(entityInfo);
        }

        protected override void AfterSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap, List<KeyMapping> keyMappings)
        {
            foreach (KeyValuePair<Type, List<EntityInfo>> valuePair in saveMap)
            {
                if (valuePair.Value[0].Entity is StaffModel)
                {
                    var savedModel = (StaffModel)valuePair.Value[0].Entity;
                    if (valuePair.Value[0].EntityState == EntityState.Added)
                    {
                 
                        if (savedModel != null)
                        {
                            var result = BreezeDataController.RegisterUserToClockingDevice(savedModel);
                        }    
                    }
                    else if (valuePair.Value[0].OriginalValuesMap.Any(x => x.Key.Equals("StaffClockCardNumber") || x.Key.Equals("ClockDevice")))
                    {
                        var result = BreezeDataController.RegisterUserToClockingDevice(savedModel);
                    }
                }
            }
        }
    }
}