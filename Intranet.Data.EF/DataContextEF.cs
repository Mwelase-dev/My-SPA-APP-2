using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Intranet.Data.EF.Mappings;
using Intranet.Models;

namespace Intranet.Data.EF
{
    public class DataContextEF : DbContext
    {
        #region Internal
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Use singular table names
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // Configuration.ProxyCreationEnabled = false;
          
            modelBuilder.Configurations.Add(new BranchMap());
            modelBuilder.Configurations.Add(new DivisionMap());
            modelBuilder.Configurations.Add(new HolidayMap());
            modelBuilder.Configurations.Add(new LinkCategoryMap());
            modelBuilder.Configurations.Add(new LinkMap());
            modelBuilder.Configurations.Add(new MenuMap());
            modelBuilder.Configurations.Add(new AnnouncementMap());
            modelBuilder.Configurations.Add(new ThoughtMap());

            modelBuilder.Configurations.Add(new StaffMap());

            modelBuilder.Configurations.Add(new StaffClockMap());
            modelBuilder.Configurations.Add(new StaffHoursMap());
            modelBuilder.Configurations.Add(new StaffLeaveMap());
            modelBuilder.Configurations.Add(new StaffLeaveCounterMap());
            modelBuilder.Configurations.Add(new StaffContactMap());
            modelBuilder.Configurations.Add(new SuggestionMap());
            modelBuilder.Configurations.Add(new StaffSuggestionVotesModelMap());
            modelBuilder.Configurations.Add(new StaffSuggestionFollowerMap());

            //Printers

            //modelBuilder.Configurations.Add(new PrinterPropertiesPropertiesMapping());
            modelBuilder.Configurations.Add(new PrinterServiceProviderModelMapping());
            modelBuilder.Configurations.Add(new PrinterModelMapping());
            modelBuilder.Configurations.Add(new PrinterPropertiesModelMapping());
            modelBuilder.Configurations.Add(new PrinterPropertiesPrinterMapping());
            modelBuilder.Configurations.Add(new TonerOrdersModelMapping());
            modelBuilder.Configurations.Add(new TonerOrderDetailsModelMapping());

            //roles
            modelBuilder.Configurations.Add(new UserRolesModelMapping());
            modelBuilder.Configurations.Add(new RolesModelMapping());

            //messages
            modelBuilder.Configurations.Add(new MessagesModelMapping());

            //Staff phoe details
            modelBuilder.Configurations.Add(new StaffPhoneDetailMap());

            //StaffLeave Summary
            //modelBuilder.Configurations.Add(new StaffLeaveSummaryMap());

            modelBuilder.Configurations.Add(new LeaveSummaryMap());

            //ClockDevices
            modelBuilder.Configurations.Add(new ClockDeviceMapping());

            Database.SetInitializer<DataContextEF>(null);
        }
        #endregion

        public DataContextEF()
            : base(nameOrConnectionString: "Intranet")
        {
            Configuration.ProxyCreationEnabled = false;
        }

        #region DB Sets
        public DbSet<StaffLeaveCounterModel> StaffLeaveCounter { get; set; }
        public DbSet<BranchModel> Branches { get; set; }
        public DbSet<DivisionModel> BranchDivisions { get; set; }
        public DbSet<HolidayModel> Holidays { get; set; }
        public DbSet<MenuModel> Menus { get; set; }
        public DbSet<AnnouncementModel> Announcements { get; set; }
        public DbSet<ThoughtModel> Thoughts { get; set; }
        public DbSet<LinkCategoryModel> LinkCategories { get; set; }
        public DbSet<LinkModel> Links { get; set; }
        public DbSet<StaffModel> Staff { get; set; }
        public DbSet<StaffClockModel> StaffClockData { get; set; }
        public DbSet<StaffHoursModel> StaffHourData { get; set; }
        public DbSet<StaffLeaveModel> StaffLeaveData { get; set; }
        public DbSet<StaffContactModel> StaffContactData { get; set; }
        public DbSet<SuggestionModel> Suggestions { get; set; }
        public DbSet<StaffSuggestionFollower> StaffSuggestionFollowerData { get; set; }
        //public DbSet<StaffLeaveSummaryModel> LeaveSummaryData { get; set; }
        public DbSet<StaffLeaveSummary> LeaveSummary { get; set; }

        //printers
       // public DbSet<PrinterPropertiesProperties> PropertiesOfPrinters { get; set; }
        public DbSet<PrinterModel> Printers { get; set; }
        public DbSet<PrinterServiceProviderModel> PrinterServiceProviders { get; set; }
        public DbSet<PrinterPropertyModel> PrinterProperties { get; set; }
        public DbSet<PrinterPropertiesPrinterModel> PrinterPropertiesPrinter { get; set; }
        public DbSet<TonerOrdersModel> TonerOrders { get; set; }
        public DbSet<TonerOrderDetailsModel> TonerOrderDetails { get; set; }

        //Roles
        //public DbSet<UserRolesModel> UserRoles { get; set; }
        //public DbSet<RolesModel> Roles { get; set; }

        //Messages
        public DbSet<MessagesModel> Messages { get; set; }

        //Phone details
        public DbSet<StaffPhoneDetailModel> PhoneDetails { get; set; }

        //ClockDevice
        public DbSet<ClockDeviceModel> ClockDeviceData { get; set; }

        #endregion

        public bool IsHolidayToday()
        {
            // Check if today is a holiday?
            var isHoliday = Holidays.Where(x => x.HolidayDate == DateTime.Today).FirstOrDefault();
            return isHoliday != null;
        }
    }
}



