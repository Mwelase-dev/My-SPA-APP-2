using System;
using System.Linq;

using Breeze.WebApi.NH;
using Intranet.Data.NH;
using Intranet.Models;

namespace Intranet.UI.Controllers
{
    public class IntranetNhContext : NHContext
    {
        public IntranetNhContext() : base(DataContextNH.OpenSession(), DataContextNH.Configuration) {}
        public IntranetNhContext Context
        {
            get { return this; }
        }
        
        #region DB Sets
        public NhQueryableInclude<BranchModel>        Branches          { get { return GetQuery<BranchModel>();        }}
        public NhQueryableInclude<DivisionModel>      BranchDivisions   { get { return GetQuery<DivisionModel>();      }}
        public NhQueryableInclude<HolidayModel>       Holidays          { get { return GetQuery<HolidayModel>();       }}
        public NhQueryableInclude<MenuModel>          Menus             { get { return GetQuery<MenuModel>();          }}
        public NhQueryableInclude<AnnouncementModel>  Announcements     { get { return GetQuery<AnnouncementModel>();  }}
        public NhQueryableInclude<ThoughtModel>       Thoughts          { get { return GetQuery<ThoughtModel>();       }}
        public NhQueryableInclude<LinkCategoryModel>  LinkCategories    { get { return GetQuery<LinkCategoryModel>();  }}
        public NhQueryableInclude<LinkModel>          Links             { get { return GetQuery<LinkModel>();          }}
        public NhQueryableInclude<StaffModel>         Staff             { get { return GetQuery<StaffModel>();         }}
        public NhQueryableInclude<StaffClockModel>    StaffClockData    { get { return GetQuery<StaffClockModel>();    }}
        public NhQueryableInclude<StaffHoursModel>    StaffHourData     { get { return GetQuery<StaffHoursModel>();    }}
        public NhQueryableInclude<StaffLeaveModel>    StaffLeaveData    { get { return GetQuery<StaffLeaveModel>();    }}
        public NhQueryableInclude<StaffContactModel>  StaffContactData  { get { return GetQuery<StaffContactModel>();  }}
        public NhQueryableInclude<SuggestionModel>    Suggestions       { get { return GetQuery<SuggestionModel>();    }}
        #endregion

        public bool IsHolidayToday()
        {
            // Check if today is a holiday?
            var isHoliday = Holidays.Where(x => x.HolidayDate == DateTime.Today).FirstOrDefault();
            return isHoliday != null;
        }
    }
}
