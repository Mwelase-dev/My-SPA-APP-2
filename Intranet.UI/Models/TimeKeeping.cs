using CsvHelper;
using Intranet.Business;
using Intranet.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml;
//using CsvHelper;
//using DocumentFormat.OpenXml.Drawing;
//using ExporterObjects;
using Intranet.Data.EF;
using Intranet.Models.Enums;
using Intranet.UI.Utilities;
//using NHibernate.Linq;
using NHibernate.Linq;
using Utilities;
using Utilities.Helpers;

namespace Intranet.UI.Models
{
    public class TimeKeepingContainer
    {
        #region Properties
        //  static  { get; set; }
        private static List<TimeKeepingItem> Items { get; set; }

        public List<TimeKeepingItem> TimeKeepingItems { get { return Items.OrderByDescending(m => m.Date).ThenBy(m => m.Date.Hour).ToList(); } }
        #endregion

        #region Constructors

        public TimeKeepingContainer()
        {
            // ClockingDays = new List<DateTime>();
            Items = new List<TimeKeepingItem>();
        }
        public TimeKeepingContainer(IEnumerable<StaffModel> staffModels)
            : this()
        {
            // staffModels = staffModels.Where(m => m.StaffClockId == 110 || m.StaffClockId == 111 || m.StaffClockId == 112);

            ProcessStaffModels(staffModels);
        }

        #endregion

        #region Private methods
        /// <summary>
        /// flatens the staffmodel object
        /// Process the staff models
        /// gets unique clocking days
        /// builds time keeping items
        /// </summary>
        /// <param name="staffModels">list of staffmodel types</param>
        private static void ProcessStaffModels(IEnumerable<StaffModel> staffModels)
        {
            #region For each employee process time keeping

            var changeColor = false;

            //todo: jay - try to reduce the loops(Try some hard core linq)
            //todo: get working hours for an individual
            // var timeKeepData = staffModels.ToList();
            foreach (var staffModel in staffModels.OrderBy(m => m.StaffClockId).ToList())
            {

                var userClockData = StaffClockDataUtils.MergeClockData(staffModel);

                //get unique clocking days
                var clockData = GetClockingDays(userClockData).OrderBy(m => m.Date);


                foreach (var dateTime in clockData)
                {
                    //gets all the reocrds for the day
                    var dayData = userClockData.Distinct().Where(m => m.ClockDateTime.Date == dateTime).OrderBy(m => m.ClockDateTime).ToList();

                    //process the user's daily clock record
                    var isFirstclockinRecord = true;
                    var clockTimesCount = dayData.Count();
                    // used to calculate the time a person left work
                    var clockItemsCount = 0;
                    foreach (var cd in dayData)
                    {
                        clockItemsCount++;

                        var tki = new TimeKeepingItem(cd.ClockDataId, staffModel.StaffId, staffModel.StaffClockId, staffModel.StaffName, staffModel.StaffSurname, cd.ClockDateTime, cd.DataStatus);

                        if (isFirstclockinRecord)
                        {
                            tki.TimeTaken = IsNotPanctual(staffModel, cd.ClockDateTime);
                            isFirstclockinRecord = false;
                        }

                        //todo:jay- config?
                        //tki.RowColor = cd.IsLeaveRecord ? "#FF0000" : changeColor ? "#c4f367" : "#cfd7f3";
                        tki.RowColor = cd.IsLeaveRecord ? "#FF0000" : changeColor ? "#CC3300" : "#E1EEF4";

                        //checks last clock in time to see
                        //if (clockItemsCount == clockTimesCount && dayData.Count() > 1)
                        //{
                            //used for the totals
                            double overtTimeMinutes = 0;
                            double timeDebtMinutes = 0;


                            tki.TimeTaken = LeftWorkEarly(staffModel, cd.ClockDateTime);

                            tki.DispalyOvertimeWorked = CalculateOverTime(staffModel, dayData.Select(m => m.ClockDateTime).ToList(), out  overtTimeMinutes);
                            tki.DisplayTimeDebt = CalculateTimeDebt(staffModel, dayData.Select(m => m.ClockDateTime).ToList(), out timeDebtMinutes);
                            tki.DisplayTimeWorked = CalculateTimeWorked(staffModel, dayData.Select(m => m.ClockDateTime).ToList());

                            tki.OverTimeInMinutes = overtTimeMinutes;
                            tki.TimeDebtInMinutes = timeDebtMinutes;
                        //}

                        Items.Add(tki);

                    }

                    changeColor = !changeColor;
                }

            }

            #endregion
        }


        /// <summary>
        /// checks if date parameter is in unique clocking days
        /// </summary>
        /// <param name="clockModels"></param>
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

        private static DateTime? IsNotPanctual(StaffModel staff, DateTime date)
        {
            //get staff's day start time
            var dayStart = (from s in staff.StaffHoursData
                            where s.DayId.Equals((int)date.DayOfWeek)
                            select s.DayTimeStart).FirstOrDefault();

            //todo:Jay - Ask Quentin what happens the user has not start time
            if (dayStart == null)
                throw new ArgumentNullException("dayStart");

            var dayStartSpan = new TimeSpan(0, dayStart.Hour, dayStart.Minute, 0);
            var currentDaySpan = new TimeSpan(0, date.Hour, date.Minute, 0);

            if (dayStartSpan.Hours > currentDaySpan.Hours)
                return null;

            var timeDff = (currentDaySpan - dayStartSpan);

            return new DateTime(2013, 1, 1, timeDff.Hours, System.Math.Abs(timeDff.Minutes), timeDff.Seconds);
        }


        /// <summary>
        /// Check the users last clocking time to see if the knocked off early
        ///  </summary>
        /// <param name="staff"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        private static DateTime? LeftWorkEarly(StaffModel staff, DateTime date)
        {
            //get staff's day start time
            var dayEnd = (from s in staff.StaffHoursData
                          where s.DayId.Equals((int)date.DayOfWeek)
                          select s.DayTimeEnd).FirstOrDefault();

            //todo:Jay - Ask Quentin what happens the user has not start time
            if (dayEnd == null)
                throw new ArgumentNullException("dayEnd");

            var dayEndSpan = new TimeSpan(0, dayEnd.Hour, dayEnd.Minute, 0);
            var currentDaySpan = new TimeSpan(0, date.Hour, date.Minute, 0);

            if (dayEndSpan < currentDaySpan)
                return null;

            //todo: jay - need to create a property in order to display negertive minutes
            var timeDiff = dayEndSpan - currentDaySpan;

            return new DateTime(2013, 1, 1, timeDiff.Hours, timeDiff.Minutes, timeDiff.Seconds);


        }


