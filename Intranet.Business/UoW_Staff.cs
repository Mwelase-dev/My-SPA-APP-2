using System.Configuration;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using Data.FaceID;
using Intranet.Data.EF;
using Intranet.Messages;
using Intranet.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Intranet.Models.Enums;
//using Newtonsoft.Json;
using Utilities;

namespace Intranet.Business
{
    public class UoWStaff
    {

        private static FaceIDConfig config = (FaceIDConfig)System.Configuration.ConfigurationManager.GetSection("clockingDevices");
        private static DateTime PreviousSyncTime { get; set; }
        // forcing us to use the DBContext directly.
        static readonly DataContextEF ctx = new DataContextEF();

        #region Private Memebers
        /// <summary>
        /// Remove in-active staff or staff with empty email addresses
        /// Ensure we are only working with staff that clock in/out
        /// </summary>
        /// <param name="staffList">Full list of staff members</param>
        /// <returns></returns>
        private static IEnumerable<StaffModel> FilterStaffList(IEnumerable<StaffModel> staffList)
        {
            return staffList
                .Where(x => x.RecordStatus.ToLower() == "active")
                .Where(x => x.StaffEmail != String.Empty)
                .Where(x => x.StaffIsClockingMember).ToList();
        }

        /// <summary>
        /// Gets the data from the clocking devices.
        /// </summary>
        /// <param name="listOfStaff"></param>
        private static IEnumerable<StaffModel> GetClockData(IEnumerable<StaffModel> listOfStaff, IList<FaceRecord> clockData)
        {
            try
            {
                var staffClockIdList = (from clock in clockData select int.Parse(clock.RecordID)).Distinct().OrderBy(m => m).ToList();





                // Get the staff where there are clock records found for them on the device
                var staffList = (from staff in listOfStaff.ToList()
                                 join clockId in staffClockIdList
                                     on staff.StaffClockId equals clockId
                                 select staff).ToList();

                // Process their clock entry
                foreach (var staff in staffList)
                {


                    //TODO: can use a merge here?
                    var staffClockData = clockData.Where(x => int.Parse(x.RecordID) == staff.StaffClockId).ToList();
                    var dataClock = staff.StaffClockData; // .ToList();
                    foreach (var clockEntry in staffClockData)
                    {
                        /*
                         *Please also keep in mind that labour law says that a person may not work while on leave.
                         *So, if a person does do so, the leave for that period of time needs to be cancelled.
                         *So that is why I say that it needs to deduct the number of hours worked from the original leave applied for 
                         *– i.e. the leave applied for needs to become shorter and that needs to be recorded appropriately.
                         *So it should not reflect as overtime, the leave application needs to be affected. 
                         */
                        if (staff.StaffLeaveData.Any(x => x.LeaveDateStart.Date.Equals(clockEntry.RecordDateTime.Date) && x.LeaveStatus == (int)LeaveStatus.Approved))
                        {
                            EditApprovedLeaveForTheClockEntryDayOfTheLeave(staff.StaffLeaveData.FirstOrDefault(x => x.LeaveDateStart.Date.Equals(clockEntry.RecordDateTime.Date)));
                        }

                        // If it is new, add it to their list. If it is a duplicate, skip over it.
                        var record = new StaffClockModel(staff.StaffId, clockEntry.RecordDateTime);
                        if (dataClock.Contains(record)) continue;

                        staff.StaffClockData.Add(record);
                        staff.StaffClockReminders = 0; // Reset counter.

                        // Here we need to check if the staff is on leave. If they are and we picked up a clock record for them on the day
                        // surely it must mean that they are not on leave. why did they clock in then?
                        // If it was an Auto Leave app, remove it then...
                        // TODO: Need to look at creating a gap for a staff member that is on leave for a week, but is in the office during that time.
                        if (!staff.StaffIsOnLeave) continue;
                        var leaveApp = staff.StaffLeaveData.FirstOrDefault(x => (x.LeaveDateStart.Date == DateTime.Today) && (x.LeaveDateEnd == DateTime.Today));
                        if (leaveApp != null)
                        {
                            leaveApp.LeaveStatus = (int)LeaveStatus.Cancelled;
                            // TODO: Need to ensure the leave status here?
                        }
                    }
                }

                ctx.SaveChanges();

                return staffList;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                throw;
            }
        }

        private static void EditApprovedLeaveForTheClockEntryDayOfTheLeave(StaffLeaveModel leaveDayTurnedWorkDay)
        {




            /*
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
             */

        }


        /// <summary>
        /// Incomplete clock data
        /// </summary>
        /// <param name="staff"></param>
        private static void ProcessDailyClockRecords(IEnumerable<StaffModel> staff)
        {
            //if (DateTime.Now.Hour != 22)
            //return;
            //This is the mail sending that works
            var lateStaff = (from lateComers in staff
                             where lateComers.StaffClockData.Count(m => m.ClockDateTime.Date.Equals(DateTime.Today.Date)) %
                                   2 != 0
                             select lateComers).ToList();

            foreach (StaffModel model in lateStaff)
            {
                var mailer = new Emailer { subject = MessageList.IncompleteDayClockData_Subject };
                mailer.TOList.Add(model.StaffEmail);
                mailer.body = string.Format("<p>Dear {0}</p>", model.StaffFullName) + MessageList.IncompleteDayClockData;
                if (model.RecieveSystemMail)
                {
                    mailer.SendEmail();
                }

            }
        }

