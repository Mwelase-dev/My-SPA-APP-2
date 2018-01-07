using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class StaffMap : EntityTypeConfiguration<StaffModel>
    {
        public StaffMap()
        {
            ToTable("tblStaffMembers");
            HasKey(x => x.StaffId);
            Property(x => x.DivisionId).HasColumnName("StaffDivisionID");

            // Personal Info
            Property(x => x.StaffName).HasColumnName("StaffName");
            Property(x => x.StaffSurname).HasColumnName("StaffSurname");
            Property(x => x.StaffIdNumber).HasColumnName("StaffIDNumber");
            Property(x => x.StaffEmail).HasColumnName("StaffEmailAddress");
            Property(x => x.StaffTellExt).HasColumnName("StaffTelExt");
            Property(x => x.StaffTelDirect).HasColumnName("StaffTelDirect");
            Property(x => x.StaffCellPhone).HasColumnName("StaffCellphone");
            Property(x => x.StaffFaxNumber).HasColumnName("StaffFaxNumber");
            Property(x => x.StaffNtName).HasColumnName("StaffNtName");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");

            // Leave Data
            Property(x => x.StaffJoinDate).HasColumnName("StaffJoinDate");
            Property(x => x.LeaveScheduleCarriedOver).HasColumnName("LeaveDaysCarriedOver");


            // Clocking Data
            Property(z => z.StaffIsClockingMember).HasColumnName("StaffIsClockingMember");
            Property(z => z.StaffClockId).HasColumnName("StaffClockID").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(z => z.StaffClockReminders).HasColumnName("StaffClockReminders");
            Property(x => x.StaffClockCardNumber).HasColumnName("PeStaffClockId");

            // HR Data
            Property(z => z.StaffManager1Id).HasColumnName("StaffManager1");
            Property(z => z.StaffManager2Id).HasColumnName("StaffManager2");
            Property(z => z.StaffDesignation).HasColumnName("StaffDesignation");
            Property(z => z.StaffPhoneStatus).HasColumnName("StaffPhoneStatus");
            Property(x => x.IsDirector).HasColumnName("IsDirector");
            Property(x => x.BroadcastBirthday).HasColumnName("BroadcastBirthday");

            Property(x => x.ClockDevice).HasColumnName("ClockDevice");
            Property(x => x.RecieveSystemMail).HasColumnName("RecieveSystemMail");
            Property(x => x.OffsiteApproval).HasColumnName("OffsiteApproval");

            //Ignore(z=>z.ManagerFullNameStaffFullName);
            // Phone Data

            Ignore(z => z.StaffPhoneIp);
            Ignore(z => z.StaffPhonePass);
            Ignore(z => z.StaffPhoneMac);
            Ignore(z => z.ClockDataId);
            //Ignore(z => z.StaffJoinDate);

            //printer
            Property(x => x.StaffDefaultPrinter).HasColumnName("StaffPrinterDefault");

            //Staff contacts
            HasMany(m => m.StaffContactData)
                .WithRequired(m => m.StaffMember)
                .HasForeignKey(m => m.StaffId);

            //UserRoles
            HasMany(x => x.Roles);

            HasMany(x => x.TonerOrders)
                .WithRequired(x => x.Staff)
                .HasForeignKey(x => x.StaffId);

            // Ignored Members
            Ignore(x => x.StaffIsOnLeave);
            Ignore(x => x.StaffFullName);
            Ignore(x => x.StaffDob);
            Ignore(x => x.StaffBirthday);

            Ignore(x => x.StaffCallRecords);
            Ignore(x => x.TotalCallCost);
            Ignore(x => x.DisplayTotalCallCost);

            // Related Entities
            HasRequired(x => x.StaffDivision);
            HasRequired(x => x.StaffManager1).WithMany().HasForeignKey(m => m.StaffManager1Id);
            HasRequired(x => x.StaffManager2).WithMany().HasForeignKey(m => m.StaffManager2Id);

            HasMany(x => x.StaffClockData).WithRequired(m => m.Staff).HasForeignKey(m => m.StaffId);
            HasMany(x => x.StaffHoursData).WithRequired(m => m.Staff).HasForeignKey(m => m.StaffId);
            HasMany(x => x.Suggestions);

            HasMany(x => x.StaffLeaveData).WithRequired(m => m.StaffMember).HasForeignKey(m => m.StaffId);
            HasMany(x => x.LeaveCounters).WithRequired(m => m.Staff).HasForeignKey(m => m.StaffId);

            HasMany(x => x.LeaveSummary).WithRequired(m => m.StaffMember).HasForeignKey(m => m.StaffId);

            //Suggestion
            HasMany(m => m.Suggestions).WithRequired(m => m.Staff).HasForeignKey(m => m.StaffId);

            //Phone Detail map
            HasOptional(m => m.PhoneDetails);
            Ignore(x => x.PhoneDataStatus);
 

            //more ignores

        }
    }
    public class StaffClockMap : EntityTypeConfiguration<StaffClockModel>
    {
        public StaffClockMap()
        {
            ToTable("tblStaffMembersClockData");
            //TODO: Ref Jay: Why is this not a GUID?
            HasKey(x => x.ClockDataId);
            //HasKey(x => new { x.StaffId, x.ClockDateTime }); // Composite Key

            //TODO: Ref Jay: Why is this not a GUID?
            Property(x => x.ClockDataId).HasColumnName("ClockDataId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(x => x.StaffId).HasColumnName("StaffID");
            Property(x => x.ClockDateTime).HasColumnName("ClockDateTime");
            Property(x => x.OriginalClockDateTime).HasColumnName("OriginalClockDateTime");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
            Property(x => x.DataStatus).HasColumnName("DataStatus");
            Property(x => x.Comments).HasColumnName("Comments");
            //Property(x => x.IsLeaveRecord).HasColumnName("IsLeaveRecord");
            //Property(x => x.PrevClockDateTime).HasColumnName("PrevClockDateTime");

            //used for data manipulation
            Ignore(x => x.IsLeaveRecord);
            Ignore(x => x.LeaveType);
            Ignore(x => x.PrevClockDateTime);
            Ignore(x => x.Email);
            Ignore(x => x.IsPublicHoldiday);
            Ignore(x => x.DataStatusHighlight);
           



            //HasRequired(m => m.Staff).WithMany(m => m.StaffClockData).HasForeignKey(m => m.StaffId);
            // HasRequired(m => m.StaffMember).WithMany(m => m.StaffContactData).HasForeignKey(m => m.StaffId);

        }
    }
    public class StaffHoursMap : EntityTypeConfiguration<StaffHoursModel>
    {
        public StaffHoursMap()
        {
            ToTable("tblStaffMembersHours");
            //TODO: Ref Jay: Will need to replace this with a proper key.
            HasKey(x => new { x.StaffId, x.DayId }); // Composite Key

            Property(x => x.StaffId).HasColumnName("StaffID");
            Property(x => x.DayId).HasColumnName("DayID");
            Property(x => x.DayTimeStart).HasColumnName("DayTimeStart");
            Property(x => x.DayTimeEnd).HasColumnName("DayTimeEnd");
            Property(x => x.DayLunchLength).HasColumnName("DayLunchLength");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");

            HasRequired(m => m.Staff)
                .WithMany(m => m.StaffHoursData)
                .HasForeignKey(m => m.StaffId);

            // Do not map these
            Ignore(x => x.DayHoursRequired);
            Ignore(x => x.WeekHoursRequired);
        }
    }
    public class StaffLeaveMap : EntityTypeConfiguration<StaffLeaveModel>
    {
        public StaffLeaveMap()
        {
            ToTable("tblStaffMembersLeaveApps");
            HasKey(x => x.LeaveId);

            Property(x => x.LeaveDateStart).HasColumnName("LeaveDateStart");
            Property(x => x.LeaveDateEnd).HasColumnName("LeaveDateEnd");
            Property(x => x.LeaveComments).HasColumnName("LeaveComments");
            Property(x => x.LeaveType).HasColumnName("LeaveType");
            Property(x => x.LeaveStatus).HasColumnName("LeaveStatus");
            Property(x => x.LeaveRequestDate).HasColumnName("LeaveRequestDate");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
            Property(x => x.ApprovedBy1).HasColumnName("LeaveApprovedBy1");
            Property(x => x.ApprovedBy2).HasColumnName("LeaveApprovedBy2");
            Property(x => x.ReasonForAction).HasColumnName("ReasonForAction");
            

            Ignore(x => x.PublicHolidays);

            HasRequired(m => m.StaffMember)
                .WithMany(m => m.StaffLeaveData)
                .HasForeignKey(m => m.StaffId);


            //Child Relationships TODO: Need solution here
            //HasOptional(x => x.ApprovedBy1).WithMany().Map(m => m.MapKey("LeaveApprovedBy1"));
            //HasOptional(x => x.ApprovedBy2).WithMany().Map(m => m.MapKey("LeaveApprovedBy2"));
        }
    }
    public class StaffLeaveCounterMap : EntityTypeConfiguration<StaffLeaveCounterModel>
    {
        public StaffLeaveCounterMap()
        {
            //todo - consider creating a better name
            ToTable("tblStaffLeaveCounter");
            HasKey(m => m.RecordId);
            Property(m => m.RecordId).HasColumnName("RecordId");

            Property(m => m.Accumulator).HasColumnName("Accumulator");
            Property(m => m.EndPeriod).HasColumnName("EndPeriod");
            Property(m => m.StartPeriod).HasColumnName("StartPeriod");
            Property(m => m.StaffId).HasColumnName("StaffId");
            Property(m => m.RecordStatus).HasColumnName("RecordStatus");

            HasRequired(m => m.Staff).WithMany(m => m.LeaveCounters);
        }
    }
    public class StaffContactMap : EntityTypeConfiguration<StaffContactModel>
    {
        public StaffContactMap()
        {
            ToTable("tblStaffMembersContacts");
            HasKey(x => x.ContactId);
            Property(x => x.ContactId).HasColumnName("ContactID");
            Property(x => x.StaffId).HasColumnName("StaffID");
            Property(x => x.ContactName).HasColumnName("ContactName");
            Property(x => x.ContactSurname).HasColumnName("ContactSurname");
            Property(x => x.ContactNumber).HasColumnName("ContactNumber");
            Property(x => x.ContactDescription).HasColumnName("ContactDescription");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
            Ignore(x => x.ContactFullName);
            HasRequired(m => m.StaffMember).WithMany(m => m.StaffContactData).HasForeignKey(m => m.StaffId);
        }
    }
    public class StaffPhoneDetailMap : EntityTypeConfiguration<StaffPhoneDetailModel>
    {
        public StaffPhoneDetailMap()
        {
            ToTable("tblStaffPhoneDetails");
            HasKey(m => m.StaffId);

            Property(m => m.StaffId).HasColumnName("StaffId");

            Property(m => m.StaffPhoneRinger).HasColumnName("StaffPhoneRinger");
            Property(m => m.StaffPhoneDomain).HasColumnName("StaffPhoneDomain");
            Property(m => m.StaffPhoneNetMask).HasColumnName("StaffPhoneNetMask");
            Property(m => m.StaffPhoneGateway).HasColumnName("StaffPhoneGateway");
            Property(m => m.StaffPhoneDNS1).HasColumnName("StaffPhoneDNS1");
            Property(m => m.StaffPhoneDNS2).HasColumnName("StaffPhoneDNS2");
            Property(m => m.StaffPhoneHost).HasColumnName("StaffphoneHost");
            Property(m => m.StaffPhoneOutBound).HasColumnName("StaffPhoneOutBound");
            Property(m => m.StaffPhoneIp).HasColumnName("StaffPhoneIp");
            Property(m => m.StaffPhonePass).HasColumnName("StaffPhonePass");
            Property(m => m.StaffPhoneMac).HasColumnName("StaffPhoneMac");
            Property(m => m.RecordStatus).HasColumnName("RecordStatus");

            HasRequired(m => m.StaffMember)
                .WithRequiredDependent(m => m.PhoneDetails);

        }
    }


    //public class StaffLeaveSummaryMap : EntityTypeConfiguration<StaffLeaveSummaryModel>
    //{
    //    public StaffLeaveSummaryMap()
    //    {
    //        ToTable("tblStaffMembersLeaveData");
    //        HasKey(m => m.RecId);


    //        Property(m => m.StaffId).HasColumnName("StaffId");

    //        Property(m => m.Increment).HasColumnName("Increment");
    //        Property(m => m.StaffJoinDate).HasColumnName("StartDate");
    //        Property(m => m.EndDate).HasColumnName("EndDate");
    //        Property(m => m.RecordStatus).HasColumnName("RecordStatus");
    //        Property(m => m.LeaveType).HasColumnName("LeaveType");
    //        HasRequired(m => m.StaffMember).WithRequiredDependent(m => m.LeaveSummary);

    //        Ignore(x => x.DaysAvailable);
    //        Ignore(x => x.DaysTaken);
    //        //Ignore(x => x.StaffDefaultPrinter);
    //        //Ignore(x => x.StaffDivision);
    //        //Ignore(x => x.StaffManager1);
    //        //Ignore(x => x.StaffManager2);       


    //    }
    //}

    public class LeaveSummaryMap : EntityTypeConfiguration<StaffLeaveSummary>
    {
        public LeaveSummaryMap()
        {
            ToTable("tblStaffMembersLeaveData");
            HasKey(m => m.RecId);


            Property(m => m.StaffId).HasColumnName("StaffId");

            Property(m => m.Increment).HasColumnName("Increment");
            Property(m => m.StaffJoinDate).HasColumnName("StartDate");
            Property(m => m.EndDate).HasColumnName("EndDate");
            Property(m => m.RecordStatus).HasColumnName("RecordStatus");
            Property(m => m.LeaveType).HasColumnName("LeaveType");
            //HasRequired(m => m.StaffMember).WithRequiredDependent(m => m.LeaveSummary);
            HasRequired(m => m.StaffMember)
                .WithMany(m => m.LeaveSummary)
                .HasForeignKey(m => m.StaffId);


            Ignore(x => x.DaysAvailable);
            Ignore(x => x.DaysTaken);
            //Ignore(x => x.StaffDefaultPrinter);
            //Ignore(x => x.StaffDivision);
            //Ignore(x => x.StaffManager1);
            //Ignore(x => x.StaffManager2);       


        }
    }


}