        private static double CalculateWeekRequiredHours(StaffModel staff, List<DateTime> clockingTimes)
        {
            var dayHours = (from h in staff.StaffHoursData
                            where h.DayId.Equals((int)clockingTimes.First().DayOfWeek)
                            select h).FirstOrDefault();

            if (dayHours == null)
            {
                return 0;
            }

            //the check to see if staff member has worked overtime
            var lunchMinutes = dayHours.DayLunchLength;
            //var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes) * 60;// Here is the problem ppl like do not have that hour to minus as a lunch alocation so this h4 was hardcoded
            var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - (lunchMinutes));

            return requiredHoursInMinutes;
        }

        /// <summary>
        /// Calculate the time a staff member
        /// has worked back
        /// </summary>
        /// <param name="staff"></param>
        /// <param name="toList"></param>
        /// <param name="clockingTimes"></param>
        /// <param name="overtimeMinutes"></param>
        /// <returns></returns>
        private static string CalculateOverTime(StaffModel staff, List<int> toList, List<DateTime> clockingTimes, out double overtimeMinutes)
        {
            overtimeMinutes = 0;

            // dont calculate time worked back if user's clocking Date is odd
            if ((clockingTimes.Count() % 2) != 0 || clockingTimes.Count == 1)
                return "*";

            clockingTimes = clockingTimes.OrderBy(m => m.Hour).ThenBy(m => m.Minute).ToList();

            //todo;Jay - refactor asap
            var hoursWorked = 0;
            var minutesWorked = 0;

            for (var i = 0; i < clockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < clockingTimes.Count)
                {
                    var startSpan = new TimeSpan(0, clockingTimes[i].Hour, clockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, clockingTimes[i + 1].Hour, clockingTimes[i + 1].Minute, 0);

                    var diff = endSpan - startSpan;

                    hoursWorked += diff.Hours;
                    minutesWorked += diff.Minutes;
                }
            }

            //if (StaffClockDataUtils.IsWeekend(clockingTimes.First()))
            //    return new TimeSpan(hoursWorked, minutesWorked, 0).ToString(@"hh\:mm");

            var publicHolidays = clockingTimes.First().Date.ThePublicHolidays(clockingTimes.First().Date);


            //todo:jay - null possibiliy
            var dayHours = (from h in staff.StaffHoursData
                            where h.DayId.Equals((int)clockingTimes.First().DayOfWeek)
                            select h).FirstOrDefault();
 