        private static void AutoLeaveApp(IEnumerable<StaffModel> staff)
        {

            var curr = DateTime.Now;
            //if (DateTime.Now.Hour != 17)
            //    return;

            var staffList = staff.Where(x => x.StaffClockData.Any(z => (z.ClockDateTime.Date == curr.Date && z.ClockDateTime.Date >= curr.Date))).ToList();

            // Get the list of staff whom have not yet clocked in!
            var absentList = staffList
                .Where(x => !x.StaffIsOnLeave) // Make sure that staff are not on leave
                .Except(staffList)
                .ToList();


            //var leaveAppsStaff = (from st in staff
            //                      where !st.StaffClockData.Any(m => m.ClockDateTime.Date.Equals(DateTime.Now.Date)) && st.RecordStatus.Equals("Active")
            //                      select st).ToList();

            using (ctx)
            {


                foreach (var models in absentList)
                {

                    var staffHoursModel =
                                models.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)DateTime.Now.DayOfWeek));

                    if (staffHoursModel == null)
                        throw new ArgumentNullException("staffHoursModel");

                    var mailer = new Emailer { subject = MessageList.AutomaticLeaveApplication_Subject };
                    ctx.StaffLeaveData.Add(new StaffLeaveModel
                        {
                            ApprovedBy1 = models.StaffManager1Id,
                            ApprovedBy2 = models.StaffManager2Id,
                            LeaveComments = "Automatically applied as a result of not clocking in for the day. \n\n For any queries pleasec contact HR or your manager",
                            LeaveDateEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0),
                            LeaveDateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0),
                            LeaveRequestDate = DateTime.Now,
                            LeaveStatus = (int)LeaveStatus.Approved,
                            LeaveType = (int)LeaveType.Annual,
                            StaffId = models.StaffId,
                            RecordStatus = "Active",
                            LeaveId = Guid.NewGuid()
                        });

                    mailer.TOList.Add(models.StaffEmail);
                    mailer.body =
                        string.Format(
                            "<p>Dear {0}</p> ", models.StaffFullName) + MessageList.AutomaticLeaveApplication;
                    mailer.SendEmail();
                }
                ctx.SaveChanges();
            }
        }



        #region Send emails
        /// <summary>
        /// Looks for staff whom have not clocked in by then "Start time" and sends them a reminder.
        /// </summary>
        /// <returns>Boolean</returns>
        private static Boolean SendLateEmails(IEnumerable<StaffModel> listOfStaff)
        {
            // Use the current Date
            var curr = DateTime.Now;
            var staffList = listOfStaff.Where(x => x.StaffIsOnLeave == false).ToList();

            // Get the list of staff that have clocked in.
            //var staffList = listOfStaff.Where(x => x.StaffClockData.Any(z => (z.ClockDateTime.TimeOfDay <= curr.TimeOfDay && z.ClockDateTime >= curr.Date))).ToList();
            staffList = staffList.Where(x => x.StaffClockData.Any(z => (z.ClockDateTime.Date == curr.Date && z.ClockDateTime.Date >= curr.Date))).ToList();

            // Get the list of staff whom have not yet clocked in!
            var lateList = listOfStaff
                .Where(x => !x.StaffIsOnLeave) // Make sure that staff are not on leave
                .Except(staffList)
                .ToList();

            //Remove people who do not clock in from mailing list
            lateList = lateList.Where(x => x.StaffIsClockingMember.Equals(true) && x.RecordStatus.Equals("Active")).ToList();

            foreach (var staff in lateList)
            {
                // Send the list an email.
                var mailer = new Emailer();
                mailer.subject = MessageList.ClockingLate_Subject;
                mailer.body = string.Format("Dear {0} ", staff.StaffFullName) + MessageList.ClockingLate_Body;

                // Only send an email if the current time is greater than the staff's start time. i.e. they are late!
                // - Do not send reminders on weekends.
                if ((curr.DayOfWeek != DayOfWeek.Saturday) && (curr.DayOfWeek != DayOfWeek.Sunday))
                {
                    // - Do not send consecutive reminders if you are going to force the leave day.
                    if (staff.StaffClockReminders < 3) //TODO: Needs to be a system setting perhaps?
                    {
                        // Get the staff members start time for this day
                        var todaysHours = staff.StaffHoursData.FirstOrDefault(x => x.DayId == (int)DateTime.Today.DayOfWeek);
                        if (todaysHours != null)
                        {
                            if (curr.TimeOfDay >= todaysHours.DayTimeStart.TimeOfDay)
                            {
                                // Increment warning counter
                                staff.StaffClockReminders++;

                                // Send BCC so that other staff cannot see whom is all late.
                                mailer.TOList.Add(staff.StaffEmail);
                            }
                        }
                        else
                        {
                            //throw new Exception(String.Format("Cannot determine {0}'s working hours for today {1}", staff.StaffName, DateTime.Today.DayOfWeek.ToString()));
                            Trace.WriteLine(String.Format("Cannot determine {0}'s working hours for today {1}", staff.StaffName, DateTime.Today.DayOfWeek.ToString()));
                        }
                    }
                }
                if (staff.RecieveSystemMail)
                {
                    mailer.SendEmail();
                }

            }
            return true;
        }
        /* private static Boolean SendLateEmails(IEnumerable<StaffModel> listOfStaff)
         {
             // Use the current Date
             var curr = DateTime.Now;
             var staffList = listOfStaff.Where(x => x.StaffIsOnLeave == false).ToList();


             // Get the list of staff that have clocked in.
             //var staffList = listOfStaff.Where(x => x.StaffClockData.Any(z => (z.ClockDateTime.TimeOfDay <= curr.TimeOfDay && z.ClockDateTime >= curr.Date))).ToList();
             staffList = staffList.Where(x => x.StaffClockData.Any(z => (z.ClockDateTime.Date == curr.Date && z.ClockDateTime.Date >= curr.Date))).ToList();




             // Get the list of staff whom have not yet clocked in!
             var lateList = listOfStaff
                 .Where(x => !x.StaffIsOnLeave) // Make sure that staff are not on leave
                 .Except(staffList)
                 .ToList();



             //Remove people who do not clock in from mailing list
             lateList = lateList.Where(x => x.StaffIsClockingMember.Equals(true) && x.RecordStatus.Equals("Active")).ToList();

             var mailer = new Emailer();

             foreach (var staff in lateList)
             {
                 // Send the list an email.

                 mailer.subject = MessageList.ClockingLate_Subject;
                 mailer.body = string.Format("Dear {0} ", staff.StaffFullName) + MessageList.ClockingLate_Body;


                 // Only send an email if the current time is greater than the staff's start time. i.e. they are late!
                 // - Do not send reminders on weekends.
                 Trace.WriteLine("Day of week: " + curr.DayOfWeek);
                 if ((curr.DayOfWeek != DayOfWeek.Saturday) && (curr.DayOfWeek != DayOfWeek.Sunday))
                 {
                     // - Do not send consecutive reminders if you are going to force the leave day.
                     Trace.WriteLine(String.Format("{0} has {1} reminder(s)", staff.StaffName, staff.StaffClockReminders));
                     if (staff.StaffClockReminders < 3) //TODO: Needs to be a system setting perhaps?
                     {
                         // Get the staff members start time for this day
                         var todaysHours = staff.StaffHoursData.FirstOrDefault(x => x.DayId == (int)DateTime.Today.DayOfWeek);
                         if (todaysHours != null)
                         {
                             Trace.WriteLine(String.Format(" - working hours for {0}: {1} - {2}", staff.StaffName,
                                                           todaysHours.DayTimeStart.ToString("HH:mm:ss"),
                                                           todaysHours.DayTimeEnd.ToString("HH:mm:ss")));
                             if (curr.TimeOfDay >= todaysHours.DayTimeStart.TimeOfDay)
                             {
                                 Trace.WriteLine(String.Format(" - {0} is late", staff.StaffName));

                                 // Increment warning counter
                                 staff.StaffClockReminders++;

                                 // Send BCC so that other staff cannot see whom is all late.
                                 mailer.BCList.Add(staff.StaffEmail);
                             }
                         }
                         else
                         {
                             //throw new Exception(String.Format("Cannot determine {0}'s working hours for today {1}", staff.StaffName, DateTime.Today.DayOfWeek.ToString()));
                             Trace.WriteLine(String.Format("Cannot determine {0}'s working hours for today {1}", staff.StaffName, DateTime.Today.DayOfWeek.ToString()));
                         }
                     }
                 }
             }

             return mailer.SendEmail();
         }*/

        /// <summary>
        /// Looks for staff that have 3 or more "Clocking" reminders and sends them an automated leave app.
        /// </summary>
        /// <returns>Boolean</returns>
        private static Boolean SendLeaveEmails(IEnumerable<StaffModel> listOfStaff)
        {
            // Get staff with more than 3 reminders
            var staffList = listOfStaff
                .Where(x => x.StaffClockReminders > 2) //TODO: Needs to be a system setting perhaps?
                .ToList();

            // Send the list an email.

            foreach (var staff in staffList)
            {
                var staffHoursModel =
                      staff.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)DateTime.Now.DayOfWeek));
                if (staffHoursModel != null)
                {
                    if (DateTime.Now.TimeOfDay > new TimeSpan(0, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute))
                    {

                        var mailer = new Emailer
                        {
                            subject = string.Format("Dear {0} ", staff.StaffFullName) + MessageList.ClockingLeave_Subject,
                            body = MessageList.ClockingLeave_Body
                        };
                        // Reset warning Counter
                        staff.StaffClockReminders = 0;


                        // Force Day's leave
                        Trace.WriteLine(String.Format("Forcing a day's leave for {0}", staff.StaffName));
                        var forcedLeave = StaffLeaveModel.ForcedLeave(staff.StaffManager1.StaffId);



                        forcedLeave.LeaveComments = forcedLeave.LeaveComments + " " + new TimeSpan(0, staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute).ToString(@"hh\h\ mm"); ;

                        forcedLeave.LeaveDateEnd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            staffHoursModel.DayTimeEnd.Hour + 2, staffHoursModel.DayTimeEnd.Minute, 0);
                        forcedLeave.LeaveDateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                            staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0);


                        staff.StaffLeaveData.Add(forcedLeave);

                        // Send the notification email

                        mailer.TOList.Add(staff.StaffEmail);
                        mailer.TOList.Add(staff.StaffManager1.StaffEmail);
                        mailer.TOList.Add(staff.StaffManager2.StaffEmail);
                        if (staff.RecieveSystemMail)
                        {
                            mailer.SendEmail();
                        }
                    }
                }
            }
            return true;
        }

        #endregion
        #endregion

        #region Public
        public static IList<FaceRecord> GetClockDataForThem()
        {
            var dataList = new List<FaceRecord>();
            string line = "";
            StreamReader file = new StreamReader("D:\\Clock data\\TIME009.TXT");
            while ((line = file.ReadLine()) != null)
            {
                var Record = new FaceRecord();
                foreach (string item in Regex.Split(line, "\" "))
                {
                    if (item.StartsWith("time="))
                    {
                        Record.RecordDateTime = DateTime.Parse(item.Substring(6, item.Length - 6));
                    }
                    if (item.StartsWith("id="))
                    {
                        Record.RecordID = item.Substring(4, item.Length - 4);
                    }
                    if (item.StartsWith("name="))
                    {
                        Record.RecordName = item.Substring(6, item.Length - 6);
                    }

                    //We have what we need, move on.
                    if (Record.RecordPopulated)
                    {
                        if (Record.RecordName.ToLower().Equals("asiwa") || Record.RecordName.ToLower().Equals("ojiya"))
                            dataList.Add(Record);
                        break;
                    }
                }
            }

            return dataList;
        }

        /// <summary>
        /// Processes the clocking data,
        /// Uses the IP addresses in the config file,
        /// Sends emails if staff are late, or forced leave, but not on public holidays
        /// </summary>
        /* public static bool ProcessClocking()
         {
             //var aliciaAndOlive = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), GetClockDataForThem());


             var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();

             // Need to remember that if it is a holiday, skip sending reminders.
             // Need to check if staff are on leave and not send reminders
             if (!DateTime.Now.IsPublicHoliday())
             {
                 if (!DateTime.Now.IsPublicHoliday())
                 {

                     // It is important to use a try/catch here as we still want to save the data that follows after the return here.
                     // TODO: Perhaps we need some threading or delegate here?
                     try
                     {

                         if (DateTime.Now.TimeOfDay > new TimeSpan(0, 08, 1, 0) || DateTime.Now.TimeOfDay < new TimeSpan(0, 08, 15, 0) || DateTime.Now.TimeOfDay < new TimeSpan(0, 08, 46, 0))
                         {
                             //Here we look for late staff and send them a reminder to clock in.
                             SendLateEmails(staffList);
                         }

                         if (DateTime.Now.TimeOfDay > new TimeSpan(0, 13, 30, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 13, 55, 0)) //if (DateTime.Now.Hour > 17)
                         {
                             //Here we look for staff that have NOT clocked in yet and send them a leave application message.
                             SendLeaveEmails(staffList);
                         }

                         if (DateTime.Now.TimeOfDay > new TimeSpan(0, 14, 0, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 14, 15, 0))
                         {
                             //This method will only run at 5pm
                             //it automatically applies leave for a staff member who has not clocked in on the day
                             AutoLeaveApp(staffList);
                         }

                         if (DateTime.Now.TimeOfDay > new TimeSpan(0, 17, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 17, 10, 0))
                         {
                             //this method will only work at 11 pm. This will Remind a user of a missed "clock in" if they do not clock back in after lunch or home time etc
                             ProcessDailyClockRecords(staffList);
                         }

                         if (DateTime.Now.Hour > PreviousSyncTime.Hour)
                         {
                             SyncTimes();
                         }

                         if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.TimeOfDay > new TimeSpan(0, 17, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 17, 10, 0))
                         {
                             using (var client = new HttpClient())
                             {
                                 var URI = WebConfigurationManager.AppSettings["ProcessWeeklyManagerMailURL"];
                                 var serializedContact = "";
                                 var content = new StringContent(serializedContact, Encoding.UTF8, "application/json");
                                 var result = client.PostAsync(URI, content);
                             }
                         }
                         if (DateTime.Now.TimeOfDay > new TimeSpan(0, 22, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 22, 10, 0))
                         {
                         DeductHoursFromLeave(staffList);
                         }

                     }
                     catch (Exception ex)
                     {
                         Trace.Write(ex);
                         throw;
                     }
                 }

             }

             return true;
         }*/

        private static void DeductHoursFromLeave(List<StaffModel> staff)
        {
            foreach (StaffModel staffModel in staff)
            {
                var clockData = staffModel.StaffClockData;
                var leaveData = staffModel.StaffLeaveData;

                var didSomeoneComeToWorkWhenSupposedToBeOnLeave = leaveData.Where(l => clockData.Any(c => c.ClockDateTime.Date == l.LeaveDateStart.Date && l.LeaveStatus.Equals(1)));
                var clockingTimesOnDayOfLeave = clockData.Where(c => leaveData.Any(l => l.LeaveDateStart.Date == c.ClockDateTime.Date));

                if ((leaveData.Where(c => clockData.Any(l => l.ClockDateTime.Date == c.LeaveDateStart.Date))).Any() && didSomeoneComeToWorkWhenSupposedToBeOnLeave.Any())
                {
                    IList<DateTime> clockingTimes = clockingTimesOnDayOfLeave.Select(staffClockModel => staffClockModel.ClockDateTime).ToList();
                    clockingTimes = clockingTimes.OrderBy(x => x.TimeOfDay).ToList();
                    var timeWorkedThatMustBeMinusedFromLeave = CalculateTimeWorkedDuringLeaveDay(clockingTimes);

                    IEnumerable<DateTime> clockRecordOnDayOfLeave = clockingTimes.Take(1);
                    //StaffLeaveModel leave = didSomeoneComeToWorkWhenSupposedToBeOnLeave.First();

                    foreach (StaffLeaveModel leave in didSomeoneComeToWorkWhenSupposedToBeOnLeave)
                    {
                        if (leave.LeaveDateStart.DifferenceInDays(leave.LeaveDateEnd) <= 1)
                        {
                            using (ctx)
                            {
                                var leaveToCancel = ctx.StaffLeaveData.FirstOrDefault(x => x.LeaveId == leave.LeaveId);

                                if (leaveToCancel != null)
                                {
                                    leaveToCancel.LeaveStatus = (int)LeaveStatus.Cancelled;
                                }
                                ctx.SaveChanges();

                                var emailer = new Emailer();
                                emailer.subject = "Leave canceled.";
                                emailer.body =
                                    "<html><p>Good Day " + staffModel.StaffFullName + ",</p> <p>As a result of you clocking in on  " + leaveToCancel.LeaveDateStart.Date + ",which is a day with approved leave, your leave application has been canceled.</p>" +
                                    "<p>Kind regards</p> <p> NVest Holdings clocking system</p>";
                                emailer.TOList.Add(staffModel.StaffEmail);
                                if (staffModel.RecieveSystemMail)
                                {
                                    emailer.SendEmail();
                                }
                            }
                        }
                        else if (clockRecordOnDayOfLeave.First().Date.Equals(leave.LeaveDateStart.Date))
                        {
                            using (ctx)
                            {
                                var leaveToEditStart = ctx.StaffLeaveData.FirstOrDefault(x => x.LeaveId == leave.LeaveId);

                                if (leaveToEditStart != null)
                                {
                                    leaveToEditStart.LeaveDateStart = leaveToEditStart.LeaveDateStart.AddDays(1);

                                    var emailer = new Emailer();
                                    emailer.subject = "Leave re adjusted.";
                                    emailer.body =
                                        "<html><p>Good Day " + staffModel.StaffFullName + ",</p> <p>As a result of you clocking in between the dates\"" + leave.LeaveDateStart.Date + " and " + leave.LeaveDateEnd.Date + "\",which is a date range with approved leave, your leave application has been re adjusted, having the day you clocked in not part of the date range you are supposed to be on leave.</p>" +
                                        "<p>Kind regards</p> <p> NVest Holdings clocking system</p>";
                                    emailer.TOList.Add(staffModel.StaffEmail);
                                    if (staffModel.RecieveSystemMail)
                                    {
                                        emailer.SendEmail();
                                    }
                                }
                                ctx.SaveChanges();

                            }
                        }
                        else if (clockRecordOnDayOfLeave.First().Date.Equals(leave.LeaveDateEnd.Date))
                        {
                            using (ctx)
                            {
                                using (ctx)
                                {
                                    var leaveToEditEnd = ctx.StaffLeaveData.FirstOrDefault(x => x.LeaveId == leave.LeaveId);

                                    if (leaveToEditEnd != null)
                                    {
                                        leaveToEditEnd.LeaveDateEnd = leaveToEditEnd.LeaveDateEnd.AddDays(-1);

                                        var emailer = new Emailer();
                                        emailer.subject = "Leave re adjusted.";
                                        emailer.body =
                                            "<html><p>Good Day " + staffModel.StaffFullName + ",</p> <p>As a result of you clocking in between the dates\"" + leave.LeaveDateStart.Date + " and " + leave.LeaveDateEnd.Date + "\",which is a date range with approved leave, your leave application has been re adjusted, having the day you clocked in not part of the date range you are supposed to be on leave.</p>" +
                                            "<p>Kind regards</p> <p> NVest Holdings clocking system</p>";
                                        emailer.TOList.Add(staffModel.StaffEmail);
                                        if (staffModel.RecieveSystemMail)
                                        {
                                            emailer.SendEmail();
                                        }
                                    }
                                    ctx.SaveChanges();

                                }
                            }
                        }
                    }



                }
            }
        }
        private static string CalculateTimeWorkedDuringLeaveDay(IList<DateTime> clockingTimes)
        {
            var hoursWorked = 0;
            var minutesWorked = 0;

            for (var i = 0; i < clockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < clockingTimes.Count())
                {
                    var startSpan = new TimeSpan(0, clockingTimes[i].Hour, clockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, clockingTimes[i + 1].Hour, clockingTimes[i + 1].Minute, 0);

                    var spanDif = endSpan - startSpan;

                    hoursWorked += spanDif.Hours < 0 ? (spanDif.Hours) : spanDif.Hours;
                    minutesWorked += spanDif.Minutes < 0 ? (spanDif.Minutes) : spanDif.Minutes;
                }
            }

            minutesWorked += hoursWorked * 60;

            return string.Format("{0:hh\\:mm}", TimeSpan.FromMinutes(minutesWorked));
        }

        private static IEnumerable<DateTime> GetClockingDays(IEnumerable<StaffClockModel> clockModels)
        {
            IList<DateTime> clockDays = new List<DateTime>();
            var clockingDays = clockModels;
            foreach (StaffClockModel staffClockModel in clockingDays)
            {
                var clockDate = new DateTime(staffClockModel.ClockDateTime.Year, staffClockModel.ClockDateTime.Month, staffClockModel.ClockDateTime.Day);

                if (!clockDays.Contains(clockDate))
                {
                    clockDays.Add(clockDate);
                }
            }
            return clockDays;
        }


        /// <summary>
        /// Processes the clocking data,
        /// Uses the IP addresses in the config file,
        /// Sends emails if staff are late, or forced leave, but not on public holidays
        /// </summary>
        public static bool ProcessClocking()
        {
            try
            {
                var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool SyncTimes()
        {
            var faceId = new FaceID();
            return faceId.SyncTime();
        }

        public static bool ClockingReminders()
        {
            try
            {
                var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();
                if (!DateTime.Now.IsPublicHoliday())
                {
                    return SendLateEmails(staffList);
                }
                return true;
             
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool SendLeaveEmails()
        {
            try
            {
                var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();
                if (!DateTime.Now.IsPublicHoliday())
                {
                    return SendLeaveEmails(staffList);
                }
                return true;
             
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool DailyClockRecords()
        {
            try
            {
                var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();
                ProcessDailyClockRecords(staffList);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        public static bool DeductHoursFromLeave()
        {
            try
            {
                var staffList = GetClockData(ctx.Staff.Include("StaffClockData").Include("StaffLeaveData").Include("StaffHoursData").ToList(), new FaceID().DataRetrieve()).ToList();
                DeductHoursFromLeave(staffList);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }





        /*
          //// Need to remember that if it is a holiday, skip sending reminders.
            //// Need to check if staff are on leave and not send reminders

            //        // It is important to use a try/catch here as we still want to save the data that follows after the return here.
            //        // TODO: Perhaps we need some threading or delegate here?
            //        try
            //        {

            //            if (DateTime.Now.TimeOfDay > new TimeSpan(0, 08, 1, 0) || DateTime.Now.TimeOfDay < new TimeSpan(0, 08, 15, 0) || DateTime.Now.TimeOfDay < new TimeSpan(0, 08, 46, 0))
            //            {
            //                //Here we look for late staff and send them a reminder to clock in.
            //                SendLateEmails(staffList);
            //            }

            //            if (DateTime.Now.TimeOfDay > new TimeSpan(0, 13, 30, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 13, 55, 0)) //if (DateTime.Now.Hour > 17)
            //            {
            //                //Here we look for staff that have NOT clocked in yet and send them a leave application message.
            //                SendLeaveEmails(staffList);
            //            }

            //            if (DateTime.Now.TimeOfDay > new TimeSpan(0, 14, 0, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 14, 15, 0))
            //            {
            //                //This method will only run at 5pm
            //                //it automatically applies leave for a staff member who has not clocked in on the day
            //                AutoLeaveApp(staffList);
            //            }

            //            if (DateTime.Now.TimeOfDay > new TimeSpan(0, 17, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 17, 10, 0))
            //            {
            //                //this method will only work at 11 pm. This will Remind a user of a missed "clock in" if they do not clock back in after lunch or home time etc
            //                ProcessDailyClockRecords(staffList);
            //            }

            //            if (DateTime.Now.Hour > PreviousSyncTime.Hour)
            //            {
            //                SyncTimes();
            //            }

            //            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday && DateTime.Now.TimeOfDay > new TimeSpan(0, 17, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 17, 10, 0))
            //            {
            //                using (var client = new HttpClient())
            //                {
            //                    var URI = WebConfigurationManager.AppSettings["ProcessWeeklyManagerMailURL"];
            //                    var serializedContact = "";
            //                    var content = new StringContent(serializedContact, Encoding.UTF8, "application/json");
            //                    var result = client.PostAsync(URI, content);
            //                }
            //            }
            //            if (DateTime.Now.TimeOfDay > new TimeSpan(0, 22, 1, 0) && DateTime.Now.TimeOfDay < new TimeSpan(0, 22, 10, 0))
            //            {
            //                DeductHoursFromLeave(staffList);
            //            }

            //        }
            //        catch (Exception ex)
            //        {
            //            Trace.Write(ex);
            //            throw;
            //        }
            return true;
         */

        #region

        /// <summary>
        /// Processes the clocking data
        /// Does NOT send emails
        /// </summary>
        /// <param name="fileName">Let's you specify a data file to read from instead of the IP addresses.</param>
        public static bool ProcessClocking(String fileName)
        {
            GetClockData(ctx.Staff, new FaceID().DataRetrieve(fileName));

            return true;
        }

        /// <summary>
        /// Processes the clocking data
        /// Does NOT send emails
        /// </summary>
        /// <param name="fileList">Let's you specify a list of data files to read from instead of the IP addresses.</param>
        public static bool ProcessClocking(IList<String> fileList)
        {
            GetClockData(ctx.Staff, new FaceID().DataRetrieve(fileList));

            return true;
        }

        #endregion

        #endregion

        public static byte[] ExportClockDataToExcel(IList selectedClockData)
        {
            try
            {
                byte[] theCsvReport;
                string appRootDir = AppDomain.CurrentDomain.BaseDirectory;


                #region Pdf Write 2
                //using (MemoryStream ms = new MemoryStream())
                //{
                // Document document = new Document(PageSize.A4, 0, 0, 0, 0);
                // StringReader sr = new StringReader(markup);
                // PdfWriter writer = PdfWriter.GetInstance(document, ms);
                // PdfWriter.GetInstance(document, new FileStream(AppDomain.CurrentDomain.BaseDirectory + Guid.NewGuid() + ".pdf", FileMode.Create));

                // PdfAction action = new PdfAction(PdfAction.PRINTDIALOG);
                // writer.SetOpenAction(action);
                // document.Open();
                // XMLWorkerHelper.GetInstance().ParseXHtml(
                //writer, document, sr
                //);
                // document.Close();
                //theCsvReport = ms.GetBuffer();
                //}

                #endregion
                #region Pdf write
                /*
                Document document = new Document();
                StringReader sr = new StringReader(markup);
                PdfWriter writer = PdfWriter.GetInstance(document,
                    new FileStream(appRootDir + "Nkabinde.txt", FileMode.Create));
                document.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(
                    writer, document, sr
                    );
                document.Close();
                */
                #endregion

                using (MemoryStream ms = new MemoryStream())
                {
                    using (StreamWriter csvStreamWriter = new StreamWriter(ms))
                    {
                        var message = new StringBuilder().AppendLine("Staff Name, Date, Time Worked, Over Time, Time Debt");

                        csvStreamWriter.WriteLine("Staff Name, Date, Time Worked, Over Time, Time Debt");
                        csvStreamWriter.WriteLine();
                    }

                    theCsvReport = ms.GetBuffer();
                }

                return theCsvReport;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static bool RegisterUserToClockingDevice(StaffModel staff, string clockDeviceIp, int deviceNumber)
        {
            #region
            IList<FaceData> cardStringListValue = new List<FaceData>();

            var record = new FaceUser();
            var cardString =
                File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory +
                                           "\\Utilities\\clockCardStringData.txt");
            var cardStringRefined = cardString.Replace("\r\n", "")
                .Replace(" ", "|")
                .Replace("face_data=", "|face_data=")
                .Replace("||", "|");
            var cardData = cardStringRefined.Split('|');
            string TmpStr = "";
            int QuoteIdx = 0;
            int StrCount = 0;
            for (int i = 0; i < cardData.Length; i++)
            {
                if (cardData[i] == "") continue;

                TmpStr = cardData[i];
                QuoteIdx = TmpStr.IndexOf('\"') + 1;
                StrCount = TmpStr.LastIndexOf("\"") - QuoteIdx;
                var Data = TmpStr.Substring(0, TmpStr.IndexOf('='));
                var Value = TmpStr.Substring(QuoteIdx, StrCount);
                switch (Data)
                {
                    case "face_data":
                        {
                            record.FaceData.Add(new FaceData { Data = Value });
                            break;
                        }
                    default:
                        break;
                }
            }
            #endregion

            var cardNum = staff.StaffClockCardNumber ?? DateTime.Now.Year + 1;
            var faceDataModel = new FaceUser()
           {
               FaceAuthority = "0X0",
               FaceCalID = "0",
               FaceCardNumber = "0X" + new FaceID().ConvertCardNumberToHex(cardNum),
               FaceCheckType = "card",
               FaceData = record.FaceData,
               FaceDoorType = "card",
               FaceID = staff.StaffClockId,
               FaceName = staff.StaffNtName,
           };

            var result = new FaceID().DeleteUser("1", clockDeviceIp, staff.StaffClockId.ToString());
            if (!result)
            {
                result = new FaceID().RegisterUserOnClockingDevice(clockDeviceIp, staff.StaffClockId, staff.StaffClockCardNumber.ToString());
                if (!result)
                    result = new FaceID().UploadTemplates(deviceNumber.ToString(), clockDeviceIp, faceDataModel, staff.StaffClockId.ToString(), "0X" + new FaceID().ConvertCardNumberToHex(cardNum));
            }
            else
            {
                result = new FaceID().UploadTemplates(deviceNumber.ToString(), clockDeviceIp, faceDataModel, staff.StaffClockId.ToString(), staff.StaffClockCardNumber.ToString());
            }

            if (result)
                return result;

            return false;
        }


        public static bool CopyProfileToOtherDevices(StaffModel staff, int deviceNumber, string deviceIp)
        {
            //ToDo: Copy profile to toher devices
            #region
            IList<FaceData> cardStringListValue = new List<FaceData>();

            var record = new FaceUser();
            var cardString =
                File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory +
                                           "\\Utilities\\clockCardStringData.txt");
            var cardStringRefined = cardString.Replace("\r\n", "")
                .Replace(" ", "|")
                .Replace("face_data=", "|face_data=")
                .Replace("||", "|");
            var cardData = cardStringRefined.Split('|');
            string TmpStr = "";
            int QuoteIdx = 0;
            int StrCount = 0;
            for (int i = 0; i < cardData.Length; i++)
            {
                if (cardData[i] == "") continue;

                TmpStr = cardData[i];
                QuoteIdx = TmpStr.IndexOf('\"') + 1;
                StrCount = TmpStr.LastIndexOf("\"") - QuoteIdx;
                var Data = TmpStr.Substring(0, TmpStr.IndexOf('='));
                var Value = TmpStr.Substring(QuoteIdx, StrCount);
                switch (Data)
                {
                    case "face_data":
                        {
                            record.FaceData.Add(new FaceData { Data = Value });
                            break;
                        }
                    default:
                        break;
                }
            }
            #endregion
            var cardNum = staff.StaffClockCardNumber ?? DateTime.Now.Year + 1;
            var faceDataModel = new FaceUser()
          {
              FaceAuthority = "0X0",
              FaceCalID = "0X0",
              FaceCardNumber = "0X" + new FaceID().ConvertCardNumberToHex(cardNum),
              FaceCheckType = "card",
              FaceData = record.FaceData,
              FaceDoorType = "card",
              FaceID = staff.StaffClockId,
              FaceName = staff.StaffNtName
          };

            foreach (ClockDevice clockingDevice in config.ClockingDevices)
            {
                if (clockingDevice.ipAddress.Equals(deviceIp))
                    continue;
                var result = new FaceID().DeleteUser("1", clockingDevice.ipAddress, staff.StaffClockId.ToString());
                var res = new FaceID().UploadTemplates(clockingDevice.Number, clockingDevice.ipAddress, faceDataModel, staff.StaffClockId.ToString(), "0X" + new FaceID().ConvertCardNumberToHex(cardNum));
                if (res)
                {
                    continue;
                }
                else
                {
                    Trace.WriteLine("Could not load profile to device: " + clockingDevice.ipAddress);
                }
            }
            //var result = new FaceID().UploadTemplates(deviceNumber.ToString(), deviceIp, faceDataModel,staff.StaffClockId.ToString(),staff.StaffClockCardNumber.ToString());
            return true;
        }

        public static object GetTemplpate(string s, string s1, string s2)
        {

            #region
            IList<FaceData> cardStringListValue = new List<FaceData>();

            var record = new FaceUser();
            var cardString =
                File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory +
                                           "\\Utilities\\clockCardStringData.txt");
            var cardStringRefined = cardString.Replace("\r\n", "")
                .Replace(" ", "|")
                .Replace("face_data=", "|face_data=")
                .Replace("||", "|");
            var cardData = cardStringRefined.Split('|');
            string TmpStr = "";
            int QuoteIdx = 0;
            int StrCount = 0;
            for (int i = 0; i < cardData.Length; i++)
            {
                if (cardData[i] == "") continue;

                TmpStr = cardData[i];
                QuoteIdx = TmpStr.IndexOf('\"') + 1;
                StrCount = TmpStr.LastIndexOf("\"") - QuoteIdx;
                var Data = TmpStr.Substring(0, TmpStr.IndexOf('='));
                var Value = TmpStr.Substring(QuoteIdx, StrCount);
                switch (Data)
                {
                    case "face_data":
                        {
                            record.FaceData.Add(new FaceData { Data = Value });
                            break;
                        }
                    default:
                        break;
                }
            }
            #endregion
            var testUser = new FaceUser()
            {
                FaceAuthority = "0X0",
                FaceCalID = "0",
                FaceCardNumber = "0X" + new FaceID().ConvertCardNumberToHex(0005208236), //"0XAC784F00*/ "0Xac784f00"
                FaceCheckType = "card",
                FaceData = record.FaceData,
                FaceDoorType = "card",
                FaceID = 210,
                FaceName = "xtester",


            };
            //var te22 = new FaceID().ConvertHexCardNumber(testUser.FaceCardNumber);


            //var uploadResult = new FaceID().UploadTemplates("1", "172.16.0.110", testUser, "210", "0X" + new FaceID().ConvertCardNumberToHex(0005208236));/* "0X" + new FaceID().ConvertCardNumberToHex(0009774383))*/;


            var downloadResult = new FaceID().DownloadTemplates("1", "172.16.0.110", "217"); //110 = "0X2f259500"






            // var ttee = new FaceID().ConvertHexCardNumber(downloadResult.FaceCardNumber); //9774390
            //var te2 = new FaceID().ConvertHexCardNumber(tet2);//38181

            var delres = new FaceID().DeleteUser("1", "172.16.0.114", "217");

            //var test1 = new FaceID().ConvertHexAuthorityNumber(downloadResult.FaceAuthority); //"0Xff"
            //var test2 = new FaceID().ConvertHexCardNumber(downloadResult.FaceCardNumber); //"0Xbd134d00"




            return downloadResult;
        }
    }
}