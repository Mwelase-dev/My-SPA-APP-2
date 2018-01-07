using System;
using System.Collections.Generic;
using System.Linq;
using Intranet.Data.EF;
using Intranet.Models;
using Intranet.Models.Enums;
using Utilities;

namespace Intranet.UI.Utilities
{
    //TODO: Ref Jay: Not sure about the context of this class.
    public static class StaffClockDataUtils
    {
        private const int LunchStartHour = 12;
        private const int LunchEndHour = 13;

        public static ICollection<StaffClockModel> MergeClockData(Guid staffId)
        {
            using (var store = new DataContextEF())
            {
                ICollection<StaffHoursModel> hoursModel = store.StaffHourData.Where(m => m.StaffId.Equals(staffId)).ToList();
                IQueryable<StaffClockModel> clockModel = store.StaffClockData.Where(m => m.StaffId.Equals(staffId));
                IQueryable<StaffLeaveModel> leaveModel = store.StaffLeaveData.Where(m => m.StaffId.Equals(staffId));

                if (!clockModel.Any() || !hoursModel.Any() || !leaveModel.Any())
                {
                    throw new Exception(string.Format("Staff member with id {0} not found.", staffId));
                }

                return MergeClockDataForGraphs(clockModel, hoursModel, leaveModel);

            }
        }