            var lunchMinutes = 0;
            if (dayHours != null)
            {
                 lunchMinutes = dayHours.DayLunchLength;
            }
          
           
            //var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes) * 60;// Here is the problem ppl like do not have that hour to minus as a lunch alocation so this h4 was hardcoded
            var requiredHoursInMinutes = 0;
            if (dayHours != null)
            {
                 requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes);
            }

            if (publicHolidays.Any(x => x.Date == clockingTimes.First().Date)  && toList.Any(x=>x  != 5))
            {
                 requiredHoursInMinutes = 0;
                 lunchMinutes = 0;
            }
             

            var hoursWorkerInMinutes = ((int)hoursWorked * 60) + minutesWorked;

            //if (StaffClockDataUtils.IsWeekend(clockingTimes.First()) || publicHolidays.Any(x => x.Date == clockingTimes.First().Date))
                //requiredHoursInMinutes = 0;
            //var publicHolidays = clockingTimes.First().Date.ThePublicHolidays(clockingTimes.Last().Date);
            //if (publicHolidays.Any(x => x.Date == clockingTimes.First().Date)){
            //    requiredHoursInMinutes = 0;
            //}
                 
                //requiredHoursInMinutes = 0;

            if (hoursWorkerInMinutes <= requiredHoursInMinutes)
                return "0";

            var overTimeDif = TimeSpan.FromMinutes(hoursWorkerInMinutes - requiredHoursInMinutes);

            overtimeMinutes = overTimeDif.TotalMinutes;



           


            return overTimeDif.ToString(@"hh\h\ mm");



        }

        /// <summary>
        /// Calculate the time a staff member
        /// has worked back
        /// </summary>
        /// <param name="staff"></param>
        /// <param name="toList"></param>
        /// <param name="clockingTimes"></param>
        /// <param name="overtimeMinutes"></param>
        /// <returns></returns>
        private static string CalculateOverTime(StaffModel staff, List<DateTime> clockingTimes, out double overtimeMinutes)
        {
            overtimeMinutes = 0;

            // dont calculate time worked back if user's clocking Date is odd
            if ((clockingTimes.Count() % 2) != 0 || clockingTimes.Count == 1)
                return "*";

            clockingTimes = clockingTimes.OrderBy(m => m.Hour).ThenBy(m => m.Minute).ToList();

            //todo;Jay - refactor asap
            var hoursWorked = 0;
            var minutesWorked = 0;




            for (var i = 0; i < clockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < clockingTimes.Count)
                {
                    var startSpan = new TimeSpan(0, clockingTimes[i].Hour, clockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, clockingTimes[i + 1].Hour, clockingTimes[i + 1].Minute, 0);

                    var diff = endSpan - startSpan;

                    hoursWorked += diff.Hours;
                    minutesWorked += diff.Minutes;
                }
            }

            //if (StaffClockDataUtils.IsWeekend(clockingTimes.First()))
            //    return new TimeSpan(hoursWorked, minutesWorked, 0).ToString(@"hh\:mm");

            var publicHolidays = clockingTimes.First().Date.ThePublicHolidays(clockingTimes.First().Date);


            //todo:jay - null possibiliy
            var dayHours = (from h in staff.StaffHoursData
                            where h.DayId.Equals((int)clockingTimes.First().DayOfWeek)
                            select h).FirstOrDefault();

            var lunchMinutes = 0;
            if (dayHours != null)
            {
                lunchMinutes = dayHours.DayLunchLength;
            }


            //var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes) * 60;// Here is the problem ppl like do not have that hour to minus as a lunch alocation so this h4 was hardcoded
            var requiredHoursInMinutes = 0;
            if (dayHours != null)
            {
                requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes);
            }

            if (publicHolidays.Any(x => x.Date == clockingTimes.First().Date))
            {
                requiredHoursInMinutes = 0;
                lunchMinutes = 0;
            }


            var hoursWorkerInMinutes = ((int)hoursWorked * 60) + minutesWorked;

            //if (StaffClockDataUtils.IsWeekend(clockingTimes.First()) || publicHolidays.Any(x => x.Date == clockingTimes.First().Date))
            //requiredHoursInMinutes = 0;
            //var publicHolidays = clockingTimes.First().Date.ThePublicHolidays(clockingTimes.Last().Date);
            //if (publicHolidays.Any(x => x.Date == clockingTimes.First().Date)){
            //    requiredHoursInMinutes = 0;
            //}

            //requiredHoursInMinutes = 0;

            if (hoursWorkerInMinutes <= requiredHoursInMinutes)
                return "0";

            var overTimeDif = TimeSpan.FromMinutes(hoursWorkerInMinutes - requiredHoursInMinutes);

            overtimeMinutes = overTimeDif.TotalMinutes;






            return overTimeDif.ToString(@"hh\h\ mm");



        }
        private static double CalculateOverTimeInMinutes(StaffModel staff, List<DateTime> clockingTimes)
        {
            double overtimeMinutes = 0;

            // dont calculate time worked back if user's clocking Date is odd
            if ((clockingTimes.Count() % 2) != 0)
                return 0;

            clockingTimes = clockingTimes.OrderBy(m => m.Hour).ThenBy(m => m.Minute).ToList();

            //todo;Jay - refactor asap
            var hoursWorked = 0;
            var minutesWorked = 0;

            for (var i = 0; i < clockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < clockingTimes.Count)
                {
                    var startSpan = new TimeSpan(0, clockingTimes[i].Hour, clockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, clockingTimes[i + 1].Hour, clockingTimes[i + 1].Minute, 0);

                    var diff = endSpan - startSpan;

                    hoursWorked += diff.Hours;
                    minutesWorked += diff.Minutes;
                }
            }

            if (StaffClockDataUtils.IsWeekend(clockingTimes.First()))
                return new TimeSpan(hoursWorked, minutesWorked, 0).Minutes;

            //todo:jay - null possibiliy
            var dayHours = (from h in staff.StaffHoursData
                            where h.DayId.Equals((int)clockingTimes.First().DayOfWeek)
                            select h).FirstOrDefault();

            if (dayHours == null)
            {
                return 0;
            }

            //the check to see if staff member has worked overtime
            var lunchMinutes = dayHours.DayLunchLength;
            //var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes) * 60;// Here is the problem ppl like do not have that hour to minus as a lunch alocation so this h4 was hardcoded
            var requiredHoursInMinutes = ((int)dayHours.DayHoursRequired - lunchMinutes);


            var hoursWorkerInMinutes = ((int)hoursWorked * 60) + minutesWorked;

            if (hoursWorkerInMinutes <= requiredHoursInMinutes)
                return 0;

            var overTimeDif = TimeSpan.FromMinutes(hoursWorkerInMinutes - requiredHoursInMinutes);

            overtimeMinutes = overTimeDif.TotalMinutes;



            return overTimeDif.Minutes;



        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="staff"></param>
        /// <param name="clockingTimes"></param>
        /// <returns></returns>
        private static string CalculateTimeWorked(StaffModel staff, IList<DateTime> clockingTimes)
        {
            if ((clockingTimes.Count() % 2) != 0 || clockingTimes.Count == 1)
                return "*";

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
        private static double CalculateTimeWorkedInMinutes(StaffModel staff, IList<DateTime> clockingTimes)
        {
            if ((clockingTimes.Count() % 2) != 0)
                return 0;

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

            return minutesWorked;
        }

        /// <summary>
        /// Calculate time debt on a day basis
        /// </summary>
        /// <param name="staff"></param>
        /// <param name="dayClockingTimes"></param>
        /// <param name="timeDebtminutes"></param>
        /// <returns></returns>
        private static string CalculateTimeDebt(StaffModel staff, IList<DateTime> dayClockingTimes, out double timeDebtminutes)
        {

            timeDebtminutes = 0;

            if ((dayClockingTimes.Count() % 2) != 0 || dayClockingTimes.Count == 1)
                return "*";

            //if it's weekend you can not have time debt
            if (StaffClockDataUtils.IsWeekend(dayClockingTimes.First()))
                return "0";

            var publicHolidays = dayClockingTimes.First().ThePublicHolidays(dayClockingTimes.First());

            if (publicHolidays.Any(x => x.Date == dayClockingTimes.First().Date))
                return "0";
       

            var hoursWorked = 0;
            var minutesWorked = 0;

            for (var i = 0; i < dayClockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < dayClockingTimes.Count())
                {
                    var startSpan = new TimeSpan(0, dayClockingTimes[i].Hour, dayClockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, dayClockingTimes[i + 1].Hour, dayClockingTimes[i + 1].Minute, 0);

                    var spanDif = endSpan - startSpan;

                    hoursWorked += spanDif.Hours < 0 ? (spanDif.Hours) : spanDif.Hours;
                    minutesWorked += spanDif.Minutes < 0 ? (spanDif.Minutes) : spanDif.Minutes;
                }
            }

            var dayHours = from s in staff.StaffHoursData
                           where s.DayId.Equals((int)dayClockingTimes.First().DayOfWeek)
                           select s;
            //todo:jay - consider weekends
            var dayData = dayHours.FirstOrDefault();

            if (dayData == null)
                throw new Exception("No working hours found for the selecetd staff member.");

            var lunchMinutes = dayData.DayLunchLength;

            var timeWorkedInMinutes = minutesWorked + (hoursWorked * 60);
            var workingHoursInMinutes = (dayData.DayHoursRequired - lunchMinutes);

            if (timeWorkedInMinutes >= workingHoursInMinutes)
                return " ";

            var span = TimeSpan.FromMinutes(timeWorkedInMinutes - workingHoursInMinutes);

            timeDebtminutes = span.TotalMinutes;

            return span.ToString(@"hh\h\ mm");
            //return string.Format("{0:hh\\:mm}", span);
        }
        private static double CalculateTimeDebtInMinutes(StaffModel staff, IList<DateTime> dayClockingTimes)
        {

            double timeDebtminutes = 0;

            if ((dayClockingTimes.Count() % 2) != 0)
                return 0;

            //if it's weekend you can not have time debt
            if (StaffClockDataUtils.IsWeekend(dayClockingTimes.First()))
                return 0;

            var hoursWorked = 0;
            var minutesWorked = 0;

            for (var i = 0; i < dayClockingTimes.Count(); i++)
            {
                if ((i % 2) == 0 && i + 1 < dayClockingTimes.Count())
                {
                    var startSpan = new TimeSpan(0, dayClockingTimes[i].Hour, dayClockingTimes[i].Minute, 0);
                    var endSpan = new TimeSpan(0, dayClockingTimes[i + 1].Hour, dayClockingTimes[i + 1].Minute, 0);

                    var spanDif = endSpan - startSpan;

                    hoursWorked += spanDif.Hours < 0 ? (spanDif.Hours) : spanDif.Hours;
                    minutesWorked += spanDif.Minutes < 0 ? (spanDif.Minutes) : spanDif.Minutes;
                }
            }

            var dayHours = from s in staff.StaffHoursData
                           where s.DayId.Equals((int)dayClockingTimes.First().DayOfWeek)
                           select s;
            //todo:jay - consider weekends
            var dayData = dayHours.FirstOrDefault();

            if (dayData == null)
                throw new Exception("No working hours for the selecetd staff were found");

            var lunchMinutes = dayData.DayLunchLength;

            var timeWorkedInMinutes = minutesWorked + (hoursWorked * 60);
            var workingHoursInMinutes = (dayData.DayHoursRequired - lunchMinutes);

            if (timeWorkedInMinutes >= workingHoursInMinutes)
                return 0;

            var span = TimeSpan.FromMinutes(timeWorkedInMinutes - workingHoursInMinutes);

            timeDebtminutes = span.TotalMinutes;

            return timeDebtminutes;
        }

        #endregion

        public static IEnumerable<StaffClockingContainer> GetStaffClockingData(StaffModel staffModel)
        {
            if (staffModel == null)
                throw new ArgumentNullException("staffModel");

            var userClockData = StaffClockDataUtils.MergeClockData(staffModel);

            if (userClockData == null)
                return default(IEnumerable<StaffClockingContainer>);
            //get unique clocking days
            var uniqueClockData = GetClockingDays(userClockData).OrderBy(m => m.Date);
            var clockDataContainer = new List<StaffClockingContainer>();

            var changeColor = false;

            foreach (var date in uniqueClockData)
            {
                //gets all the reocrds for the day
                var dayData = userClockData.Distinct().Where(m => m.ClockDateTime.Date == date).OrderBy(m => m.ClockDateTime).ToList();

                //process the user's daily clock record
                var isFirstclockinRecord = true;
                var clockTimesCount = dayData.Count();

                var clockContainerModel = new StaffClockingContainer(date, dayData.Count(m => m.IsLeaveRecord) > 0 ? dayData.FirstOrDefault().LeaveType : null);
                // used to calculate the time a person left work
                var clockItemsCount = 0;
                foreach (var cd in dayData)
                {
                    clockItemsCount++;
                    cd.DataStatusHighlight = cd.DataStatus;
                    var tki = new TimeKeepingItem(cd.ClockDataId, staffModel.StaffId, staffModel.StaffClockId, staffModel.StaffName, staffModel.StaffSurname, cd.ClockDateTime, cd.DataStatus,cd.DataStatusHighlight);

                    if (isFirstclockinRecord)
                    {
                        tki.TimeTaken = IsNotPanctual(staffModel, cd.ClockDateTime);
                        isFirstclockinRecord = false;
                    }

                    //todo:jay- config?
                    tki.RowColor = cd.IsLeaveRecord ? "#FF0000" : "#00496B";

                    var publicHolidays = dayData.First().ClockDateTime.Date.ThePublicHolidays(dayData.First().ClockDateTime.Date);

                    //checks last clock in time to see
                    if (clockItemsCount == clockTimesCount && dayData.Count() > 1 )
                    {
                        //todo - jay: refactor
                        //used for the daily totals
                        double overtime = 0;
                        double timeDebt = 0;

                        tki.TimeTaken = LeftWorkEarly(staffModel, cd.ClockDateTime);

                        tki.DispalyOvertimeWorked = CalculateOverTime(staffModel, dayData.Select(m => m.DataStatus).ToList(), dayData.Select(m => m.ClockDateTime).ToList(), out overtime);
                        tki.DisplayTimeDebt = CalculateTimeDebt(staffModel, dayData.Select(m => m.ClockDateTime).ToList(), out timeDebt);
                        tki.DisplayTimeWorked = CalculateTimeWorked(staffModel, dayData.Select(m => m.ClockDateTime).ToList());



                        tki.OverTimeInMinutes = overtime;
                        tki.TimeDebtInMinutes = timeDebt;
                         
                    }

                    // Items.Add(tki);
                    if (cd.LeaveType != null) clockContainerModel.LeaveType = (int)cd.LeaveType;
                    clockContainerModel.IsLeaveRecord = cd.IsLeaveRecord;
                    clockContainerModel.IsPublicHoliday = cd.IsPublicHoldiday;


                    if (dayData.Count() == 1)
                    {
                        tki.DispalyOvertimeWorked = "*";
                        tki.DisplayTimeDebt = "*";
                    }
                    clockContainerModel.TimeKeepingItems.Add(tki);

                   
                     

                }

                clockDataContainer.Add(clockContainerModel);

                changeColor = !changeColor;
            }


            return clockDataContainer;
        }

        public static IEnumerable<StaffClockingContainer> GetStaffClockingDataForClockingGraphs(StaffModel staffModel)
        {
            if (staffModel == null)
                throw new ArgumentNullException("staffModel");

            var userClockData = StaffClockDataUtils.MergeClockData(staffModel);
            //get unique clocking days
            var uniqueClockData = GetClockingDays(userClockData).OrderBy(m => m.Date);
            var clockDataContainer = new List<StaffClockingContainer>();

            var changeColor = false;

            foreach (var date in uniqueClockData)
            {
                //gets all the reocrds for the day
                var dayData = userClockData.Distinct().Where(m => m.ClockDateTime.Date == date).OrderBy(m => m.ClockDateTime).ToList();

                //process the user's daily clock record
                var isFirstclockinRecord = true;
                var clockTimesCount = dayData.Count();

                //var clockContainerModel = new StaffClockingContainer(date, dayData.Count(m => m.IsLeaveRecord) > 0 ? dayData.FirstOrDefault().LeaveType : null);
                var clockContainerModel = new StaffClockingContainer(date, dayData.Count(m => m.IsLeaveRecord) > 0 ? dayData.FirstOrDefault().LeaveType : null);
               

                
                // used to calculate the time a person left work
                var clockItemsCount = 0;
                foreach (var cd in dayData)
                {
                    clockItemsCount++;

                    var tki = new TimeKeepingItem(cd.ClockDataId, staffModel.StaffId, staffModel.StaffClockId, staffModel.StaffName, staffModel.StaffSurname, cd.ClockDateTime, cd.DataStatus);

                    if (isFirstclockinRecord)
                    {
                        tki.TimeTaken = IsNotPanctual(staffModel, cd.ClockDateTime);
                        isFirstclockinRecord = false;
                    }

                    //todo:jay- config?
                    tki.RowColor = cd.IsLeaveRecord ? "#FF0000" : changeColor ? "#c4f367" : "#cfd7f3";

                    //checks last clock in time to see
                    if (clockItemsCount == clockTimesCount && dayData.Count() > 1)
                    {
                        //todo - jay: refactor
                        //used for the daily totals
                        double overtime = 0;
                        double timeDebt = 0;

                        tki.TimeTaken = LeftWorkEarly(staffModel, cd.ClockDateTime);

                        tki.DispalyOvertimeWorked = CalculateOverTime(staffModel, dayData.Select(m => m.ClockDateTime).ToList(), out overtime);
                        tki.DisplayTimeDebt = CalculateTimeDebt(staffModel, dayData.Select(m => m.ClockDateTime).ToList(), out timeDebt);
                        tki.DisplayTimeWorked = CalculateTimeWorked(staffModel, dayData.Select(m => m.ClockDateTime).ToList());

                        tki.OverTimeInMinutes = overtime;
                        tki.TimeDebtInMinutes = timeDebt;
                    }

                    // Items.Add(tki);
                    if (cd.LeaveType != null) clockContainerModel.LeaveType = (int) cd.LeaveType;
                    clockContainerModel.IsLeaveRecord = cd.IsLeaveRecord;
                    clockContainerModel.IsPublicHoliday = cd.IsPublicHoldiday;
                    clockContainerModel.TimeKeepingItems.Add(tki);

                }

                clockDataContainer.Add(clockContainerModel);

                changeColor = !changeColor;
            }

            return clockDataContainer;
        }

        public static byte[] ExportClockDataToExcel(IEnumerable<StaffClockingContainer> selectedClockData)
        {
            byte[] theCsvReport = new byte[0x100000];
            string appRootDir = AppDomain.CurrentDomain.BaseDirectory;
            #region Prior tests
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
            #region CSV write
            /*
                using (MemoryStream ms = new MemoryStream())
                {
                    //using (StreamWriter csvStreamWriter = new StreamWriter(ms))
                    //{
                    StreamWriter csvStreamWriter = new StreamWriter(ms, Encoding.UTF8);

                    var message =
                        new StringBuilder().AppendLine("Staff Name, Date, Time Clocked, Over Time, Time Debt\n\n");
                    IList<TimeKeepingItem> timeKeepingItems = new List<TimeKeepingItem>();

                    foreach (StaffClockingContainer staffClockingContainer in selectedClockData)
                    {
                        foreach (TimeKeepingItem tkiItem in staffClockingContainer.TimeKeepingItems)
                        {
                            timeKeepingItems.Add(tkiItem);
                        }
                    }



                    timeKeepingItems.ForEach(
                        (m) =>
                            message.AppendFormat("{0},{1},{2},{3},{4}\n", "MwelaseTesting", m.Day, m.TimeWorkedBack,
                                m.OverTimeInMinutes, m.OverTimeInMinutes));

                    csvStreamWriter.WriteLine("Staff Name, Date, Time Worked, Over Time, Time Debt");

                    // }

                    theCsvReport = ms.GetBuffer();

                }
                */
            #endregion
            #region CSV write 2
            /* 
                using (MemoryStream ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms, Encoding.UTF8);
                    try
                    {
                        #region String

                        var message =
                            new StringBuilder().AppendLine("Staff Name, Date, Time Clocked, Over Time, Time Debt\n\n");
                        IList<TimeKeepingItem> timeKeepingItems = new List<TimeKeepingItem>();

                        foreach (StaffClockingContainer staffClockingContainer in selectedClockData)
                        {
                            foreach (TimeKeepingItem tkiItem in staffClockingContainer.TimeKeepingItems)
                            {
                                timeKeepingItems.Add(tkiItem);
                            }
                        }



                        timeKeepingItems.ForEach(
                            (m) =>
                                message.AppendFormat("{0},{1},{2},{3},{4}\n", "MwelaseTesting", m.Day, m.TimeWorkedBack,
                                    m.OverTimeInMinutes, m.OverTimeInMinutes));

                        #endregion

                        sw.Write(message);
                        sw.Flush(); //otherwise you are risking empty stream
                        ms.Seek(0, SeekOrigin.Begin);

                    }
                    finally
                    {
                        sw.Dispose();
                    }
                    theCsvReport = ms.GetBuffer();
                }
                */
            #endregion
            #region Csv write 3
            /*
                using (MemoryStream ms = new MemoryStream())
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();

                    string output = jss.Serialize(selectedClockData);

                    XmlNode xml = jss.Deserialize<XmlNode>("{records:{record:" + output + "}}");

                    XmlDocument xmldoc = new XmlDocument();
                    //Create XmlDoc Object
                    xmldoc.LoadXml(xml.InnerXml);
                    //Create XML Steam 
                    var xmlReader = new XmlNodeReader(xmldoc);
                    DataSet dataSet = new DataSet();
                    //Load Dataset with Xml
                    dataSet.ReadXml(xmlReader);
                    //return single table inside of dataset

                    var csv = ToCSV(dataSet.Tables[0], ",");
                    StreamReader reader = new StreamReader(ms, Encoding.UTF8);

                    string textObject = reader.ReadToEnd();


                    File.WriteAllText(appRootDir + "/ExportedFile.txt", output);

                    theCsvReport = ms.GetBuffer();
                    return theCsvReport;
                }
                */
            #endregion
            #region Exporter
            /*
            try
            {
              
               

                using (MemoryStream ms = new MemoryStream(theCsvReport))
                {

                    List<Electronics> list = Electronics.GetData();
                    ExportList<Electronics> exp = new ExportList<Electronics>();

                    
                    using (var sw = new StreamWriter(ms, Encoding.UTF8))
                    {
                        exp.PathTemplateFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,@"exportsTemplates\electronics");
                        exp.ExportTo(list, ExportToFormat.HTML, "a.html");
                        exp.ExportTo(list, ExportToFormat.CSV, "a.csv");
                        exp.ExportTo(list, ExportToFormat.XML, "a.xml");
                        exp.ExportTo(list, ExportToFormat.Word2003XML, "a_2003.doc");
                        exp.ExportTo(list, ExportToFormat.Excel2003XML, "a_2003.xls");
                        exp.ExportTo(list, ExportToFormat.Excel2007, "a.xlsx");
                        exp.ExportTo(list, ExportToFormat.Word2007, "a.docx");
                        exp.ExportTo(list, ExportToFormat.itextSharpXML, "a.xml");
                        exp.ExportTo(list, ExportToFormat.PDFtextSharpXML, "a.pdf");
                        sw.Write(exp);
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                    }
                }
                //return default(byte[]);
                return theCsvReport;
            }
            catch (Exception e)
            {
                throw;
            }
             * public class Electronics
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Name { get; set; }

        public static List<Electronics> GetData()
        {
            return new List<Electronics>(){
                
            new Electronics() { Id = 1, Data = DateTime.Now.Date.AddDays(-3), Name = "TV" },
            new Electronics() { Id = 2, Data = DateTime.Now.Date.AddDays(-1), Name = "PC" },
            new Electronics() { Id = 3, Data = new DateTime(2003,11,5), Name = "Camera" }
            };

        }
    }
*/
            #endregion
            #endregion

            #region CSV Tools
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {

                IList<TimeKeepingItem> timeKeepingItems = new List<TimeKeepingItem>();

                foreach (StaffClockingContainer staffClockingContainer in selectedClockData)
                {
                    foreach (TimeKeepingItem tkiItem in staffClockingContainer.TimeKeepingItems)
                    {
                        timeKeepingItems.Add(tkiItem);
                    }
                }

                csvWriter.WriteRecords(timeKeepingItems);

                streamWriter.Flush();
                memoryStream.Position = 0;
                theCsvReport = memoryStream.GetBuffer();

            }

            return theCsvReport;

            #endregion
        }


        public static byte[] ExportLeaveBalancesToExcel(IQueryable<CompanyLeaveReport> company)
        {
            byte[] theCsvReport = new byte[0x100000];
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                IList<StaffLeaveReportModel> leaveBalances = new List<StaffLeaveReportModel>();

                IList<LeaveSchedule> leaveSchedules =new List<LeaveSchedule>();

                foreach (CompanyLeaveReport companyLeaveReport in company)
                {
                    foreach (DivisionLeaveReportModel divisionLeaveReportModel in companyLeaveReport.CompanyDivisions)
                    { 
                        foreach (StaffLeaveReportModel staffLeaveReportModel in divisionLeaveReportModel.DivisionStaff)
                        {
                            var leaveSchedule = new LeaveSchedule();

                            var stringLeaveDates = "";
                            foreach (var leaveDateTime in staffLeaveReportModel.LeaveDateTimes)
                            {
                                stringLeaveDates += leaveDateTime.StartDate.ToShortDateString() + " to " + leaveDateTime.EndDate.ToShortDateString() + ",  ";
                            }

                            leaveSchedule.Employee = staffLeaveReportModel.Fullname;
                            leaveSchedule.BroughtForward = staffLeaveReportModel.DaysAtEndOfMonth - staffLeaveReportModel.DaysTakenAfterEndDateInRange;
                            leaveSchedule.DaysAccrued = staffLeaveReportModel.DaysAccrued.Round(2);
                            leaveSchedule.DatesOfLeave = stringLeaveDates;
                            leaveSchedule.NumberOfDaysTaken = staffLeaveReportModel.DaysTaken;
                            leaveSchedule.CarriedForward = (((staffLeaveReportModel.DaysAtEndOfMonth + staffLeaveReportModel.DaysAccrued) - staffLeaveReportModel.DaysTaken).Round(2)) - staffLeaveReportModel.DaysTakenAfterEndDateInRange;
                            //leaveSchedule.CarriedForward = ((staffLeaveReportModel.DaysAtEndOfMonth + staffLeaveReportModel.DaysAccrued) - staffLeaveReportModel.DaysTaken).Round(2); 2016-01-20 T 13h25
                           // leaveSchedule.CarriedForward = ((staffLeaveReportModel.DynamicRunningTotal + staffLeaveReportModel.DaysAccrued) - staffLeaveReportModel.DaysTaken).Round(2);



                            leaveSchedules.Add(leaveSchedule);
                        }
                    }
                }


                csvWriter.WriteRecords(leaveSchedules);
                
                streamWriter.Flush();
                memoryStream.Position = 0;
                theCsvReport = memoryStream.GetBuffer();

            }

            return theCsvReport;
        }

        /*
          public static byte[] ExportLeaveBalancesToExcel(IQueryable<CompanyLeaveReport> company)
        {
            byte[] theCsvReport = new byte[0x100000];
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                IList<StaffLeaveReportModel> leaveBalances = new List<StaffLeaveReportModel>();

                IList<LeaveSchedule> leaveSchedules =new List<LeaveSchedule>();

                foreach (CompanyLeaveReport companyLeaveReport in company)
                {
                    foreach (DivisionLeaveReportModel divisionLeaveReportModel in companyLeaveReport.CompanyDivisions)
                    { 
                        foreach (StaffLeaveReportModel staffLeaveReportModel in divisionLeaveReportModel.DivisionStaff)
                        {
                            var leaveSchedule = new LeaveSchedule();

                            var stringLeaveDates = "";
                            foreach (var leaveDateTime in staffLeaveReportModel.LeaveDateTimes)
                            {
                                stringLeaveDates += leaveDateTime.StartDate.ToShortDateString() + " to " + leaveDateTime.EndDate.ToShortDateString() + ",  ";
                            }

                            leaveSchedule.Employee = staffLeaveReportModel.Fullname;
                            leaveSchedule.BroughtForward = staffLeaveReportModel.DaysAtEndOfMonth - staffLeaveReportModel.DaysTakenAfterEndDateInRange;
                            //leaveSchedule.BroughtForward = (staffLeaveReportModel.DynamicRunningTotal - UoWStaffLeave.GetIncrement(staffLeaveReportModel.StaffId));
                            leaveSchedule.DaysAccrued = staffLeaveReportModel.DaysAccrued.Round(2);
                            leaveSchedule.DatesOfLeave = stringLeaveDates;
                            leaveSchedule.NumberOfDaysTaken = staffLeaveReportModel.DaysTaken;
                            // leaveSchedule.CarriedForward = staffLeaveReportModel.RunningTotal;
                            leaveSchedule.CarriedForward = ((staffLeaveReportModel.DaysAtEndOfMonth + staffLeaveReportModel.DaysAccrued) - staffLeaveReportModel.DaysTaken).Round(2);
                           // leaveSchedule.CarriedForward = ((staffLeaveReportModel.DynamicRunningTotal + staffLeaveReportModel.DaysAccrued) - staffLeaveReportModel.DaysTaken).Round(2);



                            leaveSchedules.Add(leaveSchedule);
                        }
                    }
                }
         */


        public static bool EmialManager(string managerEmail, IList<StaffModel> staffClocksToGetWeekData)
        {
            const MessagesEnum messageType = MessagesEnum.WeeklyLeaveClockEmail;

            var message = new DataContextEF().Messages
                                             .FirstOrDefault(m => m.MessageType.Equals((int)messageType));
            //if (message == null)
            //    throw new Exception("Weekly message type not found");

            var mailer = new Emailer
            {
                subject = "Clocking summary for week \t "/* + DateTime.Now.Subtract(TimeSpan.FromDays(5)).Date.ToShortDateString() + "\t to \t " + DateTime.Today.Date.ToShortDateString()*/ //EnumHelper.GetEnumDescriptions(messageType)
            };
            #region

#if DEBUG
            mailer.TOList.Add("mtshona@nvestholdings.co.za");
#else
            mailer.TOList.Add(managerEmail);
#endif
            #endregion

            mailer.body = BuildClockDataForTheWeek(staffClocksToGetWeekData);
            if (!string.IsNullOrEmpty(mailer.body))
                mailer.SendEmail();

            return true;
        }

        public static WeekClockInSummary GetStaffClockInSummaries(StaffModel theStaffModel)
        {
            try
            { 
                var wcis = new WeekClockInSummary();

                double timeWorked       = 0;
                double overTime         = 0;
                double timeDebt         = 0;
                double requiredHours    = 0;
                double hoursOnLeave     = 0;
 
                IEnumerable<StaffClockingContainer> clockData = TimeKeepingContainer.GetStaffClockingData(theStaffModel);
 
                foreach (StaffClockingContainer scc in clockData)
                {
                    timeWorked += (CalculateTimeWorkedInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList()) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    overTime += ((CalculateOverTimeInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList())) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    timeDebt += ((CalculateTimeDebtInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList())) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    requiredHours += ((CalculateWeekRequiredHours(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList()) / 60).Round(2));
                }

                if(theStaffModel.StaffLeaveData.Any())
                {
                    var leavehoursmodel = StaffClockDataUtils.ConvertLeaveDataToClockData(theStaffModel);
                    hoursOnLeave = (CalculateTimeWorkedInMinutes(theStaffModel, leavehoursmodel.Select(m => m.ClockDateTime).ToList()) / 60).Round(2);
                }
                 
                wcis.Name           = theStaffModel.StaffFullName;
                wcis.RequiredHours  = requiredHours.Round(2);
                wcis.TimeWorked     = timeWorked.Round(2);
                wcis.Overtime       = (timeWorked - requiredHours).Round(2) < 0 ? 0 : (timeWorked - requiredHours).Round(2);
                wcis.TimeDebt       = (requiredHours - (timeWorked + hoursOnLeave)).Round(2) < 0 ? 0 : (requiredHours - (timeWorked + hoursOnLeave)).Round(2);
                wcis.OnleaveHours   = hoursOnLeave.Round(2);
                 
                return wcis;
            }
            catch (Exception)
            {
                return default(WeekClockInSummary);  
            }
           
        }


        private static string BuildClockDataForTheWeek(IList<StaffModel> staff)
        {
            #region
            var message = new StringBuilder();

            message.AppendFormat("<html><head><style>");

            message.AppendFormat("table {{	border-collapse: collapse;}}table, td, th {{	border: 1px; padding : 20px;}} th {{background-color: black; color: white; padding : 20px;}}");

            message.AppendFormat("</style>");
            message.AppendFormat("</head>" +
                                 "<p>Dear Manager</p><p> Please find here-below a summary of the weeks's timekeeping records for employees relating to your division</p>" +
                                 "<div><p>Employee working hours for the week " + DateTime.Now.Subtract(TimeSpan.FromDays(5)).Date.ToShortDateString() + "  to  " + DateTime.Today.Date.ToShortDateString()+
                                 "</p></div> <br/>");
            message.AppendFormat(
                "<table><thead><th>Employee Name</th><th>Required Hours</th><th>Hours Worked</th><th>Overtime Hours</th><th>On leave Hours</th><th>Time Debt Hours</th></thead/>");

            if (!staff.Any())
                return string.Empty;
            #endregion

            IList<WeekClockInSummary> weekClockInSummaries = new List<WeekClockInSummary>();

            //if (!theStaffModel.StaffClockData.Any())
            //    return "";
            
            foreach (StaffModel theStaffModel in staff)
            {
               

                WeekClockInSummary wcis = new WeekClockInSummary();

                double timeWorked       = 0;
                double overTime         = 0;
                double timeDebt         = 0;
                double requiredHours    = 0;
                double hoursOnLeave     = 0;
 
#if !DEBUG

                                DateTime mondayDate = DateTime.Now.Subtract(TimeSpan.FromDays(5));
                                DateTime fridayDate = DateTime.Today;

#else
                DateTime mondayDate = new DateTime(2015, 02, 09);
                DateTime fridayDate = new DateTime(2015, 02, 13);
#endif
                 
                IEnumerable<StaffClockingContainer> clockData = TimeKeepingContainer.GetStaffClockingData(theStaffModel);
 
                clockData = clockData.Where(m => m.ClockDate.Date >= mondayDate.Date && m.ClockDate.Date <= fridayDate.Date).ToList();

                foreach (StaffClockingContainer scc in clockData)
                {
                    timeWorked += (CalculateTimeWorkedInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList()) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    overTime += ((CalculateOverTimeInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList())) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    timeDebt += ((CalculateTimeDebtInMinutes(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList())) / 60).Round(2);
                }

                foreach (StaffClockingContainer scc in clockData)
                {
                    requiredHours += ((CalculateWeekRequiredHours(theStaffModel, scc.TimeKeepingItems.Select(m => m.Date).ToList()) / 60).Round(2));
                }

                if(theStaffModel.StaffLeaveData.Any())
                {
                    var leavehoursmodel = StaffClockDataUtils.ConvertLeaveDataToClockData(theStaffModel);

                    leavehoursmodel = leavehoursmodel.Where(m => m.ClockDateTime.Date >= mondayDate.Date && m.ClockDateTime.Date <= fridayDate.Date).ToList();

                   hoursOnLeave = (CalculateTimeWorkedInMinutes(theStaffModel, leavehoursmodel.Select(m => m.ClockDateTime).ToList()) / 60).Round(2);

                }
                 
                wcis.Name = theStaffModel.StaffFullName;
                wcis.RequiredHours = requiredHours;
                wcis.TimeWorked = timeWorked;
                wcis.Overtime = (timeWorked - requiredHours) < 0 ? 0 : (timeWorked - requiredHours);        
                wcis.TimeDebt = (requiredHours - (timeWorked + hoursOnLeave)) < 0 ? 0 : (requiredHours - (timeWorked + hoursOnLeave)).Round(2);     
                wcis.OnleaveHours = hoursOnLeave;

                weekClockInSummaries.Add(wcis);
            }

            weekClockInSummaries.ForEach(x => message.AppendFormat("<tr><td>{0}</td><td id=\"requiredtime\"> {1}</td><td id=\"timeworked\"> {2}</td><td id=\"overtime\">{3}</td><td id=\"timedebt\">{4}</td><td id=\"timedebt\">{5}</td></tr>",
                x.Name, x.RequiredHours, x.TimeWorked, x.Overtime, x.OnleaveHours, x.TimeDebt));

            foreach (WeekClockInSummary weekClock in weekClockInSummaries)
            {
                message.AppendFormat(
                    "<tr><td>{0}</td><td id=\"requiredtime\"> {1}</td><td id=\"timeworked\"> {2}</td><td id=\"overtime\">{3}</td><td id=\"timedebt\">{4}</td><td id=\"timedebt\">{5}</td></tr>",
                    weekClock.Name, weekClock.RequiredHours, weekClock.TimeWorked, weekClock.Overtime,
                    weekClock.OnleaveHours, weekClock.TimeDebt);
            }


            message.Append("</table>");
            message.Append("");
            message.Append("<p>If a staff member has time debt it's either they have incomplete clocking data for one of the days in the week, which they got a reminder about.or that they were absent.</p>" +
                           "</p>The value of leave hours should not be zero if there is time debt.");

            message.Append("<script>if(document.getElementById(\"timeworked\").innerHTML > document.getElementById(\"requiredtime\").innerHTML){document.getElementById(\"timeworked\").style.textDecoration = \"overline\";} </script>");
            message.Append("<p>Kind Regards</p><p>NVest Clocking System</p></html>");

            return message.AppendLine(string.Empty).ToString();
        }

       
    }
     
    public class TimeKeepingItem
    {
        public int ClockDataId { get; set; }
        public Guid StaffId { get; set; }
        public int StaffClockId { get; set; }
        public string StaffName { get; set; }
        public string StaffSurname { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
        public string Day
        {
            get { return Date.DayOfWeek.ToString(); }
        }
        public DateTime? TimeTaken { get; set; }
        public DateTime? TimeWorkedBack { get; set; }
        public bool Alert { get { return TimeTaken.HasValue; } }
        public string RowColor { get; set; }
        public int DataStatus { get; set; }
        //css styling
        public string DataStatusClass
        {
            get
            {
                switch ((ClockRecordEnums)DataStatus)
                {
                    case ClockRecordEnums.Approved:
                        return "approved";
                    case ClockRecordEnums.Pending:
                        return "pending";
                    case ClockRecordEnums.Denied:
                        return "denied";
                    case ClockRecordEnums.OnLeave:
                        return "onleave";
                    case ClockRecordEnums.PublicHoliday:
                        return "publicholiday";
                    case ClockRecordEnums.Offiste:
                        return "offsite";
                    case ClockRecordEnums.Special:
                        return "special";
                    default:
                        return "";
                }
            }
        }
        public int DataStatusHighlight { get; set; }
        //css styling
        public string DataStatusClassHighlightClass
        {
            get
            {
                switch ((ClockRecordHighlightEnums)DataStatusHighlight)
                {
                    case ClockRecordHighlightEnums.OnLeave:
                        return "onleaveH";
                    case ClockRecordHighlightEnums.PublicHoliday:
                        return "publicholidayH";
                    case ClockRecordHighlightEnums.Offiste:
                        return "offsiteH";
                    case ClockRecordHighlightEnums.Special:
                        return "specialH";
                    case ClockRecordHighlightEnums.Approved:
                        return "approvedH";
                    case ClockRecordHighlightEnums.Pending:
                        return "pendingH";
                    case ClockRecordHighlightEnums.Denied:
                        return "deniedH";
                    default:
                        return "";
                }
            }
        }

        //These will make more sense after I refactor this class and the one above
        public string DisplayTimeWorked { get; set; }
        public string DispalyOvertimeWorked { get; set; }
        public string DisplayTimeDebt { get; set; }

        //todo - jay: refactor
        public double TimeDebtInMinutes { get; set; }
        public double OverTimeInMinutes { get; set; }

        public string Comments { get; set; }

        #region Formated vlaues
        public string DisplayDate { get { return string.Format("{0:dd/MM/yy}", Date); } }
        public string DisplayTimeTaken { get { return TimeTaken.HasValue ? string.Format("{0: HH:mm }", TimeTaken.Value) : " "; } }
        public string DisplayTimeWorkerdBack { get { return TimeWorkedBack.HasValue ? string.Format("{0:HH:mm}", TimeWorkedBack.Value) : "*"; } }
        #endregion
        public TimeKeepingItem()
        {

        }

        public TimeKeepingItem(int clockDataId, Guid staffId, int clockId, string name, string surname, DateTime date, int dataStatus)
        {
            ClockDataId = clockDataId;
            StaffId = staffId;
            StaffClockId = clockId;
            StaffName = name;
            StaffSurname = surname;
            Date = date;
            Time = date.ToString("HH:mm");
            DataStatus = dataStatus;
        }

        public TimeKeepingItem(int clockDataId, Guid staffId, int clockId, string name, string surname, DateTime date, int dataStatus, int dataStatusHighlight)
        {
            ClockDataId = clockDataId;
            StaffId = staffId;
            StaffClockId = clockId;
            StaffName = name;
            StaffSurname = surname;
            Date = date;
            Time = date.ToString("HH:mm");
            DataStatus = dataStatus;
            DataStatusHighlight = dataStatusHighlight;
        }

    }

    public class StaffClockingContainer
    {
        public int ClockDataId { get; set; }

        public DateTime ClockDate { get; set; }
        public bool IsLeaveRecord { get;  set; }
        public string DisplayClockDate
        {
            get { return ClockDate.ToString("dddd MMMM dd, yyyy"); }
        }

        private LeaveType? Leave { get; set; }

        //public string LeaveType
        //{
        //    get
        //    {
        //        //todo -jay: use enum descriptons
        //        if (Leave != null)
        //            switch (Leave)
        //            {
        //                case Data.EF.Enums.LeaveType.Annual:
        //                    return "annual";
        //                case Data.EF.Enums.LeaveType.Sick:
        //                    return "sick";
        //                case Data.EF.Enums.LeaveType.Study:
        //                    return "study";
        //                case Data.EF.Enums.LeaveType.Family:
        //                    return "family";
        //            }

        //        return " ";
        //    }
        //}
        public int LeaveType { get; set; }

        public List<TimeKeepingItem> TimeKeepingItems { get; set; }
        public bool? IsPublicHoliday { get; set; }

        #region Constructor

        public StaffClockingContainer()
        {
            TimeKeepingItems = new List<TimeKeepingItem>();
        }

        public StaffClockingContainer(DateTime clockDate)
            : this()
        {
            ClockDate = clockDate;
        }

        public StaffClockingContainer(DateTime clockDate, bool isLeaveRecord)
            : this(clockDate)
        {
            IsLeaveRecord = isLeaveRecord;
        }

        public StaffClockingContainer(DateTime clockDate, int? leave)
            : this(clockDate, leave.HasValue)
        {
            if (leave != null) Leave = (LeaveType)leave;
        }

        #endregion

    }

    public class LeaveSchedule
    {
        public string Employee { get; set; }
        public double BroughtForward { get; set; }
        public double DaysAccrued { get; set; }
        public string DatesOfLeave { get; set; }
        public double NumberOfDaysTaken { get; set; }
        public double CarriedForward { get; set; }
        //public double RunningTotal { get; set; }
     
    }

    public class LeaveDates
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}