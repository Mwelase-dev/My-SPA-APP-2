//#define NHibernate
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
//using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Runtime.Remoting.Contexts;
using System.Security.AccessControl;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Xml;
using System.Xml.Serialization;
using Breeze.WebApi;
using Intranet.Business;
using Intranet.Business.Data_Model;
using Intranet.Data.EF;
using Intranet.Messages;
using Intranet.Models;
using Intranet.Models.Enums;
using Intranet.PhoneUsage;
//using Intranet.UI.Hubs;
//using Intranet.UI.Hubs;
using Intranet.UI.Hubs;
using Intranet.UI.Models;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using Owin;
using NHibernate.Criterion;
using Utilities;
using System.DirectoryServices.AccountManagement;
using Intranet.UI.Helpers;
using System.Web.Configuration;
using Utilities.Helpers;
using EntityState = System.Data.EntityState;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace Intranet.UI.Controllers
{
    [BreezeController]
    public class BreezeDataController : ApiController
    {
        #region Internal

        /// <summary>
        /// Data context required for DB Access.
        /// </summary>
        readonly ContextProvider ctx = new ContextProvider();

        /// <summary>
        /// This method exposes the Metadata required by Breeze to build the entities on the client
        /// </summary>
        /// <returns></returns>

        [HttpGet]
        public string MetaData()
        {
            return ctx.Metadata();
        }

        protected virtual bool BeforeSaveEntity(EntityInfo info)
        {
            if (info.Entity.Equals(typeof(StaffLeaveModel)))
            {

            }

            return default(bool);
        }

        /// <summary>
        /// This saves the changes to the DB, through the context provider.
        /// </summary>
        /// <param name="saveBundle"></param>
        /// <returns></returns>

        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            // Need security here!!!
            // BeforeSave method available here
            return ctx.SaveChanges(saveBundle);
        }

        #endregion

        #region DataQueries
        #region Menu
        [HttpGet]
        public IQueryable<MenuModel> Menus()
        {
            return ctx.Context.Menus
                .Where(x => x.RecordStatus == "Active")
                .OrderBy(x => x.MenuOrder);
        }
        #endregion
        #region Announcements
        [HttpGet]
        public IQueryable<AnnouncementModel> Announcements()
        {
            return ctx.Context.Announcements
                .Where(x => x.RecordStatus != "Deleted")
                .OrderByDescending(x => x.AnnouncementDate);
        }
        #endregion
        #region Thoughts
        [HttpGet]
        public ThoughtModel ThoughtRandom()
        {
            return ThoughtList() // see below
                .OrderBy(t => Guid.NewGuid())
                .First();
        }
        [HttpGet]
        public IQueryable<ThoughtModel> ThoughtList()
        {
            return ctx.Context.Thoughts
                .Where(x => x.RecordStatus == "Active");
        }
        #endregion
        #region Links
        [HttpGet]
        public IQueryable<LinkCategoryModel> LinkCategories()
        {
            var data = ctx.Context.LinkCategories.Include("CategoryLinks")
                .Where(x => x.RecordStatus == "Active")
                .OrderBy(x => x.CategoryOrder).AsEnumerable().ToList();

            var results = new List<LinkCategoryModel>(data);
            for (var i = results.Count() - 1; i >= 0; i--)
            {
                var cat = results[i];
                for (var j = cat.CategoryLinks.Count() - 1; j >= 0; j--)
                {
                    var link = cat.CategoryLinks[j];
                    {
                        if (link.RecordStatus != "Active")
                        {
                            cat.CategoryLinks.RemoveAt(j);
                        }
                    }
                }
            }
            return results.AsQueryable();
        }

        [HttpGet]
        public IQueryable<LinkModel> Links()
        {
            var data = ctx.Context.Links
                .Where(x => x.RecordStatus == "Active")
                .OrderByDescending(x => x.LinkDesc).ToList();

            for (var i = 0; i < data.Count(); i++)
            {
                if (data[i].RecordStatus.Equals("Delete"))
                {
                    data.RemoveAt(i);
                }
            }

            return data.AsQueryable();
        }
        #endregion
        #region Branches
        [HttpGet]
        public IQueryable<BranchModel> BranchList()
        {
            var data = ctx.Context.Branches.Include("BranchDivisions")
                          .Include("BranchDivisions.DivisionStaff")
                          .Include("BranchDivisions.DivisionStaff.StaffLeaveData")
                          .Include("BranchDivisions.DivisionStaff.PhoneDetails")
                          .Where(x => x.RecordStatus == "Active")
                          .OrderByDescending(x => x.BranchName).AsEnumerable();

            var info = new List<BranchModel>(data.ToList());

            for (var i = 0; i < info.Count(); i++)
            {
                var branch = info[i];
                for (var j = branch.BranchDivisions.ToList().Count() - 1; j >= 0; j--)
                {
                    var branchDiv = branch.BranchDivisions.ToList()[j];
                    for (var k = branchDiv.DivisionStaff.ToList().Count() - 1; k >= 0; k--)
                    {
                        var divStaff = branchDiv.DivisionStaff.ToList()[k];
                        if (divStaff.RecordStatus != "Active")
                        {
                            branchDiv.DivisionStaff.Remove(divStaff);
                        }
                    }

                    if (branchDiv.RecordStatus != "Active")
                        branch.BranchDivisions.Remove(branchDiv);
                }
            }
            return info.ToList().AsQueryable();
        }

        /// <summary>
        /// This is for look up purposes
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IQueryable<BranchModel> BranchLookUp()
        {
            return ctx.Context.Branches
                      .Where(m => m.RecordStatus.Equals("Active"))
                      .OrderBy(m => m.BranchName);
        }
        #endregion
        #region Divisions
        [HttpGet]
        public IQueryable<DivisionModel> DivisionList()
        {
            return ctx.Context.BranchDivisions.Include("DivisionStaff")
                .Where(x => x.RecordStatus == "Active")
                .OrderByDescending(x => x.DivisionName);
        }


        [HttpGet]
        public IQueryable<DivisionModel> DivisionLookUp()
        {
            //
            return ctx.Context.BranchDivisions
                      .Where(m => m.RecordStatus.Equals("Active"))
                      .OrderBy(m => m.DivisionName);
        }
        #endregion
        #region Staff
        [HttpGet]
        public IQueryable<StaffModel> StaffByManager()
        {
            #region Get managers from ad
            var audit = new StringBuilder();
            var managers = new List<StaffModel>();
            using (var context = new PrincipalContext(ContextType.Domain, "Pacific"))
            {
                using (var group = GroupPrincipal.FindByIdentity(context, "Management"))
                {
                    if (group == null)
                    {
                        audit.Append("No users found under the management group");
                    }
                    else
                    {
                        var store = new DataContextEF();

                        group.GetMembers(true)
                            .ToList()
                            .ForEach((m) =>
                            {
                                var staff =
                                    store.Staff
                                    .FirstOrDefault(
                                        s => s.StaffNtName.ToLower().Contains(((UserPrincipal)m).SamAccountName.ToLower()));

                                if (staff == null)
                                    audit.Append("Manager not found \\t " + m.SamAccountName);
                                else
                                    managers.Add(staff);
                            });
                    }
                }
            }
            #endregion

            IList<StaffModel> staffUnderManager = new List<StaffModel>();
            bool loggedInIsManager = false;
            var ctx = new DataContextEF();

            var loggedInNtname = CurrentUser();
            var firstOrDefault = ctx.Staff.FirstOrDefault(x => x.StaffNtName.Contains(loggedInNtname));
            if (firstOrDefault == null)
                return default(IQueryable<StaffModel>);

            foreach (var manager in managers)
            {
                if (CurrentUser().ToLower().Contains(manager.StaffNtName))
                {
                    loggedInIsManager = true;
                }
            }

            if (loggedInIsManager)
            {
                var loggedInStaffId = firstOrDefault.StaffId;
                IList<StaffModel> staff = StaffByManagers(loggedInStaffId);
                if (!staff.Any())
                {
                    audit.Append("Staff found for Managers - " + staff.Count()).ToString();
                }
                else
                {
                    foreach (var staffModel in staff)
                    {
                        staffUnderManager.Add(staffModel);
                    }
                }
            }

            return staffUnderManager.AsQueryable();
        }

        [HttpGet]
        public IQueryable<StaffModel> StaffR()
        {
            return ctx.Context.Staff
                       .Where(x => x.RecordStatus == "Active" && x.StaffIsClockingMember.Equals(true)/**/)
                       .OrderBy(x => x.StaffName);
        }

        [HttpGet]
        public IQueryable<StaffModel> Staff()
        {
            return ctx.Context.Staff
                       .Where(x => x.RecordStatus == "Active" /*&& x.StaffIsClockingMember.Equals(true)*/)
                       .OrderByDescending(x => x.StaffSurname);
        }

        [HttpGet]
        public IQueryable<StaffModel> Directors()
        {
            return ctx.Context.Staff
                      .Where(x => x.RecordStatus == "Active")
                       .Where(x => x.IsDirector == true)
                      .OrderByDescending(x => x.StaffSurname);
        }

        //TODO: Needs some work here.
        [HttpGet]
        public IQueryable<StaffModel> StaffBirthdays()
        {
            var dateFrom = DateTime.Today.AddDays(-2); //TODO: Needs to be a config file setting
            var dateTo = DateTime.Today.AddDays(20); //TODO: Needs to be a config file setting

            // Need to do the .ToList here as we are working with a "Virtual" Field
            var list = Staff().ToList();

            return list
                .Where(x => x.StaffIsClockingMember.Equals(true))
                 .Where(x => x.BroadcastBirthday.Equals(true))
                .Where(x => (x.StaffBirthday >= dateFrom) && (x.StaffBirthday <= dateTo) && (x.RecordStatus == "Active"))
                .OrderBy(x => x.StaffBirthday)
                .AsQueryable();
        }

        [HttpGet]
        public IQueryable<SuggestionModel> StaffSuggestions()
        {
            return ctx.Context.Suggestions
                      .Where(x => x.RecordStatus == "Active");
        }

        [HttpGet]
        public IQueryable<StaffSuggestionFollower> StaffSuggestionFollowers()
        {
            return ctx.Context.StaffSuggestionFollowerData;
        }

        [HttpGet]
        public IQueryable<StaffContactModel> StaffContacts()
        {
            return ctx.Context.StaffContactData.Where(m => m.RecordStatus.Equals("Active"));
        }

        [HttpGet]
        public bool DialContact(string phoneNumber)
        {
            phoneNumber = phoneNumber.Replace(" ", "");
            phoneNumber = phoneNumber.Replace("-", "");
            phoneNumber = phoneNumber.Replace("(", "");
            phoneNumber = phoneNumber.Replace(")", "");
            phoneNumber = phoneNumber.Replace("+", "00");

            var currentUser = CurrentUser();

            var staff = new DataContextEF()
                .Staff
                .Include("PhoneDetails")
                .FirstOrDefault(m => m.StaffNtName.Equals(currentUser));

            if (staff == null)
                return false;

            var phoneDet = staff.PhoneDetails;
            if (phoneDet == null)
                return false;

            try
            {
                // Prepare web request... = http://172.16.2.82/index.htm?adrnumber=0719558872&outgoing=Active
                var ownExt = staff.StaffTellExt;
                var phoneIp = "";

                bool isCapeTown = int.Parse(ownExt) >= 500 && int.Parse(ownExt) <= 599;
                bool isDurban = int.Parse(ownExt) >= 600 && int.Parse(ownExt) <= 699;
                bool isEastLondon = int.Parse(ownExt) >= 700 && int.Parse(ownExt) <= 799;
                bool isPortElizabeth = int.Parse(ownExt) >= 900 && int.Parse(ownExt) <= 999;

                if (isCapeTown)
                    phoneIp = "172.16.5." + (int.Parse(ownExt) - 500);
                if (isDurban)
                    phoneIp = "172.16.6." + (int.Parse(ownExt) - 600);
                if (isEastLondon)
                    phoneIp = "172.16.2." + (int.Parse(ownExt) - 700);
                if (isPortElizabeth)
                    phoneIp = "192.168.4." + (int.Parse(ownExt) - 900);


                var client = new WebClient();
                var content = client.DownloadString("http://" + phoneIp + "?NUMBER=" + phoneNumber + "&DIAL=Dial&active_line=1");
                bool dialed = String.IsNullOrEmpty(content);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
        #region Gallery
        [HttpGet]
        public IQueryable<GalleryModel> GalleryList()
        {
            return UoWGallery.GetFolderList().AsQueryable();
        }
        [HttpGet]
        public IQueryable<GalleryImageModel> GetGalleryImageList(String galleryName)
        {
            return UoWGallery.GetImageList(galleryName).AsQueryable();
        }
        #endregion
        #region Backend Data
        [HttpGet]
        public IQueryable<HolidayModel> HolidaysList()
        {
            return ctx.Context.Holidays
                      .Where(x => x.RecordStatus == "Active");
        }

        [HttpGet]
        public string CurrentUser()
        {
            try
            {
                var windowsIdentity = User.Identity as WindowsIdentity;
                return windowsIdentity != null ? windowsIdentity.Name : string.Empty;
            }
            catch (Exception ex)
            {
                // Log exception
                return ex.ToString();
            }
        }


        [HttpGet]
        public IQueryable<StaffModel> CurrentStaff()
        {
            var currentUserName = CurrentUser();

            var staff = Staff().Where(m => m.StaffNtName.Equals(currentUserName));

            if (staff == null)
                throw new Exception("Staff member not found");

            return staff.AsQueryable();

        }

        [HttpGet]
        public bool CurrentUserIsAdmin()
        {
            try
            {
                // TODO: Make role a config setting
                return Roles.IsUserInRole(@"NVESTHOLDINGS\IntranetAdmins");
            }
            catch
            {
                // Log exception
                return false;
            }
        }

        #endregion
        #region Clocking Data

        [HttpGet]
        public IQueryable<StaffModel> GetClockDataProfiles(Guid id)
        {

            var data = ctx.Context.Staff.Where(x => x.ClockDevice != null).ToList();

            if (data.Any())
            {
                data = data.Where(x => x.ClockDevice.Equals(id)).ToList();
            }
            return data.AsQueryable();
        }


        [HttpGet]
        public IQueryable<ClockDeviceModel> ClockDevices()
        {
            var data = ctx.Context.ClockDeviceData.Where(x => x.RecordStatus.Equals("Active")).ToList().OrderBy(x => x.ClockDeviceIp);
            return data.AsQueryable();

        }

        [HttpGet]
        public IQueryable<StaffHoursModel> GetStaffWorkHours(Guid id)
        {
            return ctx.Context.StaffHourData.Where(x => x.RecordStatus.Equals("Active") && x.StaffId.Equals(id));
        }

        [HttpGet]
        public IQueryable<StaffClockingContainer> StaffClockData(int id, Guid staffId)
        {
            using (var store = new DataContextEF())
            {

                var data = from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                           where s.StaffClockId.Equals(id) && s.StaffId.Equals(staffId) && s.RecordStatus.Equals("Active")
                           select s;

                var staff = data.FirstOrDefault();
                if (staff == null)
                    return default(IQueryable<StaffClockingContainer>);

                //clockdata within the range specified
                staff.StaffClockData =
                    staff.StaffClockData.Where(
                        m =>
                        m.ClockDateTime.Date >= DateTime.Now.Date).ToList();

                //leavedata within the range specified
                staff.StaffLeaveData = staff.StaffLeaveData.Where(
                    m =>
                        m.LeaveDateStart >= DateTime.Now.Date).ToList();

                return TimeKeepingContainer.GetStaffClockingData(staff).AsQueryable();
            }
        }


        [HttpGet]
        public HttpResponseMessage PeOfficeClockIn(string remoteClockData)
        {
            using (var dtx = new DataContextEF())
            {
                StaffClockModel remoteClock =
                    dtx.StaffClockData.First(x => x.Staff.StaffClockCardNumber.Equals(remoteClockData.Split('-').Last()));

                if (remoteClock != null)
                {
                    remoteClock.DataStatus = 0;
                    remoteClock.ClockDateTime = DateTime.Now;
                    remoteClock.RecordStatus = "Active";
                    remoteClock.StaffId = GetStaffIdByCardNumber(remoteClock.StaffId);
                    remoteClock.Comments = remoteClock.Comments +
                                           ("PE office clock in");

                    dtx.StaffClockData.Add(remoteClock);
                    dtx.SaveChanges();


                    object obj = new
                    {
                        Name = remoteClock.Staff.StaffFullName,
                        Time = DateTime.Now.TimeOfDay,
                        CardNumber = remoteClock.Staff.StaffClockCardNumber

                    };

                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        private Guid GetStaffIdByEmail(string staffEmail)
        {
            using (var dataContext = new DataContextEF())
            {
                var staffId = from staffmember in dataContext.Staff
                              where staffmember.StaffEmail.Equals(staffEmail)
                              select staffmember.StaffId;

                return staffId.FirstOrDefault();
            }
        }

        private Guid GetStaffIdByCardNumber(Guid staffid)
        {
            using (var dataContext = new DataContextEF())
            {
                var staffId = from staffmember in dataContext.Staff
                              where staffmember.StaffId.Equals(staffid)
                              select staffmember.StaffId;

                return staffId.FirstOrDefault();
            }
        }



        [HttpGet]
        public bool ReminderToClearOldTonerOrders()
        {
            return UoWTonerManager.ClearOldTonerOrders();
        }

        [HttpGet]
        public Boolean ProcessClockingData()
        {
            try
            {

                return UoWStaff.ProcessClocking();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public Boolean SyncDeviceTimes()
        {
            try
            {

                return UoWStaff.SyncTimes();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public Boolean ClockingReminders()
        {
            try
            {

                return UoWStaff.ClockingReminders();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public Boolean SendLeaveEmails()
        {
            try
            {

                return UoWStaff.SendLeaveEmails();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public Boolean IncompleteClockData()
        {
            try
            {

                return UoWStaff.DailyClockRecords();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public Boolean DeductHoursFromLeave()
        {
            try
            {

                return UoWStaff.DeductHoursFromLeave();
            }
            catch
            {
                throw;
            }
        }
         
        [HttpGet]
        public string ProcessWeeklyManagerMail()
        {
            var audit = new StringBuilder();
            var managers = new List<StaffModel>();
            try
            {
                #region Get managers from ad
                using (var context = new PrincipalContext(ContextType.Domain, "Pacific"))
                {
                    using (var group = GroupPrincipal.FindByIdentity(context, "Management"))
                    {
                        if (group == null)
                        {
                            audit.Append("No users found under the management group");
                        }
                        else
                        {
                            var store = new DataContextEF();

                            group.GetMembers(true)
                                .ToList()
                                .ForEach((m) =>
                                {
                                    var staff =
                                        store.Staff
                                        .FirstOrDefault(
                                            s => s.StaffEmail.ToLower().Equals(((UserPrincipal)m).EmailAddress.ToLower()));

                                    if (staff == null)
                                        audit.Append("Manager not found \\t " + m.SamAccountName);
                                    else
                                        managers.Add(staff);
                                });
                        }
                    }
                }
                #endregion

                var clockMessage = string.Empty;
                var leaveMessage = string.Empty;

                // Get all staff for a managers
                using (var store = new DataContextEF())
                {
                    IList<StaffModel> staffClocksToGetWeekData = new List<StaffModel>();
                    IList<StaffModel> staffUnapprovedClocks = new List<StaffModel>();
                    foreach (var man in managers)
                    {
                        IList<StaffModel> staff = StaffClockModelForEmails(man.StaffId);

                        if (!staff.Any())
                        {
                            audit.Append("Staff found for Managers - " + staff.Count()).ToString();
                        }
                        else
                        {
                            foreach (var staffModel in staff)
                            {
                                #region
#if !DEBUG

                                DateTime mondayDate = DateTime.Now.Subtract(TimeSpan.FromDays(5));
                                DateTime fridayDate = DateTime.Today;

 
#endif
                                #endregion

                                DateTime mondayDate = new DateTime(2015, 02, 09);
                                DateTime fridayDate = new DateTime(2015, 02, 13);
                                staffModel.StaffClockData = staffModel.StaffClockData.Where(m => m.ClockDateTime.Date >= mondayDate.Date && m.ClockDateTime.Date <= fridayDate.Date).ToList();
                                staffClocksToGetWeekData.Add(staffModel);
                            }
                            TimeKeepingContainer.EmialManager(man.StaffEmail, staffClocksToGetWeekData.ToList());
                        }

                        IList<StaffModel> staffWithClockApprovals = StaffClockModelForEmailsAndUnApprovedRecords(man.StaffId);

                        if (!staffWithClockApprovals.Any())
                        {
                            audit.Append("Staff found for Managers - " + staffWithClockApprovals.Count()).ToString();
                        }
                        else
                        {
                            foreach (var staffModel in staffWithClockApprovals)
                            {
                                staffModel.StaffClockData = staffModel.StaffClockData.Where(m => m.DataStatus == 2).ToList();
                                staffUnapprovedClocks.Add(staffModel);
                            }

                            WeeklyMessenger.EmialManager(man.StaffEmail, staffUnapprovedClocks.ToList());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                audit.Append("An error has occured while processing your request. Details -" + exception.Message);
            }
            return audit.Append(" Managers found - " + managers.Count()).ToString();
            //get all the staff members managed by the manager
            //include their pendding emails
            //include clocking records which are in pending/ edited states
        }




        #region Weekly Emails
        /// <summary>
        /// Method used to remind managers about pending leaves and clocking records
        /// </summary>
        /// <returns>Returns a json object detailing the process results
        /// </returns>

        #endregion

        [HttpPost]
        public Boolean SaveOffSiteData(StaffClockModel offsiteclockdata)
        {
            try
            {
                using (DataContextEF dtx = new DataContextEF())
                {


                    dtx.StaffClockData.Add(offsiteclockdata);
                    dtx.SaveChanges();

                    var todaysDate = DateTime.Now.Date;
                    var staffDayClockData = dtx.StaffClockData


                   .Where(x => x.StaffId.Equals(offsiteclockdata.StaffId)
                   && x.ClockDateTime == todaysDate)
                   .OrderByDescending(x => x.ClockDateTime)
                   .ToList();

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }


        }

        [HttpGet]
        public IQueryable<StaffClockingContainer> GetGraphData(Guid id, string startDate, string endDate)
        {
            var data1 = from s in ctx.Context.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                        where s.StaffId.Equals(id)
                        select s;

            if (data1 == null)
                throw new Exception("Staff clock data not found");

            var staff = data1.FirstOrDefault();

            if (staff == null)
                throw new Exception("Staff clock data not found");

            DateTime start = new DateTime();
            DateTime end = new DateTime();
            //x.DataStatus != 8 &&
            staff.StaffClockData = staff.StaffClockData.Where(
                              x =>
                              x.ClockDateTime.Date >= DateTime.Parse(startDate).Date &&
                              x.ClockDateTime.Date <= DateTime.Parse(endDate).Date).ToList();

            staff.StaffLeaveData = staff.StaffLeaveData.Where(
                       m =>
                       m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                       m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date).ToList();

            return TimeKeepingContainer.GetStaffClockingData(staff).AsQueryable();
            // return TimeKeepingContainer.GetStaffClockingDataForClockingGraphs(staff).AsQueryable();
        }

        [HttpGet]
        public IQueryable<StaffClockingContainer> TimeKeepingData(int id, string startDate, string staffName, string staffSurname, string endDate)
        {
            using (var store = new DataContextEF())
            {


                var data = from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                           where s.StaffClockId.Equals(id) && s.StaffName.Contains(staffName) && s.StaffSurname.Contains(staffSurname) && s.RecordStatus.Equals("Active")
                           select s;

                var staff = data.FirstOrDefault();
                if (staff == null)
                    return default(IQueryable<StaffClockingContainer>);

                //clockdata within the range specified
                staff.StaffClockData =
                    staff.StaffClockData.Where(
                        m =>
                        m.DataStatus != 8 &&
                        m.ClockDateTime.Date >= DateTime.Parse(startDate).Date &&
                        m.ClockDateTime.Date <= DateTime.Parse(endDate).Date).ToList();

                //leavedata within the range specified
                staff.StaffLeaveData = staff.StaffLeaveData.Where(
                    m =>
                        m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                        m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date/* &&
                        m.LeaveStatus.Equals(1)*/).ToList();

                return TimeKeepingContainer.GetStaffClockingData(staff).AsQueryable();
            }

        }

        [HttpGet]
        public IQueryable<StaffClockModel> StaffClockModelData()
        {
            return ctx.Context.StaffClockData.Where(m => m.RecordStatus.Equals("Active"));
        }

        [HttpGet]
        public IQueryable<StaffModel> StaffClockModel()
        {
            IList<StaffModel> myList = new List<StaffModel>();
            foreach (var staffModel in StaffClockModelAllAll())
            {
                var staff = staffModel;
                staff.StaffClockData = new Collection<StaffClockModel>(staffModel.StaffClockData.Where(model => model.DataStatus == 2).ToList());
                staff.StaffClockData = new Collection<StaffClockModel>(staffModel.StaffClockData.Where(model =>
                    model.DataStatus == 2 ||
                    model.DataStatus == 111112).ToList());
                myList.Add(staff);
            }
            return myList.AsQueryable();
        }

        [HttpGet]
        public StaffModel StaffClockModel(Guid id)
        {
            using (var dataContext = new DataContextEF())
            {
                var data =
                    dataContext.Staff.Include("StaffClockData")
                        .Include("StaffHoursData")
                        .Include("StaffLeaveData")
                        .First(x => x.StaffId.Equals(id) && x.StaffClockData.Any(m => m.DataStatus.Equals(2)));

                if (data != null)
                {
                    var staff = (StaffModel)data;
                    var staffClock = data.StaffClockData;

                    staffClock = staffClock.Where(x => x.DataStatus.Equals(2)).ToList();
                    staff.StaffClockData = staffClock;
                    return staff;
                }
            }
            return default(StaffModel);
        }

        [HttpGet]
        public IQueryable<StaffModel> StaffClockModelAll()
        {
            IList<StaffModel> myList = new List<StaffModel>();
            foreach (var staffModel in StaffClockModelAllAll())
            {
                var staff = staffModel;
                staff.StaffClockData = new Collection<StaffClockModel>(staffModel.StaffClockData.Where(model => model.DataStatus == 2).ToList());
                staff.StaffClockData = new Collection<StaffClockModel>(staffModel.StaffClockData.Where(model =>
                    model.DataStatus == 2 ||
                    model.DataStatus == 111112).ToList());
                myList.Add(staff);
            }
            return myList.AsQueryable();
        }

        [HttpGet]
        public IList<StaffModel> StaffClockModelAllAll()
        {
            var richStaffModel = from staffModel in ctx.Context.Staff.Include("StaffClockData").Include("StaffClockData").Include("StaffHoursData")
                                 where staffModel.StaffClockData.Any(x => x.DataStatus.Equals(2))
                                 where staffModel.StaffClockData.Any(x =>
                                     x.DataStatus.Equals(2) ||
                                     x.DataStatus.Equals(111112))
                                 select staffModel;
            return richStaffModel.ToList();
        }

        [HttpGet]
        public IList<StaffModel> StaffClockModelForEmails(Guid managerStaffId)
        {
            var richStaffModel = from staffModel in ctx.Context.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                                 where staffModel.StaffManager1Id.Equals(managerStaffId)
                                 select staffModel;
            return richStaffModel.ToList();
        }

        [HttpGet]
        public IList<StaffModel> StaffByManagers(Guid managerStaffId)
        {
            var richStaffModel = from staffModel in ctx.Context.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                                 where staffModel.StaffManager1Id.Equals(managerStaffId)
                                 select staffModel;
            return richStaffModel.ToList();
        }

        [HttpGet]
        public IList<StaffModel> StaffClockModelForEmailsAndUnApprovedRecords(Guid managerStaffId)
        {
            var richStaffModel = from staffModel in ctx.Context.Staff.Include("StaffClockData").Include("StaffHoursData")
                                 where staffModel.StaffManager1Id.Equals(managerStaffId)/* && staffModel.StaffClockData.Any(x => x.DataStatus.Equals(2))*/
                                 select staffModel;
            return richStaffModel.ToList();
        }

        [HttpGet]
        public Boolean RemindUserOfMissedClockIn()
        {
            try
            {
                return UoWStaff.ProcessClocking();
            }
            catch
            {
                throw;
            }
        }

        [HttpGet]
        public byte[] ExportClockDataToExcel(string id, string startDate, string endDate)
        {
            try
            {
                using (var store = new DataContextEF())
                {
                    //var data2 = from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                    //           where s.StaffClockId.Equals(id) && s.StaffName.Contains(staffName) && s.StaffSurname.Contains(staffSurname) && s.RecordStatus.Equals("Active")
                    //           select s;

                    //var data = store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData").First(x => x.StaffClockId.Equals(int.Parse(id)));

                    int clockId = int.Parse(id);
                    var data = from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                               where s.StaffClockId.Equals(clockId)
                               select s;


                    var staff = data.FirstOrDefault();

                    if (staff != null)
                    {
                        staff.StaffClockData =
                            staff.StaffClockData.Where(
                                m =>
                                    m.ClockDateTime.Date >= DateTime.Parse(startDate).Date &&
                                    m.ClockDateTime.Date <= DateTime.Parse(endDate).Date).ToList();

                        var clockData = TimeKeepingContainer.GetStaffClockingData(staff);

                        return TimeKeepingContainer.ExportClockDataToExcel(clockData);
                    }

                    return default(byte[]);
                }
            }
            catch
            {
                return default(byte[]);
                throw;
            }
        }

        [HttpGet]
        public byte[] ExportPayrollLeaveBalances(string startDate, string endDate, string companyId, string divisionId, string staffId)
        {
            using (var store = new DataContextEF())
            {
                var leavedata =
                    store.Staff.Include("StaffLeaveData")
                        .ToList();

                var branches =
                    store.Branches.Include("BranchDivisions")
                        .Include("BranchDivisions.DivisionStaff")
                        .Where(b => b.RecordStatus == "Active")
                        .ToList();

                #region Filter Company

                if ((companyId != null) && (Guid.Parse(companyId) != Guid.Empty))
                {
                    branches = branches.FindAll(data => data.BranchId == Guid.Parse(companyId));
                }

                #endregion

                foreach (var branch in branches)
                {
                    #region Filter Divisions

                    if ((divisionId != null) && (Guid.Parse(divisionId) != Guid.Empty))
                    {
                        branch.BranchDivisions =
                            branch.BranchDivisions.ToList().FindAll(m => m.DivisionId.Equals(Guid.Parse(divisionId)));
                    }

                    #endregion

                    foreach (var div in branch.BranchDivisions.ToList())
                    {
                        #region Filter Staff

                        div.DivisionStaff = div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList();
                        if ((staffId != null) && (Guid.Parse(staffId) != Guid.Empty))
                        {
                            div.DivisionStaff =
                                div.DivisionStaff.Where(data => data.StaffId == Guid.Parse(staffId)).ToList();
                        }

                        #endregion

                        //foreach (var staff in div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList())
                        foreach (var staff in div.DivisionStaff)
                        {

                            #region Filter Date

                            if (staff.StaffLeaveData == null)
                                continue;

                            //if (staff.StaffLeaveData == null)
                            //    return default(IQueryable<CompanyLeaveReport>);

                            //staff.StaffLeaveData = staff.StaffLeaveData
                            //    .Where(
                            //        m =>
                            //            m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                            //            m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date).ToList();

                            #endregion

                            #region Filter LeaveType

                            staff.StaffLeaveData = staff.StaffLeaveData
                          .Where(
                              m =>
                                  m.LeaveDateStart.Date >= DateTime.Parse(endDate).Subtract(new TimeSpan(31, 0, 0, 0)) &&
                                  //m.LeaveDateStart.Date >= DateTime.Parse(endDate).Date &&
                                  m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date &&
                                  m.LeaveType.Equals(1) &&
                                  m.LeaveType != 7 &&
                                   m.LeaveType != 6 &&
                                  m.LeaveStatus.Equals(1)).ToList();

                            #endregion

                        }
                    }
                }

                string leaveType = "";
                var dataStructure = branches.ToList()
                    .ConvertAll(
                        m =>
                            new CompanyLeaveReport(m.BranchId, m.BranchName, m.BranchDivisions, startDate, endDate,
                                leaveType)).AsQueryable();
                //return dataStructure;


                return TimeKeepingContainer.ExportLeaveBalancesToExcel(dataStructure);
            }
        }

        [HttpGet]
        public bool SendEmailsForPendingLeaveAppleacations()
        {

            using (var contextEf = new DataContextEF())
            {
                var leaveData = contextEf.Staff.Include("StaffLeaveData").Include("StaffHoursData").Include("LeaveCounters").Include("StaffDivision.DivisionBranch")
               .Where(x => x.RecordStatus.Equals("Active")).ToList();

                foreach (StaffModel staff in leaveData)
                {
                    if (staff.StaffDivision.DivisionBranch.BranchName.Equals("NFB Short Term"))
                    {
                        foreach (StaffLeaveModel pendingLeaveApplication in staff.StaffLeaveData)
                        {
                            if (pendingLeaveApplication.LeaveStatus == (int)LeaveStatus.Pending)
                            {
                                EmailLeaveApplication(pendingLeaveApplication.LeaveId.ToString());
                            }
                        }
                    }
                }
            }
            return true;
        }


        [HttpPost]
        public bool RemoteClockIn(StaffClockModel remoteClock)
        {
            remoteClock.ClockDateTime = DateTime.Now;
            using (DataContextEF dataContext = new DataContextEF())
            {
                dataContext.StaffClockData.Add(remoteClock);
                dataContext.SaveChanges();
            }
            return true;
        }

        [HttpGet]
        public bool EmailClockRecDenied(string id)
        {
            var recId = int.Parse(id);

            var clockRec = new DataContextEF().StaffClockData.Include("Staff").FirstOrDefault(m => m.ClockDataId.Equals(recId));

            if (clockRec == null)
                return false;

            var staffName = "Dear \t" + clockRec.Staff.StaffFullName;
            var recordHistory = "\t" + clockRec.Comments;

            StringBuilder messageBuilder = new StringBuilder().AppendLine("");
            messageBuilder.AppendFormat(
                "<html><body><p>" + staffName + "</p><p>Your clock record update has been declined. Please see below clock record history</p><p>" + recordHistory + "</p><p>Kind Regards</p><p>Nvest Clocking System</p></body></html>");
            messageBuilder.AppendLine("").ToString();



            var message =
                string.Format(
                    "Dear {0} \n\n Your clock record update has been declined. \n Please see below clock record history. \n\n {1}.  \n\n Kind regards \n Nvest Clocking System",
                    clockRec.Staff.StaffFullName, clockRec.Comments);
            try
            {
                new Emailer
                {
                    subject = "Clock Record Denied",
                    TOList = new List<string>() { clockRec.Staff.StaffEmail },
                    body = messageBuilder.ToString()
                    //body = message
                }.SendEmail();

                return true;

            }
            catch (Exception exception)
            {
                //Log what happened.
                return false;
            }
        }

        [HttpPost]
        public bool UpdateClockRecord(StaffClockModel clockModel)
        {
            try
            {
                using (DataContextEF dataContext = new DataContextEF())
                {
                    dataContext.StaffClockData.Add(clockModel);
                    dataContext.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        [HttpGet]
        public IQueryable<StaffClockingContainer> TimeKeepingDataForLeaveClockRecord(Guid id, string startDate, string endDate)
        {
            //ProcessClockingData();

            using (var store = new DataContextEF())
            {
                var data = from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                           where s.StaffId.Equals(id)
                           select s;

                var staff = data.FirstOrDefault();
                if (staff == null)
                    return default(IQueryable<StaffClockingContainer>);

                staff.StaffClockData =
                    staff.StaffClockData.Where(
                        m =>
                        m.ClockDateTime.Date >= DateTime.Parse(startDate).Date &&
                        m.ClockDateTime.Date <= DateTime.Parse(endDate).Date).ToList();

                return TimeKeepingContainer.GetStaffClockingData(staff).AsQueryable();
            }

        }

        [HttpGet]
        public bool DeleteteLeaveClockRecord(Int32 id)
        {
            try
            {
                using (var store = new DataContextEF())
                {
                    StaffClockModel staffClockModel = store.StaffClockData.First(i => i.ClockDataId.Equals(id));
                    store.StaffClockData.Remove(staffClockModel);
                    store.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;

            }
        }

        [HttpGet]
        public IQueryable<CompanyClockSummariesReport> CompanyStaffClockingSummaries(string startDate, string endDate, string companyId, string divisionId, string staffId, string leaveType)
        {
            using (var store = new DataContextEF())
            {
                var staffclockdata =
                    (from s in store.Staff.Include("StaffClockData").Include("StaffHoursData").Include("StaffLeaveData")
                     select s).ToList();

                var branches = store.Branches.Include("BranchDivisions").Include("BranchDivisions.DivisionStaff").Where(b => b.RecordStatus == "Active").ToList();
                #region Filter Company
                if ((companyId != null) && (Guid.Parse(companyId) != Guid.Empty))
                {
                    branches = branches.FindAll(data => data.BranchId == Guid.Parse(companyId));
                }
                #endregion

                //branches = branches.OrderBy(x => x.BranchDivisions).ToList();
                //branches.ToList().OrderBy(x => x.BranchDivisions.Select(c => c.DivisionName));
                foreach (var branch in branches)
                {
                    #region Filter Divisions
                    if ((divisionId != null) && (Guid.Parse(divisionId) != Guid.Empty))
                    {
                        branch.BranchDivisions = branch.BranchDivisions.ToList().FindAll(m => m.DivisionId.Equals(Guid.Parse(divisionId)));
                    }

                    #endregion
                    foreach (var div in branch.BranchDivisions.ToList().OrderByDescending(x => x.DivisionName).ToList())
                    {
                        #region Filter Staff
                        div.DivisionStaff = div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active") && s.StaffClockData.Any()).ToList();
                        if ((staffId != null) && (Guid.Parse(staffId) != Guid.Empty))
                        {
                            div.DivisionStaff = div.DivisionStaff.Where(data => data.StaffId == Guid.Parse(staffId)).ToList();
                        }
                        #endregion

                        foreach (var staff in div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList())
                        {
                            #region Filter Date
                            if (staff.StaffClockData == null)
                                return default(IQueryable<CompanyClockSummariesReport>);


                            staff.StaffClockData = staff.StaffClockData
                                .Where(
                                    m =>
                                    m.ClockDateTime.Date >= DateTime.Parse(startDate).Date &&
                                    m.ClockDateTime.Date <= DateTime.Parse(endDate).Date).ToList();


                            staff.StaffLeaveData = staff.StaffLeaveData
                              .Where(
                                  m =>
                                  m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                                  m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date).ToList();
                            #endregion
                        }
                    }
                }
                var numberOfDaysInRange = DateTime.Parse(startDate).DifferenceInDays(DateTime.Parse(endDate));
                var dataStructure = branches.ToList().ConvertAll(m => new CompanyClockSummariesReport(m.BranchId, m.BranchName, m.BranchDivisions, numberOfDaysInRange, startDate, endDate)).AsQueryable();
                return dataStructure;
            }
        }

        #endregion
        #region Automatic Weekly Reports

        [HttpGet]
        public bool SendUnApprovedClockRecords()
        {
            IList<StaffModel> myList = new List<StaffModel>();
            foreach (var staffModel in StaffClockModelAllAll())
            {
                var staff = staffModel;
                staff.StaffClockData = new Collection<StaffClockModel>(staffModel.StaffClockData.Where(model => model.DataStatus == 2).ToList());
                myList.Add(staff);
            }

            if (!myList.Any())
            {
                return false;
            }
            else
            {

            }

            return true;
        }

        [HttpGet]
        public bool SendWeekStaffHoursSummary()
        {
            return true;
        }

        #endregion
        #region Web Request

        [HttpPost]
        public bool RequestWebAccess(RequestWebAccessModel model)
        {
            try
            {
                var mailer = new Emailer();

                mailer.TOList.Add(WebConfigurationManager.AppSettings["WebRequestTo"]);
                mailer.CCList.Add(WebConfigurationManager.AppSettings["systemAdminEmail"]);

                mailer.subject = MessageList.WebRequestSubject;
                mailer.body = string.Format(MessageList.WebRequestBody, model.NameSurname, model.WebsiteAddress,
                                            model.Motivation, WebConfigurationManager.AppSettings["systemAdminEmail"], WebConfigurationManager.AppSettings["systemAdminName"], model.NameSurname);
                mailer.SendEmail();

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        #endregion
        #region Phone usage
        #region Public API methods

        #region Old Dialling code
        /*
        [HttpGet]
        public HttpResponseMessage Dial(string ownExt, string dialingExt)
        {
            var phoneIp = "";

            bool isCapeTown = int.Parse(ownExt) >= 500 && int.Parse(ownExt) <= 599;
            bool isDurban = int.Parse(ownExt) >= 600 && int.Parse(ownExt) <= 699;
            bool isEastLondon = int.Parse(ownExt) >= 700 && int.Parse(ownExt) <= 799;
            bool isPortElizabeth = int.Parse(ownExt) >= 900 && int.Parse(ownExt) <= 999;

            if (isCapeTown)
                phoneIp = "172.16.5." + (int.Parse(ownExt) - 500);
            if (isDurban)
                phoneIp = "172.16.6." + (int.Parse(ownExt) - 600);
            if (isEastLondon)
                phoneIp = "172.16.2." + (int.Parse(ownExt) - 700);
            if (isPortElizabeth)
                phoneIp = "172.16.4." + (int.Parse(ownExt) - 900);
            //phoneIp = "192.168.4." + (int.Parse(ownExt) - 900);


            var client = new WebClient();
            var content =
                client.DownloadString("http://" + phoneIp + "?NUMBER=" + dialingExt + "&DIAL=Dial&active_line=1");
            bool dialed = String.IsNullOrEmpty(content);
            return Request.CreateResponse(!dialed ? HttpStatusCode.OK : HttpStatusCode.ExpectationFailed, dialed);
        }
        */
        #endregion


        [HttpGet]
        public string Dial(string ownExt, string dialingExt)
        {
            var phoneIp = "";

            bool isCapeTown = int.Parse(ownExt) >= 500 && int.Parse(ownExt) <= 599;
            bool isDurban = int.Parse(ownExt) >= 600 && int.Parse(ownExt) <= 699;
            bool isEastLondon = int.Parse(ownExt) >= 700 && int.Parse(ownExt) <= 799;
            bool isPortElizabeth = int.Parse(ownExt) >= 900 && int.Parse(ownExt) <= 999;

            if (isCapeTown)
                phoneIp = "172.16.5." + (int.Parse(ownExt) - 500);
            if (isDurban)
                phoneIp = "172.16.6." + (int.Parse(ownExt) - 600);
            if (isEastLondon)
                phoneIp = "172.16.2." + (int.Parse(ownExt) - 700);
            if (isPortElizabeth)
                phoneIp = "172.16.4." + (int.Parse(ownExt) - 900);
            //phoneIp = "192.168.4." + (int.Parse(ownExt) - 900);

            var loHttp = (HttpWebRequest)WebRequest.Create("http://" + phoneIp + "?NUMBER=" + dialingExt + "&DIAL=Dial&active_line=1");
            loHttp.AllowAutoRedirect = true;
            HttpWebResponse loWebResponse = null;
            StreamReader loResponseStream = null;
            String lcHtml;
            try
            {
                loWebResponse = (HttpWebResponse)loHttp.GetResponse();
                Encoding enc = Encoding.UTF8;
                loResponseStream = new StreamReader(loWebResponse.GetResponseStream(), enc);

                // Marker for Breakpoint/bugging
                lcHtml = loResponseStream.ReadToEnd();

                //if (loResponseStream. == HttpStatusCode.OK)
                //{

                //}
                // Need to catch the Profida exceptions:
                // 100 = Error_Not_foundx
                // 200 = Error_Null_Object
                // 300 = Error_Pk_Not_Permitted
                // <error id="300">New person Clarkson, Kelly could not be created, No PK field may be supplied but the XML contained pk = 23514.</error>
            }
            catch (WebException eWeb)
            {
                throw new Exception(String.Format("Web Exception: {0} - {1}", eWeb.Status, eWeb.Message));
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("General Exception: {0}", ex.Message), ex.InnerException);
            }
            finally
            {
                if (loWebResponse != null) loWebResponse.Close();
                if (loResponseStream != null) loResponseStream.Close();
            }


            //var client = new WebClient();
            //var content = client.DownloadString("http://" + phoneIp + "?NUMBER=" + dialingExt + "&DIAL=Dial&active_line=1");
            //bool dialed = String.IsNullOrEmpty(content);


            //return Request.CreateResponse(!dialed ? HttpStatusCode.OK : HttpStatusCode.ExpectationFailed, dialed);
            return lcHtml;
        }


        [HttpGet]
        public IQueryable<StaffPhoneRecord> GetStaffPhoneRecords(string startDate, string endDate)
        {
            var currentStaff = CurrentStaff().FirstOrDefault();
            var staffContacts = new DataContextEF().StaffContactData.Where(m => m.StaffId.Equals(currentStaff.StaffId));

            var phoneRecords = GetStaffCDRData(startDate, endDate, CurrentStaff().FirstOrDefault().StaffTellExt).ToList();

            phoneRecords.ForEach((m) =>
                {
                    m.DisplayDestination = GetContactNameByNumber(m.dst, staffContacts);
                });

            return phoneRecords.OrderByDescending(m => m.calldate).AsQueryable();
        }

        [HttpGet]
        public IQueryable<StaffPhoneRecordDetail> GetStaffCDRSummary(string startDate, string endDate, string extension)
        {
            if (extension == null) return null;

            var phoneRecords = GetStaffCDRData(startDate, endDate, extension);
            var uniqueNum = phoneRecords.Select(m => m.dst).Distinct();
            var staffContacts = new DataContextEF().Staff.Include("StaffContactData")
                .FirstOrDefault(m => m.StaffTellExt.Equals(extension) && m.RecordStatus.Equals("Active"))
                .StaffContactData;

            var cdrSummaryList = (from num in uniqueNum
                                  select new StaffPhoneRecordDetail()
                                      {
                                          CallCount = phoneRecords.Count(m => m.dst.Equals(num)),
                                          TotalCallCost = phoneRecords.Where(m => m.dst.Equals(num)).Sum(m => m.CallCost),
                                          DisplayDestination = GetContactNameByNumber(num, staffContacts),
                                          Destination = num,
                                          CallDuration = phoneRecords.Where(m => m.dst.Equals(num)).Sum(m => m.billsec),
                                      }).ToList();

            return cdrSummaryList.OrderByDescending(m => m.TotalCallCost).AsQueryable();
        }

        public IQueryable<StaffPhoneRecord> GetCDRSummary(string startDate, string endDate, string extension, string destination)
        {
            var staffContacts = new DataContextEF().Staff.Include("StaffContactData")
                .FirstOrDefault(m => m.StaffTellExt.Equals(extension) && m.RecordStatus.Equals("Active"))
                .StaffContactData;

            var data = GetStaffCDRData(startDate, endDate, extension).Where(m => m.dst.Equals(destination)).ToList();

            data.ForEach(
                (m) =>
                {
                    m.DisplayDestination = GetContactNameByNumber(destination, staffContacts);
                });

            return data.OrderByDescending(m => m.CallCost).AsQueryable();
        }

        [HttpGet]
        public IQueryable<CompanyReportModel> GetCompanyCDR(string startDate, string endDate, string companyId, string divisionId, string staffId, string carrier)
        {
            var data = CDRAccessLayer.CDRData(DateTime.Parse(startDate), DateTime.Parse(endDate)
                , string.IsNullOrEmpty(companyId) ? (Guid?)null : Guid.Parse(companyId)
                , string.IsNullOrEmpty(divisionId) ? (Guid?)null : Guid.Parse(divisionId)
                , string.IsNullOrEmpty(staffId) ? (Guid?)null : Guid.Parse(staffId)
                , carrier);

            //var data = GetStaffCDRData(DateTime.Parse(startDate), DateTime.Parse(endDate), Guid.Parse(companyId), Guid.Parse(divisionId), Guid.Parse(staffId), "");

            //return default(IQueryable<CompanyReportModel>);

            return data.ToList()
                    .ConvertAll(m => new CompanyReportModel(m.BranchId, m.BranchName, m.BranchDivisions))
                    .AsQueryable();
        }


        [HttpGet]
        public IQueryable<CompanyClockSummariesReport> GetIncompleteClockData(string startDate, string endDate, string companyId, string divisionId, string staffId, string leaveType)
        {

            return default(IQueryable<CompanyClockSummariesReport>);
        }


        [HttpGet]
        public IQueryable<CompanyLeaveReport> CompanyStaffLeave(string startDate, string endDate, string companyId, string divisionId, string staffId, string leaveType)
        {
            using (var store = new DataContextEF())
            {
                var leavedata =
                    store.Staff.Include("StaffLeaveData")
                     .ToList();

                var branches = store.Branches.Include("BranchDivisions").Include("BranchDivisions.DivisionStaff").Where(b => b.RecordStatus == "Active").ToList();
                #region Filter Company
                if ((companyId != null) && (Guid.Parse(companyId) != Guid.Empty))
                {
                    branches = branches.FindAll(data => data.BranchId == Guid.Parse(companyId));
                }
                #endregion
                foreach (var branch in branches)
                {
                    #region Filter Divisions
                    if ((divisionId != null) && (Guid.Parse(divisionId) != Guid.Empty))
                    {
                        branch.BranchDivisions = branch.BranchDivisions.ToList().FindAll(m => m.DivisionId.Equals(Guid.Parse(divisionId)));
                    }

                    #endregion
                    foreach (var div in branch.BranchDivisions.ToList().Where((x => x.DivisionStaff.Count > 0)))
                    {
                        #region Filter Staff
                        div.DivisionStaff = div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList();
                        if ((staffId != null) && (Guid.Parse(staffId) != Guid.Empty))
                        {
                            div.DivisionStaff = div.DivisionStaff.Where(data => data.StaffId == Guid.Parse(staffId)).ToList();
                        }
                        #endregion

                        //foreach (var staff in div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList())
                        foreach (var staff in div.DivisionStaff)
                        {

                            #region Filter Date
                            if (staff.StaffLeaveData == null)
                                continue;

                            //if (staff.StaffLeaveData == null)
                            //    return default(IQueryable<CompanyLeaveReport>);

                            //staff.StaffLeaveData = staff.StaffLeaveData
                            //    .Where(
                            //        m =>
                            //        m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                            //        m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date).ToList();
                            #endregion
                            #region Filter LeaveType
                            //if (leaveType != null)
                            //{
                            staff.StaffLeaveData = staff.StaffLeaveData
                           .Where(
                               m =>
                                   m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                                   m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date &&
                                   //m.LeaveType.Equals(1) &&
                                    m.LeaveStatus.Equals((int)LeaveStatus.Approved)
                                /*m.LeaveType != 7*/).ToList();
                            // //}

                            #endregion

                        }
                    }
                }

                var dataStructure = branches.ToList()
                    .ConvertAll(m => new CompanyLeaveReport(m.BranchId, m.BranchName, m.BranchDivisions, startDate, endDate, leaveType)).AsQueryable();
                return dataStructure;
            }
        }

        [HttpGet]
        public IQueryable<CompanyLeaveReport> CompanyStaffLeaveBalance(string startDate, string endDate, string companyId, string divisionId, string staffId)
        {
            using (var store = new DataContextEF())
            {
                var leavedata =
                    store.Staff.Include("StaffLeaveData")
                     .ToList();

                var branches = store.Branches.Include("BranchDivisions").Include("BranchDivisions.DivisionStaff").Where(b => b.RecordStatus == "Active").ToList();
                #region Filter Company
                if ((companyId != null) && (Guid.Parse(companyId) != Guid.Empty))
                {
                    branches = branches.FindAll(data => data.BranchId == Guid.Parse(companyId));
                }
                #endregion
                foreach (var branch in branches)
                {
                    #region Filter Divisions
                    if ((divisionId != null) && (Guid.Parse(divisionId) != Guid.Empty))
                    {
                        branch.BranchDivisions = branch.BranchDivisions.Where(x => x.RecordStatus.Equals("Active")).ToList().FindAll(m => m.DivisionId.Equals(Guid.Parse(divisionId)));
                    }

                    #endregion
                    foreach (var div in branch.BranchDivisions.ToList())
                    {
                        #region Filter Staff
                        div.DivisionStaff = div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active") && s.StaffIsClockingMember.Equals(true)).OrderBy(x => x.StaffFullName).ToList();
                        if ((staffId != null) && (Guid.Parse(staffId) != Guid.Empty))
                        {
                            div.DivisionStaff = div.DivisionStaff.Where(data => data.StaffId == Guid.Parse(staffId)).ToList();
                        }
                        #endregion

                        //foreach (var staff in div.DivisionStaff.Where(s => s.RecordStatus.Equals("Active")).ToList())
                        foreach (var staff in div.DivisionStaff)
                        {

                            #region Filter Date
                            if (staff.StaffLeaveData == null)
                                continue;

                            //if (staff.StaffLeaveData == null)
                            //    return default(IQueryable<CompanyLeaveReport>);

                            //staff.StaffLeaveData = staff.StaffLeaveData
                            //    .Where(
                            //        m =>
                            //        m.LeaveDateStart.Date >= DateTime.Parse(startDate).Date &&
                            //        m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date).ToList();
                            #endregion
                            #region Filter LeaveType
                            //if (leaveType != null)
                            //{
                            staff.StaffLeaveData = staff.StaffLeaveData
                           .Where(
                               m =>
                                   m.LeaveDateStart.Date >= DateTime.Parse(endDate).Subtract(new TimeSpan(31, 0, 0, 0)) &&
                                   //m.LeaveDateStart.Date >= DateTime.Parse(endDate).Date &&
                                   m.LeaveDateEnd.Date <= DateTime.Parse(endDate).Date &&
                                   m.LeaveType.Equals(1) &&
                                   m.LeaveType != 7 &&
                                    m.LeaveType != 6 &&
                                   m.LeaveStatus.Equals(1)).ToList();
                            //}

                            #endregion

                        }
                    }
                }

                string leaveType = "";
                var dataStructure = branches.ToList()
                    .ConvertAll(m => new CompanyLeaveReport(m.BranchId, m.BranchName, m.BranchDivisions, startDate, endDate, leaveType)).AsQueryable();
                return dataStructure;
            }
        }



        #endregion
        #region Private helper methods
        private static IList<StaffPhoneRecord> GetStaffCDRData(string startDate, string endDate, string ext)
        {
            return CDRAccessLayer.CDRData(DateTime.Parse(startDate), DateTime.Parse(endDate), ext);
        }

        private static IList<BranchModel> GetStaffCDRData(DateTime dateFrom, DateTime dateTo, Guid? companyId, Guid? divisionId, Guid? staffId, String Carrier)
        {
            return CDRAccessLayer.CDRData(dateFrom, dateTo, companyId, divisionId, staffId, "").ToList();
        }

        private static string GetContactNameByNumber(string number, IEnumerable<StaffContactModel> contactList)
        {
            var staffContactModels = contactList ?? null;
            if (contactList == null) { return number; }
            return staffContactModels.Count(m => m.ContactNumber.Equals(number)) == 1
                       ? staffContactModels.FirstOrDefault(m => m.ContactNumber.Equals(number)).ContactFullName
                       : number;
        }
        private static double GetCallCostTotal(IEnumerable<StaffPhoneRecord> callRecords)
        {
            if (callRecords == null)
                return 0;

            return (from m in callRecords
                    select m.CallCost).Sum();

        }
        #endregion
        #endregion
        #region Printer/Toner orders

        [HttpGet]
        public bool SavePrinter(string printerData, Guid printerId)
        {
            if (printerData != "" && printerData != "[]" && printerId != Guid.Empty)
            {


                printerData = printerData.Replace("[", "");
                printerData = printerData.Replace("]", "");
                printerData = printerData.Replace("\\", "");
                printerData = printerData.Replace("\"\"", "\"");

                IList<PrinterPropertiesPrinterModel> printerProperties = new List<PrinterPropertiesPrinterModel>();
                var propertyIds = printerData.Split(',');

                IList<Guid> listOfGuid = new List<Guid>();
                foreach (string propertyId in propertyIds)
                {
                    var test = propertyId.Trim();
                    var test2 = propertyId.Replace("\"", " ");
                    Guid entry = Guid.Parse(test2);
                    listOfGuid.Add(entry);
                }

                for (int i = 0; i < listOfGuid.Count; i++)
                {
                    var printers = new PrinterPropertiesPrinterModel
                    {
                        PrinterId = printerId,
                        PropertyId = listOfGuid[i],
                        RecordStatus = "Active"
                    };
                    printerProperties.Add(printers);
                }

                try
                {
                    using (var contextEf = new DataContextEF())
                    {
                        var printerProp = contextEf.PrinterPropertiesPrinter.Where(x => x.PrinterId.Equals(printerId)).ToList();

                        foreach (PrinterPropertiesPrinterModel toDelete in printerProp)
                        {
                            contextEf.PrinterPropertiesPrinter.Remove(toDelete);
                        }


                        foreach (PrinterPropertiesPrinterModel printerPropertiesPrinterModel in printerProperties)
                        {
                            contextEf.PrinterPropertiesPrinter.Add(printerPropertiesPrinterModel);
                        }
                        contextEf.SaveChanges();
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        [HttpGet]
        public IQueryable<PrinterModel> GetAllPrinters()
        {
            return ctx.Context.Printers.Where(x => x.RecordStatus.Equals("Active")).OrderBy(x => x.PrinterDescription);
        }



        [HttpGet]
        public IQueryable<PrinterModel> Printers()
        {
            var activeStatus = RecordStatusEnum.Active.ToString();
            return ctx.Context.Printers.Where(m => m.RecordStatus.Equals(activeStatus));
        }

        [HttpGet]
        public IQueryable<PrinterServiceProviderModel> PrinterServiceProviders()
        {
            var activeStatus = RecordStatusEnum.Active.ToString();
            return ctx.Context.PrinterServiceProviders;
        }

        [HttpGet]
        public IQueryable<TonerOrderDetailsModel> OrderDetailsModels()
        {
            var recStatus = RecordStatusEnum.Active.ToString();
            return ctx.Context.TonerOrderDetails.Where(m => m.RecordStatus.Equals(recStatus));
        }


        [HttpGet]
        public IQueryable<PrinterPropertyModel> GetAllPrinterProperties()
        {
            return ctx.Context.PrinterProperties.OrderBy(x => x.PropertyDescription);
        }

        [HttpGet]
        public IQueryable<PrinterPropertiesPrinterModel> GetPropertiesOfPrinter(Guid printerId)
        {
            return ctx.Context.PrinterPropertiesPrinter.Where(x => x.PrinterId.Equals(printerId)).OrderBy(x => x.PropertyId).AsQueryable();
        }

        [HttpGet]
        public IQueryable<PropertiesDto> GetPrinterProperties(string printerId)
        {
            var printId = Guid.Parse(printerId);

            if (printId == null)
                return null;

            //todo jay- make this part of the query below (E.F)
            const int openOrderStatus = (int)OrderStatus.Opened;

            var data =
                ctx.Context.PrinterPropertiesPrinter.Include("Property")
                     .Where(m => m.PrinterId.Equals(printId))
                     .ToList()
                     .ConvertAll(m => new PropertiesDto(m.PropertyId, m.Property.PropertyDescription)).ToList().AsQueryable();

            //todo - jay: change this code when there are perfomance issue
            // this will also depend on the business rules
            foreach (var propertiesDto in data)
            {
                var t = from s in ctx.Context.TonerOrderDetails
                        where s.PrinterId.Equals(printId) && s.PropertyId.Equals(propertiesDto.ColourId) && s.OrderStatus.Equals(openOrderStatus)
                        select s;
                propertiesDto.Order = t.Any();
            }
            return data;
        }

        [HttpGet]
        public IQueryable<TonerOrdersModel> TonerOrders()
        {
            return ctx.Context.TonerOrders;
        }

        [HttpGet]
        public IQueryable<TonerOrdersDto> GetCurrentOrders()
        {
            using (var store = new DataContextEF())
            {
                const int openOrders = (int)OrderStatus.Opened;

                var data = store.TonerOrderDetails.Include("TonerOrder.Staff")
                             .Include("PrinterProperty.Printer")
                             .Include("PrinterProperty.Property")
                             .Where(m => m.OrderStatus.Equals(openOrders));

                return data.ToList().ConvertAll(m => new TonerOrdersDto
                                                         (m.DetailsId, m.PrinterId, m.PropertyId,
                                                          m.PrinterProperty.Printer.SerialNumber,
                                                          m.PrinterProperty.Property.PropertyDescription,
                                                          m.TonerOrder.Staff.StaffFullName,
                                                          m.TonerOrder.OrderDate)).AsQueryable();
            }
        }

        [HttpPost]
        public bool OrderTonerEmail(IEnumerable<TonerOrderDetailsModel> model)
        {
            if (!model.Any())
                return false;

            var printerId = model.FirstOrDefault();
            var printer = new DataContextEF().Printers.Include("PrinterProvider").Where(m => m.PrinterId.Equals(printerId.PrinterId));
            return printer.Any() && UoWTonerManager.OrderToner(printer.FirstOrDefault(), CurrentStaff().FirstOrDefault().StaffFullName, model.ToList());
        }

        [HttpPost]
        public bool SuggestionSubscription(List<SuggestionModel> followers)
        {
            var lastItem = followers.LastOrDefault();

            const string message = "Following a suggestion.....Ignore Testing here"; //TODO:rephrase
            try
            {
                for (var i = 0; i < followers.Count(); i++)
                {
                    new Emailer
                    {
                        subject = "Intranet Suggestion Following",
                        TOList = new List<string>() { followers[i].Staff.StaffEmail },//followers[i].StaffEmail
                        body = message
                    }.SendEmail();
                }
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        [HttpGet]
        public IQueryable<StaffModel> StaffList1(Guid staffId)
        {
            return ctx.Context.Staff
                      .Where(x => x.StaffId == staffId)
                      .OrderByDescending(x => x.StaffSurname);
        }

        [HttpPost]
        public bool SuggestionSubscription1(SuggestionModel followers)
        {
            List<StaffSuggestionFollower> followerIds = followers.SuggestionFollowers;

            if (followers.SuggestionFollowers.Count > 0 && followers.SuggestionFollowers[0].SuggestionID.Equals(Guid.Empty))
            {
                followers.SuggestionId = Guid.NewGuid();
            }

            followers.SuggestionDate = DateTime.Now;
            followers.RecordStatus = "Active";

            for (var i = 0; i < followerIds.Count; i++)
            {
                followerIds[i].SuggestionID = followers.SuggestionId;
                var getFollowerEmail = StaffList1(followerIds[i].StaffID).Select(x => x.StaffEmail).ToList();
                //var getFollowerName = StaffList1(followerIds[i].StaffID).Select(x => x.StaffFullName).ToList();
                var url = "http://Intranet/#/view_suggestion/" + followers.SuggestionId;
                StringBuilder urlBuilder = new StringBuilder().AppendLine("");
                urlBuilder.AppendFormat(
                    "<html><body><p>Dear Staff member </p><p> Click link to see the suggestion: <a href= " + url + ">here</a></p></body><p>Kind Regards</p><p>NVest Intranet</p></html>");
                urlBuilder.AppendLine("").ToString();


                new Emailer
                {
                    subject = "You have been tagged in a suggestion",
                    TOList = new List<string>() { getFollowerEmail[0] }, //"lmarumbwa@nvestholdings.co.za" followers[i].StaffEmail
                    body = urlBuilder.ToString()
                }.SendEmail();
            }

            using (var dataContext = new DataContextEF())
            {
                SuggestionModel suggestiontoupdate = dataContext
                                                    .Suggestions
                                                    .Include("SuggestionFollowers")
                                                    .FirstOrDefault(m => m.SuggestionId.Equals(followers.SuggestionId));

                if (suggestiontoupdate == null)
                {
                    dataContext.Suggestions.Add(new SuggestionModel()
                    {
                        RecordStatus = followers.RecordStatus,
                        Staff = followers.Staff,
                        StaffId = followers.StaffId,
                        SuggestionDate = followers.SuggestionDate,
                        SuggestionFollowers = followers.SuggestionFollowers,
                        SuggestionId = followers.SuggestionId,
                        SuggestionSubject = followers.SuggestionSubject,
                        SuggestionText = followers.SuggestionText,
                        Votes = followers.Votes,
                        SuggestionStatus = "Pending",
                        SuggestionResponse = "No response yet!"
                    });
                }
                else
                {
                    suggestiontoupdate.RecordStatus = followers.RecordStatus;
                    suggestiontoupdate.Staff = followers.Staff;
                    suggestiontoupdate.StaffId = followers.StaffId;
                    suggestiontoupdate.SuggestionDate = followers.SuggestionDate;
                    suggestiontoupdate.SuggestionFollowers = followers.SuggestionFollowers;
                    suggestiontoupdate.SuggestionSubject = followers.SuggestionSubject;
                    suggestiontoupdate.SuggestionText = followers.SuggestionText;
                    suggestiontoupdate.Votes = followers.Votes;
                }
                dataContext.SaveChanges();
            }
            return true;
        }

        #endregion
        #region Roles

        //[HttpGet]
        //public IQueryable<UserRolesModel> UserRoles()
        //{
        //    return ctx.Context.UserRoles;
        //}

        //[HttpGet]
        //public IQueryable<RolesModel> GetRoles()
        //{
        //    return ctx.Context.Roles;
        //}

        [HttpGet]
        public bool CurrentUserInRole(int roleId)
        {
            var staff = CurrentStaff().FirstOrDefault();
            return CurrentUserRoles().Any(m => m.Equals(roleId));
        }


        [HttpGet]
        public IQueryable<int> CurrentUserRoles()
        {
            return GetUserRoles().AsQueryable();
        }

        #region Active directory region EL
        /*  private IEnumerable<int> GetUserRoles()
        {
            var currentUser = CurrentUser();

            //establish domain context
            var domainContext = new PrincipalContext(ContextType.Domain, "Pacific");


            //find the user
            var user = UserPrincipal.FindByIdentity(domainContext, currentUser);

            //if user exists, get its users
            if (user == null)
                return null;

            var groups = user.GetAuthorizationGroups();

            // Roles from AD
            var result = groups.Where(principal => principal != null).Where(p => p is GroupPrincipal).Cast<GroupPrincipal>().ToList();

            // Enum list from C# class.
            var enumList = EnumHelper.ConvertEnumToList<CustomRoles>();

            return (from enumVal in enumList let value = EnumHelper.GetEnumDescriptions(enumVal) where result.Any(m => m.Name.ToLower().Equals(value.ToLower())) select (int)enumVal).ToList();

        } */
        #endregion

        #region Active directory region PE
        /**/
        private IEnumerable<int> GetUserRoles()
        {
            #region
            var currentUser = CurrentUser();

            //establish domain context
            var domainContext = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings["DomainServer"]); //Pacific //"192.168.16.2"

            //find the user
            var user = UserPrincipal.FindByIdentity(domainContext, currentUser);


            //if user exists, get its users
            if (user == null)
                return null;

            var groups = user.GetGroups();

            // Roles from AD
            var result =
                groups.Where(principal => principal != null).Where(p => p is GroupPrincipal).Cast<GroupPrincipal>().ToList();

            // Enum list from C# class.
            var enumList = EnumHelper.ConvertEnumToList<CustomRoles>();

            var test = (from enumVal in enumList
                        let value = EnumHelper.GetEnumDescriptions(enumVal)
                        where result.Any(m => m.Name.ToLower().Equals(value.ToLower()))
                        select (int)enumVal).ToList();

            return test;

            #endregion
        }

        #endregion

        //#region Active directory region PE
        ///**/
        //private IEnumerable<int> GetUserRoles()
        //{
        //    #region
        //    var currentUser = CurrentUser();

        //    //establish domain context
        //    var domainContext = new PrincipalContext(ContextType.Domain, "192.168.16.2"); //Pacific

        //    //find the user
        //    var user = UserPrincipal.FindByIdentity(domainContext, currentUser);


        //    //if user exists, get its users
        //    if (user == null)
        //        return null;

        //    var groups = user.GetGroups();

        //    // Roles from AD
        //    var result =
        //        groups.Where(principal => principal != null).Where(p => p is GroupPrincipal).Cast<GroupPrincipal>().ToList();


        //    //foreach (Principal principal in groups)
        //    //{

        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + groups.Count() + ".txt");
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + principal.Name + ".txt");
        //    //}

        //    //foreach (GroupPrincipal groupPrincipal in result)
        //    //{
        //    //    //File.Create(AppDomain.CurrentDomain.BaseDirectory + result.Count + ".txt");
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + Guid.NewGuid() + ".txt");
        //    //}


        //    // Enum list from C# class.
        //    var enumList = EnumHelper.ConvertEnumToList<CustomRoles>();

        //    //var testGroupsThatXolisaIsPartOf = from s in result
        //    //                                   where result.Any(m => m.Name.Equals(groups))
        //    //                                   select s.Name.ToList();

        //    //foreach (var VARIABLE in testGroupsThatXolisaIsPartOf)
        //    //{
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + Guid.NewGuid() + ".txt");
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + VARIABLE + ".txt");
        //    //}

        //    var test = (from enumVal in enumList
        //                let value = EnumHelper.GetEnumDescriptions(enumVal)
        //                where result.Any(m => m.Name.ToLower().Equals(value.ToLower()))
        //                select (int)enumVal).ToList();
        //    //foreach (int i in test)
        //    //{
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + Guid.NewGuid() + ".txt");
        //    //    File.Create(AppDomain.CurrentDomain.BaseDirectory + i + ".txt");
        //    //}
        //    return test;

        //    #endregion
        //}

        #endregion

        #endregion
        #region Messages
        [HttpGet]
        public IQueryable<MessagesModel> Messages()
        {
            return ctx.Context.Messages.Where(m => m.RecordStatus.Equals("Active"));
        }
        #endregion
        #region Leave Applications


        [HttpGet]
        public IEnumerable<string> GetPublicHolidays()
        {
            var value = DateTime.Now;
            var publicHols = new List<DateTime>
                {
                    new DateTime(value.Year, 1, 1), //New Years
                    new DateTime(value.Year, 3, 21), //Human rights day
                    new DateTime(value.Year, 3, 25),
                    new DateTime(value.Year, 3, 28), //Public holiday
                    new DateTime(value.Year, 4, 27), //Good friday
                  //new DateTime(value.Year, 5, 1), //Workers day
                    new DateTime(value.Year, 5, 2), //Workers day
                    new DateTime(value.Year, 6, 16), //Youth day
                    new DateTime(value.Year, 8, 8), //Heritage day
                    new DateTime(value.Year, 9, 24), //Heritage day
                    new DateTime(value.Year, 12, 16), //day of reconciliation
                    new DateTime(value.Year, 12, 25), //Day of Goodwill//Christmas day
                    new DateTime(value.Year, 12, 26), //Day of Goodwill
                };

            return publicHols.Select(x => x.ToShortDateString());

        }

        [HttpGet]
        public bool AddOctoberIncrement()
        {
            var count = 0;
            using (var dtx = new DataContextEF())
            {
                var data = dtx.Staff.Include("LeaveCounters").Where(x => x.RecordStatus.Equals("Active") && x.StaffNtName != null).ToList();

                if (!data.Any())
                    return false;

                foreach (StaffModel staffModel in data)
                {

                    var leaveCounter =
                        staffModel.LeaveCounters.FirstOrDefault(x => x.RecordStatus.Equals("Active"));


                    if (leaveCounter == null)
                    {
                        var test2 = staffModel.StaffFullName;
                        continue;
                    }

                    count++;

                    staffModel.LeaveScheduleCarriedOver = (staffModel.LeaveScheduleCarriedOver + leaveCounter.Accumulator).Round(2);
                }
                var test = count;
                dtx.SaveChanges();
            }
            return true;
        }

        //[HttpGet]
        //public bool EmailChriss400Emails()
        //{
        //    var emailHr = new SmtpClient();
        //    var mailMeassage = new MailMessage();
        //    mailMeassage.To.Add(new MailAddress("choole@nvestholdings.co.za"));
        //    mailMeassage.Subject = "Amendment made to staff join date.";
        //    for (int i = 0; i < 201; i++)
        //    {

        //        mailMeassage.Body = i.ToString();
        //        mailMeassage.IsBodyHtml = true;
        //        emailHr.Send(mailMeassage);
        //    }
        //    return true;
        //}


        [HttpGet]
        public bool ApplyLeaveForEveryone()
        {
            using (var contextEf = new DataContextEF())
            {
                var leaveData = contextEf.Staff.Include("StaffClockData").Include("StaffHoursData").Include("LeaveCounters").Include("StaffDivision.DivisionBranch")
                .Where(x => x.RecordStatus.Equals("Active")).ToList();

                foreach (StaffModel staff in leaveData)
                {
                    if (staff.StaffDivision.DivisionBranch.BranchName.Equals("NFB Short Term"))
                    {
                        continue;
                    }

                    if (staff.StaffDivision.DivisionBranch.BranchName.Equals("NVest Securities"))
                    {
                        continue;
                    }

                    if (staff.RecordStatus.Equals("Deleted"))
                    {
                        continue;
                    }

                    if (staff.StaffDesignation == null)
                    {
                        continue;
                    }

                    if (staff.StaffEmail == null)
                    {
                        continue;
                    }

                    var staffHoursModel = staff.StaffHoursData.FirstOrDefault(m => m.DayId.Equals(4));
                    if (staffHoursModel == null)
                        continue;

                    //var clock1 = new StaffClockModel()
                    //{
                    //    ClockDateTime = new DateTime(2015, 12, 24, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0),
                    //    OriginalClockDateTime = new DateTime(2015, 12, 24, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0),
                    //    Comments = "Special Day 1 in December closing period in record",
                    //    LeaveType = (int)LeaveType.Annual,
                    //    RecordStatus = "Active",
                    //    StaffId = staff.StaffId,
                    //};
                    //contextEf.StaffClockData.Add(clock1);


                    //var clock2 = new StaffClockModel()
                    //{
                    //    ClockDateTime = new DateTime(2015, 12, 24, staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0),
                    //    OriginalClockDateTime = new DateTime(2015, 12, 24, staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0),
                    //    Comments = "Special Day 1 in December closing period out record",
                    //    LeaveType = (int)LeaveType.Annual,
                    //    RecordStatus = "Active",
                    //    StaffId = staff.StaffId,
                    //};
                    //contextEf.StaffClockData.Add(clock2);

                    //var clock3 = new StaffClockModel()
                    //{
                    //    ClockDateTime = new DateTime(2015, 12, 28, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0),
                    //    OriginalClockDateTime = new DateTime(2015, 12, 28, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0),
                    //    Comments = "Special Day 2 in December closing period in record",
                    //    LeaveType = (int)LeaveType.Annual,
                    //    RecordStatus = "Active",
                    //    StaffId = staff.StaffId,
                    //};
                    //contextEf.StaffClockData.Add(clock3);


                    //var clock4 = new StaffClockModel()
                    //{
                    //    ClockDateTime = new DateTime(2015, 12, 28, staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0),
                    //    OriginalClockDateTime = new DateTime(2015, 12, 28, staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0),
                    //    Comments = "Special Day 2 in December closing period out record",
                    //    LeaveType = (int)LeaveType.Annual,
                    //    RecordStatus = "Active",
                    //    StaffId = staff.StaffId,
                    //};
                    //contextEf.StaffClockData.Add(clock4);

                    var leave3 = new StaffLeaveModel()
                    {
                        LeaveDateStart = DateTime.Parse("2015-12-29 08:00:00.000"),
                        LeaveDateEnd = DateTime.Parse("2015-12-31 17:00:00.000"),
                        LeaveComments = "Compulsory 3 days leave in December closing period",
                        LeaveType = (int)LeaveType.Annual,
                        LeaveStatus = (int)LeaveStatus.Approved,
                        LeaveRequestDate = DateTime.Now,
                        RecordStatus = "Active",
                        ApprovedBy1 = staff.StaffManager1Id,
                        ApprovedBy2 = staff.StaffManager2Id,
                        StaffId = staff.StaffId,
                        LeaveId = Guid.NewGuid()
                    };
                    leave3 = ValidateLeaveApplication(leave3);
                    contextEf.StaffLeaveData.Add(leave3);
                }
                contextEf.SaveChanges();
            }
            return true;
        }


        [HttpGet]
        public bool DeductIncrementFromOpenningBalance()
        {
            using (var contextEf = new DataContextEF())
            {
                var allStaff = contextEf.Staff.Include("StaffLeaveData").Include("LeaveCounters").Where(x => x.StaffIsClockingMember.Equals(true) && x.RecordStatus.Equals("Active")).ToList();

                foreach (StaffModel staffModel in allStaff)
                {
                    staffModel.LeaveScheduleCarriedOver = staffModel.LeaveScheduleCarriedOver - (double)staffModel.StaffLeaveIncrement;
                }
                contextEf.SaveChanges();
            }
            return true;
        }


        [HttpGet]
        public bool SaveClockRecordsForEveryone()
        {
            using (var contextEf = new DataContextEF())
            {
                var leaveData = contextEf.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").Include("LeaveCounters").Include("StaffDivision.DivisionBranch")
                .Where(x => x.RecordStatus.Equals("Active")).ToList();

                foreach (var staffModel in leaveData.First().StaffLeaveData)
                {
                    var clock1 = new StaffClockModel()
                    {
                        ClockDateTime = staffModel.LeaveDateStart,
                        OriginalClockDateTime = new DateTime(),
                        Comments = "On Leave",
                        LeaveType = (int)LeaveType.Annual,
                        RecordStatus = "Active",
                        StaffId = staffModel.StaffId,
                    };

                }


                contextEf.SaveChanges();
            }
            return true;
        }

        public static double RoundUp(double input, int places)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(places));
            return Math.Ceiling(input * multiplier) / multiplier;
        }
        decimal IncrementLastDigit(decimal value)
        {
            int[] bits1 = decimal.GetBits(value);
            int saved = bits1[3];
            bits1[3] = 0;   // Set scaling to 0, remove sign
            int[] bits2 = decimal.GetBits(new decimal(bits1) + 1);
            bits2[3] = saved; // Restore original scaling and sign
            return new decimal(bits2);
        }
        private DateTime AddWeekendDaysAsWeekDays(DateTime startDate, DateTime endDate)
        {
            var days = 0;
            while (startDate <= endDate)
            {
                if (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    days++;
                }
                startDate = startDate.AddDays(1);
            }
            return endDate.Add(new TimeSpan(days, 0, 0, 0));
            //return endDate.AddDays(days);
        }

        [HttpGet]
        public IQueryable<StaffModel> AllStaff()
        {
            return ctx.Context.Staff.Include("StaffHoursData")
                .Where(x => x.StaffIsClockingMember.Equals(true))
                .Where(x => x.RecordStatus.Equals("Active")).OrderBy(x => x.StaffName);
        }
        [HttpGet]
        public IQueryable<StaffModel> LeaveDataSummary()
        {
            return ctx.Context.Staff
                .Where(x => x.RecordStatus.Equals("Active"));
        }
        [HttpGet]
        public IQueryable<StaffLeaveModel> StaffLeave(string id)
        {
            Guid staffId = Guid.Parse(id);
            return ctx.Context.StaffLeaveData
                .Where(x => x.StaffId.Equals(staffId) && x.StaffMember.StaffIsClockingMember.Equals(true))
                .Where(x => x.RecordStatus.Equals("Active")).OrderByDescending(x => x.StaffMember.StaffSurname).ThenByDescending(x => x.LeaveDateStart);
        }


        [HttpGet]
        public IQueryable<StaffLeaveModel> StaffLeaveAll()
        {
            var data = ctx.Context.StaffLeaveData
                .Where(x => x.StaffMember.StaffIsClockingMember.Equals(true))
                .Where(x => x.RecordStatus.Equals("Active"));

            if (!data.Any())
                return default(IQueryable<StaffLeaveModel>);

            data = data.OrderBy(x => x.StaffMember.StaffName).ThenBy(x => x.LeaveDateStart);

            return data;
        }




        [HttpGet]
        public bool UpdateButKeepOriginalClockRecord(StaffClockModel id)
        {
            using (var contextEf = new DataContextEF())
            {
                contextEf.StaffClockData.Add(id);
                contextEf.SaveChanges();
                return true;
            }
        }


        [HttpGet]
        public IQueryable<StaffLeaveModel> DirectorsLeave()
        {
            var data = ctx.Context.StaffLeaveData.Include("StaffMember").Where(x => x.StaffMember.IsDirector.Equals(true));

            if (!data.Any())
                return default(IQueryable<StaffLeaveModel>);

            data = data.OrderBy(x => x.StaffMember.StaffName).ThenBy(x => x.LeaveDateStart);

            return data;
        }


        //[HttpGet]
        //public IQueryable<CompanyLeaveReport> StaffLeave2()
        //{
        //    using (var store = new DataContextEF())
        //    {
        //        var branches =
        //            store.Branches.Include("BranchDivisions")
        //                .Include("BranchDivisions.DivisionStaff") 
        //                .Where(b => b.RecordStatus == "Active")
        //                .ToList();

        //        var dataStructure =
        //            branches.ToList()
        //                .ConvertAll(m => new CompanyLeaveReport(m.BranchId, m.BranchName, m.BranchDivisions))
        //                .AsQueryable();

        //        return dataStructure;

        //    }
        //}

        [HttpGet]
        public ClockDeviceModel GetClockDeviceModel()
        {
            using (var contextEf = new DataContextEF())
            {
                Guid deviceId = Guid.Parse("C2A1656A-4105-4F72-9B69-EAF3499F8458");
                var data = contextEf.ClockDeviceData.Where(x => x.ClockDeviceId.Equals(deviceId)).First();
                return data;
            }
        }



        public static StaffClockModel ValidateClockModel(StaffClockModel clockData)
        {
            using (var contextEf = new DataContextEF())
            {
                var leaveData = contextEf.Staff.Include("StaffClockData").Include("StaffHoursData").Include("LeaveCounters").Include("StaffDivision.DivisionBranch")
                .FirstOrDefault(x => x.StaffId.Equals(clockData.StaffId) && x.RecordStatus.Equals("Active"));

                var clockModel = (StaffModel)leaveData;

                if (clockModel.StaffDivision.DivisionBranch.BranchName.Equals("NFB Cape Town") || clockModel.StaffDivision.DivisionBranch.BranchName.Equals("NFB Port Elizabeth"))
                {
                    clockData.DataStatus = 0;
                }

                contextEf.SaveChanges();
            }
            return clockData;
        }

        public static bool RegisterUserToClockingDevice(StaffModel staff)
        {
            using (var contextEf = new DataContextEF())
            {
                Guid deviceId = Guid.Empty;
                if (staff.ClockDevice != null)
                {
                    deviceId = (Guid)staff.ClockDevice;
                }
                var data = contextEf.ClockDeviceData.First(x => x.ClockDeviceId.Equals(deviceId));
                bool result = UoWStaff.RegisterUserToClockingDevice(staff, data.ClockDeviceIp, data.DeviceNumber);
                if (result)
                {
                    result = UoWStaff.CopyProfileToOtherDevices(staff, data.DeviceNumber, data.ClockDeviceIp);
                    //var downloadResult = UoWStaff.GetTemplpate("1", "172.16.0.112", "217");

                    //var delres = new FaceID().DeleteUser("1", "172.16.0.114", "217");


                    return result;
                }
            }



            return false;
        }


        [HttpGet]
        public bool GetCardMachineType()
        {

            //int test = 0005213695;
            //string hex = test.ToString("X");

            //int intAgain = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            //result = UoWStaff.CopyProfileToOtherDevices(staff, data.DeviceNumber, data.ClockDeviceIp);
            //var downloadResult = UoWStaff.GetTemplpate("1", "172.16.0.110", "215");

            var data = UoWStaff.GetTemplpate("1", "172.16.0.114", "215");
            return true;
        }

        public static StaffLeaveModel ValidateLeaveApplication(StaffLeaveModel leave)
        {
            if (leave == null)
                return default(StaffLeaveModel);
            using (var contextEf = new DataContextEF())
            {
                if (leave.LeaveStatus == 2)
                {
                    var data1 = from s in contextEf.Staff.Include("StaffLeaveData")
                                where s.StaffId.Equals(leave.StaffId)
                                select s;

                    var data = data1.FirstOrDefault();

                    if (data == null)
                    {

                    }
                    else
                    {
                        //data.StaffLeaveData = data.StaffLeaveData.Where(x => x.LeaveDateStart.Date.Equals(leave.LeaveDateStart.Date) || x.LeaveDateEnd.Date.Equals(leave.LeaveDateEnd.Date)).ToList();//.. as ICollection<StaffLeaveModel>;
                        //var existingLeaveDays = data.StaffLeaveData;
                        //if (existingLeaveDays != null && existingLeaveDays.Count > 0)
                        //    throw new Exception("There is an existing leave application on the selected date");//+ existingLeaveDays.ToList().FirstOrDefault(x => x.StaffMember.StaffFullName));
                    }
                }


                var staffHours = contextEf.StaffHourData.Where(x => x.StaffId.Equals(leave.StaffId)).ToList();

                if (!staffHours.Any())
                    return default(StaffLeaveModel);

                var todaysDate = DateTime.Now.Date;

                var staffDayClockData = contextEf.StaffClockData
                    .Where(x => x.StaffId.Equals(leave.StaffId)
                    && x.ClockDateTime == todaysDate)
                    .OrderByDescending(x => x.ClockDateTime)
                    .ToList();


                double hoursAlreadyWorked = 0;
                double minutesAlreadyWorked = 0;
                double requiredMinutes = 0;
                double requiredHours = 0;

                for (int i = 0; i < staffDayClockData.Count; i++)
                {
                    hoursAlreadyWorked = (staffDayClockData[staffDayClockData.Count - 1].ClockDateTime.TimeOfDay - staffDayClockData[i].ClockDateTime.TimeOfDay).Hours;
                    minutesAlreadyWorked = (staffDayClockData[staffDayClockData.Count - 1].ClockDateTime.TimeOfDay - staffDayClockData[i].ClockDateTime.TimeOfDay).Minutes;
                }

                for (int i = 0; i < staffHours.Count; i++)
                {
                    if (staffHours[i].DayId == (int)leave.LeaveDateStart.DayOfWeek)
                    {
                        var hourLunch = 1;
                        requiredHours = ((staffHours[i].DayTimeEnd.TimeOfDay - staffHours[i].DayTimeStart.TimeOfDay).Hours);
                        requiredMinutes = (staffHours[i].DayTimeEnd.TimeOfDay - staffHours[i].DayTimeStart.TimeOfDay).Minutes;
                        //if (requiredHours < 6)
                        //    requiredHours++;
                        break;
                    }
                }
                double durationOfLeaveH = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay).Hours;
                double durationOfLeaveM = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay).Minutes;

                //while (leave.LeaveDateStart != leave.LeaveDateEnd)
                //{
                //    if (leave.LeaveDateStart.IsPublicHoliday())
                //    {
                //        leave.LeaveDateStart.AddDays(1);
                //    }
                //}

                if (durationOfLeaveH > requiredHours)
                {
                    int valueToDeductH = (int)durationOfLeaveH - (int)requiredHours;

                    //adjust leaveend time by adding or deducting the surplus of the time 
                    leave.LeaveDateEnd = leave.LeaveDateEnd.AddHours(-valueToDeductH);
                    var durationOfLeaveMAfter = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay);
                    if (durationOfLeaveM > requiredMinutes)
                    {
                        int valueToDeductM = (int)durationOfLeaveM - (int)requiredMinutes;

                        //adjust leaveend time by adding or deducting the surplus of the time 
                        leave.LeaveDateEnd = leave.LeaveDateEnd.AddMinutes(-valueToDeductM);
                        var durationOfLeaveMAftert = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay); //verify changes to end date occured
                    }

                }



                if (hoursAlreadyWorked > 0)
                {
                    leave.LeaveDateStart = leave.LeaveDateStart.AddHours(hoursAlreadyWorked);

                    var durationOfLeaveMAfter = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay);  //verify changes to end date occured
                }
                if (minutesAlreadyWorked > 0)
                {
                    leave.LeaveDateStart = leave.LeaveDateStart.AddHours(minutesAlreadyWorked);

                    var durationOfLeaveMAfter = (leave.LeaveDateEnd.TimeOfDay - leave.LeaveDateStart.TimeOfDay); //verify changes to end date occured
                }

                return leave;
            }

        }


        public static StaffModel ValidateStaffModel(StaffModel staffModel)
        {
            if (staffModel == null)
                return default(StaffModel);



            using (var dtx = new DataContextEF())
            {

                //var newLeaveCountersData = staffModel.LeaveCounters.Where(x => x.StaffId.Equals(staffModel.StaffId)).ToList();
                //var originialLeaveCountersData = dtx.Staff.FirstOrDefault(x => x.StaffId.Equals(staffModel.StaffId));
                //var leaveCounters = new List<StaffLeaveCounterModel>();

                //if (originialLeaveCountersData != null)
                //     leaveCounters = originialLeaveCountersData.LeaveCounters.ToList();



                var data = dtx.Staff.FirstOrDefault(x => x.StaffId.Equals(staffModel.StaffId));

                if (data == null)
                    return staffModel;


                var originalModel = (StaffModel)data;
                if (originalModel.StaffJoinDate != staffModel.StaffJoinDate)
                {
                    if ((originalModel.StaffJoinDate - staffModel.StaffJoinDate).Days >= 31 || (staffModel.StaffJoinDate - originalModel.StaffJoinDate).Days >= 31)
                    {
                        try
                        {
                            var hr = new List<StaffModel>();

                            #region Get HR from ad
                            using (var context = new PrincipalContext(ContextType.Domain, "Pacific"))
                            {
                                using (var group = GroupPrincipal.FindByIdentity(context, "Human Relations"))
                                {
                                    if (group == null)
                                    {

                                    }
                                    else
                                    {
                                        var store = new DataContextEF();

                                        group.GetMembers(true)
                                            .ToList()
                                            .ForEach((m) =>
                                            {
                                                var staff =
                                                    store.Staff
                                                    .FirstOrDefault(
                                                        s => s.StaffEmail.ToLower().Equals(((UserPrincipal)m).EmailAddress.ToLower()));

                                                if (staff != null)
                                                    hr.Add(staff);
                                            });
                                    }
                                }
                            }
                            #endregion

                            var emailHr = new SmtpClient();
                            var mailMeassage = new MailMessage();
                            mailMeassage.To.Add(new MailAddress(hr.First(x => x.StaffName.Equals("Taryn")).StaffEmail));
                            mailMeassage.CC.Add(new MailAddress(staffModel.StaffEmail));
                            mailMeassage.Subject = "Amendment made to staff join date.";
                            mailMeassage.Body = "<p> Dear Hr </p> <p> With the recent change made to " + staffModel.StaffFullName + "'s commencement date," +
                                                " you might want to revisit the feature where you edit the startdate of the leave increment as well.</p><p>Kind regards</p><p>NVest Financial Holdings Leave Management System";
                            mailMeassage.IsBodyHtml = true;
                            emailHr.Send(mailMeassage);
                        }
                        catch (SmtpException e)
                        {

                        }
                    }

                }
                //if (newLeaveCountersData.Count > leaveCounters.Count)
                //{
                //    for (int i = 0; i < leaveCounters.Count(); i++)
                //    {
                //        leaveCounters.Last().EndPeriod = newLeaveCountersData[leaveCounters.Count() - 1].StartPeriod;
                //        staffModel.LeaveCounters = leaveCounters;
                //        break;
                //    }
                //}
            }

            return staffModel;
        }

        public static StaffLeaveCounterModel ValidateLeaveCounterModel(StaffLeaveCounterModel leaveCounter)
        {
            using (var contextEf = new DataContextEF())
            {
                var originialLeaveCountersData = contextEf.StaffLeaveCounter.FirstOrDefault(x => x.RecordId.Equals(leaveCounter.RecordId));


                var data = contextEf.Staff.Include("LeaveCounters").FirstOrDefault(x => x.StaffId.Equals(leaveCounter.StaffId));
                if (data != null)
                {
                    var leaveCounters = (IList<StaffLeaveCounterModel>)data.LeaveCounters.ToList();
                    leaveCounters = leaveCounters.Where(x => x.RecordStatus.Equals("Active")).ToList();

                    if (!leaveCounters.Any())
                        return leaveCounter;


                    for (int i = 0; i < leaveCounters.Count(); i++)
                    {
                        leaveCounters[leaveCounters.Count() - 1].EndPeriod = leaveCounter.StartPeriod;

                        contextEf.StaffLeaveCounter.AddOrUpdate(new StaffLeaveCounterModel()
                         {

                             RecordId = leaveCounters[leaveCounters.Count() - 1].RecordId,
                             StaffId = leaveCounters[leaveCounters.Count() - 1].StaffId,
                             StartPeriod = leaveCounters[leaveCounters.Count() - 1].StartPeriod,
                             EndPeriod = leaveCounters[leaveCounters.Count() - 1].EndPeriod,
                             Accumulator = leaveCounters[leaveCounters.Count() - 1].Accumulator,
                             Staff = leaveCounters[leaveCounters.Count() - 1].Staff,
                             RecordStatus = "Deleted",

                         });



                        break;
                    }
                }
                contextEf.SaveChanges();
            }

            return leaveCounter;
        }

        [HttpGet]
        public IEnumerable<object> StaffLeave1(Guid staffId)
        {
            return UoWStaffLeave.GetLeaveSummaryDetails(staffId);
        }

        [HttpGet]
        public IEnumerable<object> AllStaffLeaveSummary()
        {
            return UoWStaffLeave.GetAllLeaveSummaryDetails();
        }



        public int CalculateLeaveTaken(DateTime leaveStart, DateTime leaveEnd)
        {
            if (leaveStart.TimeOfDay.TotalSeconds > 1 || leaveEnd.TimeOfDay.TotalSeconds > 1)
            {


                var leaveDaysTaken = 1;
                var weekendHolidayCounter = 0;

                DateTime startValue = leaveStart;

                var holidayList = ctx.Context.Holidays
                          .Where(x => x.RecordStatus == "Active").ToList();

                //check for half here
                if (leaveStart == leaveEnd)
                    return leaveDaysTaken;


                var timeSpanInHours = leaveEnd.Subtract(leaveStart).TotalHours;
                if (leaveStart.Date.Equals(leaveEnd.Date) && timeSpanInHours > 0)
                    return (int)timeSpanInHours / 8;




                while (startValue.Date != leaveEnd.Date)
                {
                    leaveDaysTaken += 1;
                    //check if its a weekend
                    var dayOfWeek = startValue.DayOfWeek;
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    {
                        weekendHolidayCounter += 1;
                    }

                    //check if holiday
                    var response = holidayList.Find(r => r.HolidayDate.Date == startValue);
                    if (response != null)
                    {
                        weekendHolidayCounter += 1;
                    }

                    startValue = startValue.AddDays(1);
                }

                return leaveDaysTaken - weekendHolidayCounter;
            }
            return 1;
        }

        public double StaffLeaveAllocation(int type, StaffModel staff)
        {
            double allocatedDays = 0;

            switch (type)
            {
                case 1:
                    allocatedDays = staff.AnnualDays;
                    return allocatedDays;
                case 2:
                    allocatedDays = staff.SickCycleDays;
                    return allocatedDays;
                case 3:
                    allocatedDays = staff.StudyLeaveDays;
                    return allocatedDays;
                case 4:
                    allocatedDays = staff.FamilyResponsibilityLeaveDays;
                    return allocatedDays;
                case 5:
                    allocatedDays = staff.AnnualUnpaid;
                    return allocatedDays;
                case 6:
                    allocatedDays = 365;
                    return allocatedDays;
                default:
                    return allocatedDays;
            }
        }

        /// <summary>
        /// Get simultanious leave applications for other staff in same divisions 
        /// </summary>
        /// <param name="leaveId"></param>
        /// <returns></returns>
        [HttpGet]
        public IQueryable<StaffLeaveModel> StaffSimulLeaveApps(string leaveId)
        {
            Guid leaveGuid;

            if (!Guid.TryParse(leaveId, out leaveGuid))
                throw new Exception("Leave ID in an incorrect format");

            using (var store = new DataContextEF())
            {
                var leave = store.StaffLeaveData
                    .Include("StaffMember")
                    .FirstOrDefault(m => m.LeaveId.Equals(leaveGuid));

                if (leave == null)
                    throw new Exception("Leave not found");

                return UoWLeaveApplication.GetSimultaniousLeaveApps(leave.StaffMember, leave.LeaveDateStart, leave.LeaveDateEnd).AsQueryable();
            }
        }

        #region Leave application
        [HttpGet]
        public IQueryable<EnumModel> Leavetypes()
        {
            var leavetypes = EnumHelper.ConvertEnumToList<LeaveType>().ToList();

            var leaveContainer = new List<EnumModel>();

            leavetypes.ForEach((m) =>
            {
                if ((LeaveType)m != LeaveType.Invalid)
                {
                    leaveContainer.Add(new EnumModel
                    {
                        LeaveType = (int)(LeaveType)m,
                        Description = EnumHelper.GetEnumDescriptions((LeaveType)m)
                    });
                }
            });

            return leaveContainer.AsQueryable().OrderBy(x => x.Description);
        }

        [HttpGet]
        public IQueryable<StaffLeaveModel> LeaveApplications()
        {
            return ctx.Context.StaffLeaveData
                      .Where(m => m.RecordStatus.Equals("Active"));
        }

        [HttpGet]
        public StaffLeaveModel GetLeave(string id)
        {
            return GetLeaveModel(id);
        }

        [HttpGet]
        public bool EmailLeaveApplication(string leaveId)
        {
            var leaveApp = GetLeaveModel(leaveId);
            if (leaveApp == null)
                return false;

            if (leaveApp.LeaveType == (int)LeaveType.OffSite)
            {
                //if (!UoWStaff.ProcessClocking())
                //    return false;

                using (var contextEf = new DataContextEF())
                {
                    var staff = contextEf.Staff.Include("StaffHoursData").FirstOrDefault(m => m.StaffId.Equals(leaveApp.StaffId) && m.RecordStatus.Equals("Active"));
                    if (staff == null)
                        return false;

                    var tempStart = DateTime.Parse(leaveApp.LeaveDateStart.ToShortDateString());
                    var tempEnd = DateTime.Parse(leaveApp.LeaveDateEnd.ToShortDateString());

                    do
                    {
                        var dayHours = staff.StaffHoursData.FirstOrDefault(m =>
                                m.DayId.Equals((int)leaveApp.LeaveDateStart.DayOfWeek) &&
                                m.RecordStatus.Equals("Active"));

                        if (dayHours == null)
                            return false;

                        var clockDate = new DateTime(leaveApp.LeaveDateStart.Year,
                                                     leaveApp.LeaveDateStart.Month,
                                                     leaveApp.LeaveDateStart.Day,
                                                     dayHours.DayTimeStart.Hour + 2, //Forcing the issue here!
                                                     dayHours.DayTimeStart.Minute, 0);

                        contextEf.StaffClockData.Add(new StaffClockModel()
                            {
                                ClockDateTime = clockDate,
                                Comments = "Automated insert - Onsite leave application day time start",
                                DataStatus = (int)ClockRecordEnums.Approved,
                                IsLeaveRecord = true,
                                LeaveType = (int)LeaveType.OffSite,
                                StaffId = staff.StaffId,
                                RecordStatus = "Active"
                            });

                        contextEf.StaffClockData.Add(new StaffClockModel
                            {
                                ClockDateTime =
                                    new DateTime(clockDate.Year,
                                                 clockDate.Month,
                                                 clockDate.Day,
                                                 dayHours.DayTimeEnd.Hour + 2, //Forcing the issue here!
                                                 dayHours.DayTimeEnd.Minute, 0),
                                Comments = "Automated insert - Oniste leave appliaction day end",
                                DataStatus = (int)ClockRecordEnums.Approved,
                                IsLeaveRecord = true,
                                LeaveType = (int)LeaveType.OffSite,
                                StaffId = staff.StaffId,
                                RecordStatus = "Active"
                            });

                        tempStart = tempStart.AddDays(1);
                    } while (tempStart.Date <= tempEnd.Date);
                    contextEf.SaveChanges();
                    return true; 
                }
            }
            else
            {

                //get all staff in the user's division and check if they will be on leave
                // If they will be on leave add them include them in the email details
                var divisionStaff =
                    new DataContextEF().Staff.Include("StaffLeaveData").Where(m =>
                                                                              m.DivisionId.Equals(
                                                                                  leaveApp.StaffMember.DivisionId) &&
                                                                              !m.StaffId.Equals(leaveApp.StaffId));

                LeaveMessage message = new LeaveApplications(leaveApp, divisionStaff);

                if (leaveApp.StaffMember.RecieveSystemMail)
                    return SendLeaveMessage(message);


                    return true;

               
            }
        }

        [HttpGet] 
        public bool EmailLeaveDeclined(string leaveId)
        {
            var leaveApp = GetLeaveModel(leaveId);
            return leaveApp != null && SendLeaveMessage(new DeclineLeaveApplication(leaveApp));
        }

        [HttpGet]
        public bool EmailLeaveApproved(string leaveId)
        {
            var leaveApp = GetLeaveModel(leaveId);
            if (leaveApp.StaffMember.RecieveSystemMail)
            {
                return SendLeaveMessage(new ApproveLeaveApplication(leaveApp));
            }
            #region

            /* using (var contextEf = new DataContextEF())
           {
               var staff =
                     contextEf.Staff.Include("StaffHoursData")
                              .FirstOrDefault(
                                  m => m.StaffId.Equals(leaveApp.StaffId) && m.RecordStatus.Equals("Active"));
               if (staff == null)
                   return false;

               var dayHours =
                           staff.StaffHoursData.FirstOrDefault(
                               m =>
                               m.DayId.Equals((int)leaveApp.LeaveDateStart.DayOfWeek) &&
                               m.RecordStatus.Equals("Active"));

               if (dayHours == null)
                   return false;

               var clockDate = new DateTime(leaveApp.LeaveDateStart.Year, leaveApp.LeaveDateStart.Month,
                                                   leaveApp.LeaveDateStart.Day, dayHours.DayTimeStart.Hour +2,
                                                   dayHours.DayTimeStart.Minute, 0);

               var tempStart = new DateTime(leaveApp.LeaveDateStart.Year, leaveApp.LeaveDateStart.Month,
                                                   leaveApp.LeaveDateStart.Day, dayHours.DayTimeStart.Hour + 2,
                                                   dayHours.DayTimeStart.Minute, 0);


               var tempEnd = new DateTime(leaveApp.LeaveDateEnd.Year, leaveApp.LeaveDateEnd.Month,
                                                   leaveApp.LeaveDateEnd.Day, dayHours.DayTimeStart.Hour + 2,
                                                   dayHours.DayTimeEnd.Minute, 0);

               #region This do while is not the best loop for this operation I'll try a foreach monday
               do
               {
                   contextEf.StaffClockData.Add(new StaffClockModel()
                   {
                       ClockDateTime = tempStart, // if (IsWeekend(date)) continue;
                       Comments = "Automated insert - Staff member is on leave",
                       DataStatus = (int) ClockRecordEnums.OnLeave,
                       IsLeaveRecord = true,
                       LeaveType = (int) leaveApp.LeaveType,
                       StaffId = staff.StaffId,
                       RecordStatus = "Active"
                   });
                   tempStart = tempStart.AddDays(1);

               } while (tempStart.Date <= tempEnd.Date);
               #endregion

               
               contextEf.SaveChanges();

           }*/

            #endregion

            return false;
        }

        [HttpGet]
        public bool EmailLeaveCancelled(string leaveId)
        {
            var staffLeave = GetLeaveModel(leaveId);
            if (staffLeave.StaffMember.RecieveSystemMail)
            {
                return SendLeaveMessage(new CancelLeaveApplication(staffLeave));
             
            }
            return false;
        }

        /*
        [HttpGet]
        public bool EmailLeaveCancelled(string leaveId)
        {
            var staffLeave = GetLeaveModel(leaveId);
            return SendLeaveMessage(new CancelLeaveApplication(GetLeaveModel(leaveId)));
        }
         */

        [HttpGet]
        public IQueryable<EnumModel> LeaveStatuses()
        {
            var statustypes = EnumHelper.ConvertEnumToList<LeaveStatus>().ToList();
            var statusContainer = new List<EnumModel>();
            statustypes.ForEach((m) =>
            {
                if ((LeaveStatus)m != LeaveStatus.Invalid)
                {
                    statusContainer.Add(new EnumModel
                    {
                        LeaveType = (int)(LeaveStatus)m,
                        Description = EnumHelper.GetEnumDescriptions((LeaveStatus)m)
                    });
                }
            });
            return statusContainer.AsQueryable();
        }

        [HttpGet]
        public IQueryable<StaffLeaveModel> SStaffLeaveDaysApprovedtaffLeaveDaysApproved2()
        {
            return ctx.Context.StaffLeaveData
                      .Where(m => m.LeaveStatus.Equals('1'));
        }

        [HttpGet]
        public IQueryable<StaffModel> StaffLeaveDaysApproved()
        {
            return ctx.Context.Staff.Where(m => m.RecordStatus.Equals("Active"));
        }

        #region Private Helper methods

        private static bool SendLeaveMessage(LeaveMessage message)
        {
            try
            {
                return message.SendMessage();
            }
            catch (Exception exception)
            {
                //catch and log the exception
                throw;
            }

        }

        private static StaffLeaveModel GetLeaveModel(string id)
        {
            Guid leaveId;

            if (!Guid.TryParse(id, out leaveId))
                throw new Exception("Invalid leave id");

            //get the leave application
            return new DataContextEF().StaffLeaveData.Include("StaffMember").FirstOrDefault(m => m.LeaveId.Equals(leaveId));
        }

        #endregion
        #endregion

        #endregion
        #region Phone Details

        [HttpGet]
        public HttpResponseMessage StaffPhone(string id)
        {
            return new HttpResponseMessage() { Content = new StringContent(UowPhone.GetPhoneSettings(id), Encoding.UTF8, "application/xml") };
        }


        [HttpGet]
        public HttpResponseMessage UpdatePhoneStatusOffhook(string id)
        {
            using (var dtx = new DataContextEF())
            {
                var mac = id.Split('^').First();
                //var status = id.Split('^').Last();
                StaffPhoneDetailModel staff = dtx.PhoneDetails.Include("StaffMember").First(x => x.StaffPhoneMac.Equals(mac));
                var phoneMac = staff.StaffPhoneMac;
                ActivateHub("Busy", staff.StaffId);
                return new HttpResponseMessage() { Content = new StringContent(UowPhone.UpdatePhoneStatus("Busy", phoneMac), Encoding.UTF8) };
            }
        }


        [HttpGet]
        public HttpResponseMessage UpdatePhoneStatusOnhook(string id)
        {
            using (var dtx = new DataContextEF())
            {
                var mac = id.Split('^').First();
                //var status = id.Split('^').Last();
                StaffPhoneDetailModel staff = dtx.PhoneDetails.Include("StaffMember").First(x => x.StaffPhoneMac.Equals(mac));
                var phoneMac = staff.StaffPhoneMac;
                ActivateHub("Available", staff.StaffId);
                return new HttpResponseMessage() { Content = new StringContent(UowPhone.UpdatePhoneStatus("Available", phoneMac), Encoding.UTF8) };
            }
        }

        #endregion


        #region Signal R
        private static void ActivateHub(string staffPhoneStatus, Guid staffid)
        {
            var hub = new PhoneHub();
            hub.Send("", staffPhoneStatus, staffid);
        }
        #endregion
    }
}