        /// <summary>
        /// Merges leave days and clocking Records for Graphs
        /// </summary>
        /// <param name="clockModel"></param>
        /// <param name="hoursModel"></param>
        /// <param name="leaveModel"></param>
        /// <returns></returns>
        public static ICollection<StaffClockModel> MergeClockDataForGraphs(IQueryable<StaffClockModel> clockModel, ICollection<StaffHoursModel> hoursModel, IQueryable<StaffLeaveModel> leaveModel)
        {
            var clockData = clockModel.ToList();
            var leaveData = leaveModel;

            if (leaveData == null)
                return clockData;

            if (clockData.Count < 1)
                return clockData;

            foreach (var data in leaveData)
            {
                // check how long the leave is
                var numOfLeaveDays = GetDaysOff(data.LeaveDateStart, data.LeaveDateEnd);

                for (var i = 0; i < numOfLeaveDays; i++)
                {
                    var date = data.LeaveDateStart.AddDays(i);

                    if (IsWeekend(date)) continue;

                    #region Get the staff's start and end times


                    var staffHoursModel = hoursModel.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));

                    if (staffHoursModel == null)
                        throw new ArgumentNullException("staffHoursModel");

                    var dayStart = new StaffClockModel(data.StaffId,
                                                       new DateTime(date.Year, date.Month, date.Day,
                                                                    staffHoursModel.DayTimeStart.Hour, 0, 0), data.LeaveType);
                    var dayEnd = new StaffClockModel(data.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  staffHoursModel.DayTimeEnd.Hour, 0, 0), data.LeaveType);
                    var lunchStart = new StaffClockModel(data.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchStartHour, 0, 0), data.LeaveType);
                    var lunchEnd = new StaffClockModel(data.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchEndHour, 0, 0), data.LeaveType);

                    clockData.Add(dayStart);
                    clockData.Add(lunchStart);
                    clockData.Add(lunchEnd);
                    clockData.Add(dayEnd);

                    #endregion
                }
            }
            return clockData;
        }


        /*
        /// <summary>
        /// Merges leave days and clocking Records
        /// </summary>
        /// <param name="model"></param>
        public static ICollection<StaffClockModel> MergeClockData(StaffModel model)
        {
            var clockData = model.StaffClockData;
            var leaveData = model.StaffLeaveData;

            #region Insert clock records for Public holidays and if public holiday is on a Sunday
            var publicHolidays = DateTime.Now.ThePublicHolidays();

            var minDate = new DateTime();
            var maxDate = new DateTime();

            if (clockData.Any())
            {
                minDate = (from d in clockData select d.ClockDateTime).Min();
                maxDate = (from d in clockData select d.ClockDateTime).Max();
            }
            else if (leaveData.Any())
            {
                minDate = (from d in leaveData select d.LeaveDateStart).Min();
                maxDate = (from d in leaveData select d.LeaveDateEnd).Max();
            }

            publicHolidays = publicHolidays.Where(m => m.Date >= minDate && m.Date <= maxDate).ToList();

            foreach (DateTime publicHoliday in publicHolidays)
            {
                var staffHoursModel = new StaffHoursModel();
                var dayStartPublicHolidayOnSunday = new StaffClockModel();
                var dayEndPublicHolidayOnSunday = new StaffClockModel();
                var dayStart = new StaffClockModel();
                var dayEnd = new StaffClockModel();

                var date = publicHoliday.Date;

                if (date.DayOfWeek == DayOfWeek.Saturday) continue;

                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals(1));
                }
                else
                {
                    staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));
                }
                if (staffHoursModel == null)
                    throw new ArgumentNullException("staffHoursModel");


                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayStartPublicHolidayOnSunday = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0));

                    dayEndPublicHolidayOnSunday = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0));
                }
                else
                {
                    dayStart = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0));

                    dayEnd = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute, 0));
                }

                dayStart.IsPublicHoldiday = true;
                dayEnd.IsPublicHoldiday = true;

                dayStart.IsLeaveRecord = false;
                dayEnd.IsLeaveRecord = false;

                dayEnd.DataStatus = (int)ClockRecordEnums.PublicHoliday;

                var valueDayStart = clockData.FirstOrDefault(x => x.ClockDateTime.Date == dayStart.ClockDateTime.Date);
                var valueDayEnd = clockData.FirstOrDefault(x => x.ClockDateTime.Date == dayStart.ClockDateTime.Date);

                if (valueDayEnd == null)
                    clockData.Add(dayEnd);

                if (valueDayStart == null)
                    clockData.Add(dayStart);

                if (valueDayEnd == null && dayStart.ClockDateTime != default(DateTime))
                {
                    dayStartPublicHolidayOnSunday.IsLeaveRecord = false;
                    dayStartPublicHolidayOnSunday.IsPublicHoldiday = true;
                    clockData.Add(dayStartPublicHolidayOnSunday);
                }

                if (valueDayEnd == null && dayStart.ClockDateTime != default(DateTime))
                {
                    dayEndPublicHolidayOnSunday.IsLeaveRecord = false;
                    dayEndPublicHolidayOnSunday.IsPublicHoldiday = true;
                    clockData.Add(dayEndPublicHolidayOnSunday);
                }
            }

            #endregion

            #region Insert clock record for days on leave
            foreach (var data in leaveData)
            {
                if (data.LeaveStatus == (int)LeaveStatus.Approved)
                {
                    var numOfLeaveDays = GetDaysOff(data.LeaveDateStart, data.LeaveDateEnd);
                    for (var i = 0; i < numOfLeaveDays; i++)
                    {
                        var date = data.LeaveDateStart.AddDays(i);
                        if (IsWeekend(date)) continue;

                        var dayStart = new StaffClockModel(model.StaffId,
                            new DateTime(date.Year, date.Month, date.Day,
                                data.LeaveDateStart.Hour, data.LeaveDateStart.Minute, 0), data.LeaveType);

                        var dayEnd = new StaffClockModel(model.StaffId,
                            new DateTime(date.Year, date.Month, date.Day,
                                (data.LeaveDateEnd.Hour), data.LeaveDateEnd.Minute,
                                0), data.LeaveType);

                        dayStart.IsLeaveRecord = true;
                        dayEnd.IsLeaveRecord = true;

                        dayStart.IsPublicHoldiday = false;
                        dayEnd.IsPublicHoldiday = false;

                        dayStart.LeaveType = data.LeaveType;
                        dayEnd.LeaveType = data.LeaveType;

                        dayEnd.DataStatus = (int)ClockRecordEnums.OnLeave;
                        dayStart.DataStatus = (int)ClockRecordEnums.OnLeave;

                        clockData.Add(dayEnd);
                        clockData.Add(dayStart);
                    }
                }
            }
            #endregion

            if (clockData.Count < 1)
                return clockData;

            clockData = clockData.ToList().OrderBy(m => m.ClockDateTime).ToList();
            return clockData;
        }
        */


        /// <summary>
        /// Merges leave days and clocking Records
        /// </summary>
        /// <param name="model"></param>
        public static ICollection<StaffClockModel> MergeClockData(StaffModel model)
        {
            var clockData = model.StaffClockData;
            var leaveData = model.StaffLeaveData;
            if (!clockData.Any() && !leaveData.Any())
                return default(ICollection<StaffClockModel>);
            var publicHolidays = DateTime.Now.ThePublicHolidays((from d in clockData select d.ClockDateTime).Max(), (from d in clockData select d.ClockDateTime).Min());

            var minDate = new DateTime();
            var maxDate = new DateTime();

            #region

            if (clockData.Any())
            {
                minDate = (from d in clockData select d.ClockDateTime).Min();
                maxDate = (from d in clockData select d.ClockDateTime).Max();
            }
            else if (leaveData.Any())
            {
                minDate = (from d in leaveData select d.LeaveDateStart).Min();
                maxDate = (from d in leaveData select d.LeaveDateEnd).Max();
            }


            //publicHolidays = publicHolidays.Where(m => m.Date >= minDate.Date && m.Month >= minDate.Month && m.Date <= maxDate.Date && m.Month <= maxDate.Month).ToList();

            

            foreach (DateTime publicHoliday in publicHolidays)
            {
                if (clockData.Any(x=>x.ClockDateTime.Date == publicHoliday.Date))
                {
                    continue;
                }
                var staffHoursModel = new StaffHoursModel();
                var dayStartPublicHolidayOnSunday = new StaffClockModel();
                var dayEndPublicHolidayOnSunday = new StaffClockModel();
                var dayStart = new StaffClockModel();
                var dayEnd = new StaffClockModel();
               
                var date = publicHoliday.Date;
                 
                //if (IsWeekend(date)) continue;

                if (date.DayOfWeek == DayOfWeek.Saturday) continue;

                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals(1));
                }
                else
                {
                    staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));
                }


                if (staffHoursModel == null)
                    throw new ArgumentNullException("staffHoursModel");


                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    dayStartPublicHolidayOnSunday = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeStart.Hour + 2, 0, 0));

                    dayEndPublicHolidayOnSunday = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeEnd.Hour + 1, publicHoliday.DayOfWeek == DayOfWeek.Friday ? 30 : 0, 0));
                }
                else
                {
                    dayStart = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute == 0 ? 0 : staffHoursModel.DayTimeStart.Minute, 0));

                    dayEnd = new StaffClockModel(model.StaffId,
                        new DateTime(publicHoliday.Year, publicHoliday.Month, publicHoliday.Day,
                            staffHoursModel.DayTimeEnd.Hour + 1, staffHoursModel.DayTimeEnd.Minute == 0 ? 0 : staffHoursModel.DayTimeEnd.Minute, 0));

                }



                dayStart.IsPublicHoldiday = true;
                dayEnd.IsPublicHoldiday = true;

                dayStart.IsLeaveRecord = false;
                dayEnd.IsLeaveRecord = false;

                dayEnd.DataStatus = (int)ClockRecordEnums.PublicHoliday;
                dayStart.DataStatus = (int)ClockRecordEnums.PublicHoliday;

                dayEnd.DataStatusHighlight = (int)ClockRecordHighlightEnums.PublicHoliday;
                dayStart.DataStatusHighlight = (int)ClockRecordHighlightEnums.PublicHoliday;

                var valueDayStart = clockData.FirstOrDefault(x => x.ClockDateTime.Date == dayStart.ClockDateTime.Date);
                var valueDayEnd = clockData.FirstOrDefault(x => x.ClockDateTime.Date == dayStart.ClockDateTime.Date);

                if (valueDayEnd == null)
                    clockData.Add(dayEnd);

                if (valueDayStart == null)
                    clockData.Add(dayStart);

                //if (valueDayEnd == null && dayStart.ClockDateTime != default(DateTime))
                //{
                //    dayStartPublicHolidayOnSunday.IsLeaveRecord = false;
                //    dayStartPublicHolidayOnSunday.IsPublicHoldiday = true;
                //    clockData.Add(dayStartPublicHolidayOnSunday);
                //}


                //if (valueDayEnd == null && dayStart.ClockDateTime != default(DateTime))
                //{
                //    dayEndPublicHolidayOnSunday.IsLeaveRecord = false;
                //    dayEndPublicHolidayOnSunday.IsPublicHoldiday = true;
                //    clockData.Add(dayEndPublicHolidayOnSunday);
                //}



            }
            // }

            #endregion

            #region

            if (!leaveData.Any())
            {
                return clockData;
            }
            else
            {
                foreach (var data in leaveData)
                {
                    if (data.LeaveStatus == (int)LeaveStatus.Approved)
                    {
                        // check how long the leave is
                        var numOfLeaveDays = GetDaysOff(data.LeaveDateStart, data.LeaveDateEnd);
                        for (var i = 0; i < numOfLeaveDays; i++)
                        {
                            var date = data.LeaveDateStart.AddDays(i);
                            if (IsWeekend(date)) continue;

                            #region Get the staff's start and end times of the leave and add them as clocking records

                            var staffHoursModel =
                                model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));

                            if (staffHoursModel == null)
                                throw new ArgumentNullException("staffHoursModel");
                            var dayStart = new StaffClockModel(model.StaffId,
                                new DateTime(date.Year, date.Month, date.Day,
                                    data.LeaveDateStart.Hour, data.LeaveDateStart.Minute, 0), data.LeaveType);

                            //var dayEnd = new StaffClockModel(model.StaffId,
                            //    new DateTime(date.Year, date.Month, date.Day,
                            //      data.LeaveDateEnd.Date > data.LeaveDateStart.Date ? staffHoursModel.DayTimeEnd.Hour + 1 : (data.LeaveDateEnd.Hour),
                            //      data.LeaveDateEnd.Minute > staffHoursModel.DayTimeEnd.Minute ? staffHoursModel.DayTimeEnd.Minute : data.LeaveDateEnd.Minute,
                            //        0), data.LeaveType);

                            var dayEnd = new StaffClockModel(model.StaffId,
                                new DateTime(date.Year, date.Month, date.Day,
                                  data.LeaveDateEnd.Date > data.LeaveDateStart.Date ? staffHoursModel.DayTimeEnd.Hour + 1 : (data.LeaveDateEnd.Hour),
                                 staffHoursModel.DayTimeEnd.Minute,
                                    0), data.LeaveType);

                            if (staffHoursModel.DayId.Equals((int) DayOfWeek.Friday))
                            {
                                
                            }

                            //if (data.LeaveType != (int)LeaveType.Special)
                            //{

                            //    if (data.LeaveDateEnd.Subtract(data.LeaveDateStart).TotalHours > 7)
                            //    {

                            //        //if (dayEnd.ClockDateTime.Hour == staffHoursModel.DayTimeEnd.AddHours(2).Hour)
                            //        //{
                            //        //    dayEnd.ClockDateTime = dayEnd.ClockDateTime.Subtract(new TimeSpan(0,0,staffHoursModel.DayLunchLength,0));
                            //        //}
                            //        if (staffHoursModel.DayTimeEnd.Hour + 2 == 16 && staffHoursModel.DayTimeEnd.Minute == 30)
                            //        {
                            //            dayEnd.ClockDateTime = dayEnd.ClockDateTime.Subtract(new TimeSpan(0, 0, staffHoursModel.DayId == 5 && staffHoursModel.DayLunchLength > 0 ? (/*staffHoursModel.DayLunchLength +*/ 30) : staffHoursModel.DayLunchLength, 0, 0));
                              
                            //        }
                            //        else if (staffHoursModel.DayTimeEnd.Hour + 2 < 16 && staffHoursModel.DayTimeEnd.Minute == 00)
                            //        {
                                        
                            //        }
                            //        //dayEnd.ClockDateTime = dayEnd.ClockDateTime.Subtract(new TimeSpan(0, 0, staffHoursModel.DayId == 5 && staffHoursModel.DayLunchLength > 0 ? (/*staffHoursModel.DayLunchLength +*/ 30) : staffHoursModel.DayLunchLength, 0, 0));
                            //    }

                            //}
                            dayStart.IsLeaveRecord = true;
                            dayEnd.IsLeaveRecord = true;

                            dayStart.IsPublicHoldiday = false;
                            dayEnd.IsPublicHoldiday = false;

                            dayStart.LeaveType = data.LeaveType;
                            dayEnd.LeaveType = data.LeaveType;


                            if (data.LeaveType == (int)LeaveType.Special)
                            {
                                dayEnd.DataStatus = (int)ClockRecordEnums.Special;
                                dayStart.DataStatus = (int)ClockRecordEnums.Special;

                                dayEnd.DataStatusHighlight = (int)ClockRecordHighlightEnums.Special;
                                dayStart.DataStatusHighlight = (int)ClockRecordHighlightEnums.Special;
                            }
                            else if (data.LeaveType == (int) LeaveType.OffSite)
                            {
                                dayEnd.DataStatus = (int) ClockRecordEnums.Offiste;
                                dayStart.DataStatus = (int) ClockRecordEnums.Offiste;

                                dayEnd.DataStatusHighlight = (int)ClockRecordHighlightEnums.Offiste;
                                dayStart.DataStatusHighlight = (int)ClockRecordHighlightEnums.Offiste;
                            }
                            else
                            {
                                dayEnd.DataStatus = (int)ClockRecordEnums.OnLeave;
                                dayStart.DataStatus = (int)ClockRecordEnums.OnLeave;

                                dayEnd.DataStatusHighlight = (int)ClockRecordHighlightEnums.OnLeave;
                                dayStart.DataStatusHighlight = (int)ClockRecordHighlightEnums.OnLeave;
                            }


                            clockData.Add(dayEnd);
                            clockData.Add(dayStart);

                            #endregion

                        }
                    }
                }
            }

            #endregion

            if (clockData.Count < 1)
                return clockData;

            clockData = clockData.ToList().OrderBy(m => m.ClockDateTime).ToList();
            return clockData;
        }
        /**/

        /*
        /// <summary>
        /// Merges leave days and clocking Records
        /// </summary>
        /// <param name="model"></param>
        public static ICollection<StaffClockModel> MergeClockData(StaffModel model)
        {

            var clockData = model.StaffClockData;
            var leaveData = model.StaffLeaveData;

            if (leaveData == null)
                return clockData;

            //if (clockData.Count < 1)
            //    return clockData;

            foreach (var data in leaveData)
            {
                // check how long the leave is
                var numOfLeaveDays = GetDaysOff(data.LeaveDateStart, data.LeaveDateEnd);

                for (var i = 0; i < numOfLeaveDays; i++)
                {
                    var date = data.LeaveDateStart.AddDays(i);

                    if (IsWeekend(date)) continue;

                    #region Get the staff's start and end times

                    var staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));

                    if (staffHoursModel == null)
                        throw new ArgumentNullException("staffHoursModel");

                    var dayStart = new StaffClockModel(model.StaffId,
                                                       new DateTime(date.Year, date.Month, date.Day,
                                                                    staffHoursModel.DayTimeStart.Hour + 2, staffHoursModel.DayTimeStart.Minute, 0), data.LeaveType);
                    var dayEnd = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  staffHoursModel.DayTimeEnd.Hour + 2, staffHoursModel.DayTimeEnd.Minute, 0), data.LeaveType);
                    var lunchStart = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchStartHour, 0, 0), data.LeaveType);
                    var lunchEnd = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchEndHour, 0, 0), data.LeaveType);

                    dayStart.IsLeaveRecord   = true;
                    dayEnd.IsLeaveRecord     = true;
                    lunchStart.IsLeaveRecord = true;
                    lunchEnd.IsLeaveRecord   = true;

                    dayStart.IsPublicHoldiday   = false;
                    dayEnd.IsPublicHoldiday = false;
                    lunchStart.IsPublicHoldiday = false;
                    lunchEnd.IsPublicHoldiday = false;

                    dayStart.LeaveType   = data.LeaveType;
                    dayEnd.LeaveType     = data.LeaveType;
                    lunchStart.LeaveType = data.LeaveType;
                    lunchEnd.LeaveType   = data.LeaveType;

                    clockData.Add(dayStart);
                    clockData.Add(lunchStart);
                    clockData.Add(lunchEnd);
                    clockData.Add(dayEnd);

                    #endregion
                }

            }

            return clockData;
        }
        */
        public static ICollection<StaffClockModel> ConvertLeaveDataToClockData(StaffModel model)
        {
            IList<StaffClockModel> clockData = new List<StaffClockModel>();
            var leaveData = model.StaffLeaveData;

            if (leaveData == null)
                return clockData;

            foreach (var data in leaveData)
            {
                // check how long the leave is
                var numOfLeaveDays = GetDaysOff(data.LeaveDateStart, data.LeaveDateEnd);

                for (var i = 0; i < numOfLeaveDays; i++)
                {
                    var date = data.LeaveDateStart.AddDays(i);

                    if (IsWeekend(date)) continue;

                    #region Get the staff's start and end times

                    var staffHoursModel = model.StaffHoursData.FirstOrDefault(m => m.DayId.Equals((int)date.DayOfWeek));

                    if (staffHoursModel == null)
                        throw new ArgumentNullException("staffHoursModel");

                    var dayStart = new StaffClockModel(model.StaffId,
                                                       new DateTime(date.Year, date.Month, date.Day,
                                                                    staffHoursModel.DayTimeStart.Hour, 0, 0), data.LeaveType);
                    var dayEnd = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  staffHoursModel.DayTimeEnd.Hour, 0, 0), data.LeaveType);
                    var lunchStart = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchStartHour, 0, 0), data.LeaveType);
                    var lunchEnd = new StaffClockModel(model.StaffId,
                                                     new DateTime(date.Year, date.Month, date.Day,
                                                                  LunchEndHour, 0, 0), data.LeaveType);

                    clockData.Add(dayStart);
                    clockData.Add(lunchStart);
                    clockData.Add(lunchEnd);
                    clockData.Add(dayEnd);

                    #endregion
                }
            }
            return clockData;
        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static int GetDaysOff(DateTime start, DateTime end)
        {
            if (start.Date == end.Date)
                return 1;

            //add one day because the dates are not inclusive
            return (int)((end - start).Days + 1);
        }

    }
}