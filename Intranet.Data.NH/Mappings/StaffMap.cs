using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class StaffBriefMap : ClassMap<StaffModel>
    {
        public StaffBriefMap()
        {
            Table("tblStaffMembersBrief");

            Id(x => x.StaffId        , "StaffID"          );
            Map(x => x.DivisionId    , "StaffDivisionID"  );
            Map(x => x.StaffName     , "StaffName"        );
            Map(x => x.StaffSurname  , "StaffSurname"     );
            Map(x => x.StaffIdNumber , "StaffIDNumber"    );
            Map(x => x.StaffEmail    , "StaffEmailAddress");
            Map(x => x.StaffTellExt  , "StaffTelExt"      );
            Map(x => x.StaffTelDirect, "StaffTelDirect"   );
            Map(x => x.StaffCellPhone, "StaffCellphone"   );
            Map(x => x.StaffFaxNumber, "StaffFaxNumber"   );
            Map(x => x.StaffNtName   , "StaffNTName"      );
            Map(x => x.RecordStatus  , "RecordStatus"     );
           
            //Leave Data
            //Map(x => x.StaffJoinDate        , "StaffJoinDate"        );
            //Map(x => x.StaffLeaveAllowed    , "StaffLeaveAllowed"    );
            //Map(x => x.StaffSickLeaveAllowed, "StaffSickLeaveAllowed");
            //Map(x => x.StaffLeaveIncrement  , "StaffLeaveIncrement"  );
            
            //Clocking Data
            Map(x => x.StaffIsClockingMember, "StaffIsClockingMember");
            Map(x => x.StaffClockId         , "StaffClockID"         ).Generated.Insert();
            Map(x => x.StaffClockReminders  , "StaffClockReminders"  );

            //HR Data
            Map(x => x.StaffManager1Id      , "StaffManager1"        );
            Map(x => x.StaffManager2Id      , "StaffManager2"        );

            //Phone Data
            //Map(x => x.StaffPhoneStatus     , "StaffPhoneStatus"     );
            Map(x => x.StaffPhoneIp         , "StaffPhoneIP"         );
            Map(x => x.StaffPhonePass       , "StaffPhonePass"       );
            Map(x => x.StaffPhoneMac        , "StaffPhoneMac"        );
            
            //Parent Relationships
            HasOne(x => x.StaffDivision);
            HasOne(x => x.StaffManager1); //.ForeignKey("StaffManager1"); //References(x => x.StaffManager2).Column("StaffManager1").ForeignKey("StaffManager1").Nullable();
            HasOne(x => x.StaffManager2); //References(x => x.StaffManager2).Column("StaffManager2").Nullable();

            //Child relationships
            HasMany(x => x.StaffClockData  ).KeyColumn("StaffID"); //.Cascade.SaveUpdate().Not.LazyLoad();
            HasMany(x => x.StaffHoursData  ).KeyColumn("StaffID"); //.Cascade.SaveUpdate().Not.LazyLoad();
            HasMany(x => x.StaffLeaveData  ).KeyColumn("StaffID"); //.Cascade.SaveUpdate().Not.LazyLoad();
            HasMany(x => x.StaffContactData).KeyColumn("StaffID"); //.Cascade.SaveUpdate().Not.LazyLoad();
            HasMany(x => x.Suggestions     ).KeyColumn("StaffID"); //.Cascade.SaveUpdate().Not.LazyLoad();
        }
    }
    
    public class StaffClockMap : ClassMap<StaffClockModel>
    {
        public StaffClockMap()
        {
            Table("tblStaffMembersClockData");

            CompositeId()
                .KeyProperty(x => x.StaffId      , "StaffClockID" )
                .KeyProperty(x => x.ClockDateTime, "ClockDateTime");
            Map(x => x.RecordStatus , "RecordStatus" );
            Map(x => x.ClockDateTime, "ClockDateTime");
        }
    }
    public class StaffHoursMap : ClassMap<StaffHoursModel>
    {
        public StaffHoursMap()
        {
            Table("tblStaffMembersHours");
            CompositeId()
                .KeyProperty(x => x.StaffId, "StaffID"       )
                .KeyProperty(x => x.DayId  , "DayID"         );
            Map(x => x.DayTimeStart        , "DayTimeStart"  );
            Map(x => x.DayTimeEnd          , "DayTimeEnd"    );
            Map(x => x.DayLunchLength      , "DayLunchLength");
            Map(x => x.RecordStatus        , "RecordStatus"  );
        }
    }
    public class StaffLeaveMap : ClassMap<StaffLeaveModel>
    {
        public StaffLeaveMap()
        {
            Table("tblStaffMembersLeaveApps");
            Id(x => x.LeaveId          , "LeaveID"         );
            //Map(x => x.StaffId         , "StaffID"         );
            Map(x => x.LeaveDateStart  , "LeaveDateStart"  );
            Map(x => x.LeaveDateEnd    , "LeaveDateEnd"    );
            Map(x => x.LeaveComments   , "LeaveComments"   );
            Map(x => x.LeaveType       , "LeaveType"       );
            Map(x => x.LeaveStatus     , "LeaveStatus"     );
            Map(x => x.LeaveRequestDate, "LeaveRequestDate");
            Map(x => x.RecordStatus    , "RecordStatus"    );

            //Child Relationships
            //Map(x => x.ApprovedBy1Id   ,          "LeaveApprovedBy1");
            //Map(x => x.ApprovedBy2Id   ,          "LeaveApprovedBy2");
            //References(x => x.ApprovedBy1).Column("LeaveApprovedBy1").Nullable();
            //References(x => x.ApprovedBy2).Column("LeaveApprovedBy2").Nullable();
        }
    }
    public class StaffContactMap : ClassMap<StaffContactModel>
    {
        public StaffContactMap()
        {
            Table("tblStaffMembersContacts");
            Id(x => x.ContactId          , "ContactID"         );
            Map(x => x.StaffId           , "StaffID"           );
            Map(x => x.ContactName       , "ContactName"       );
            Map(x => x.ContactSurname    , "ContactSurname"    );
            Map(x => x.ContactNumber     , "ContactNumber"     );
            Map(x => x.ContactDescription, "ContactDescription");
            Map(x => x.RecordStatus      , "RecordStatus"      );
        }
    }
}