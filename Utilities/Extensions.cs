using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Extensions
    {
        #region Type double extensions

        public static double Round(this double value, int decimalPlace)
        {
            return Math.Round(value, decimalPlace);
        }

        #endregion

        #region Type DateTime extensions

        public static bool IsWeekend(this DateTime value)
        {
            return value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsPublicHoliday(this DateTime value)
        {
            var publicHols = new List<DateTime>
                {
                    new DateTime(value.Year, 1, 1), //New Years
                    new DateTime(value.Year, 3, 21), //Human rights day
                    new DateTime(value.Year, 4, 18), //Good friday
                    new DateTime(value.Year, 4, 21), //Family day
                    new DateTime(value.Year, 4, 27), //Freedom day
                    new DateTime(value.Year, 4, 28), //Public holiday
                    new DateTime(value.Year, 5, 1), //Workers day
                    new DateTime(value.Year, 6, 16), //Youth day
                    new DateTime(value.Year, 9, 24), //Heritage day
                    new DateTime(value.Year, 12, 16), //day of reconciliation
                    new DateTime(value.Year, 3, 25), //Christmas day
                    new DateTime(value.Year, 3, 26), //Day of Goodwill
                };

            return publicHols.Any(m => m.Date == value.Date);
        }

        public static bool IsLeaveRangePublicHoliday(this DateTime value)
        {
            var publicHols = new List<DateTime>
                {
                    new DateTime(value.Year, 1, 1), //New Years
                    new DateTime(value.Year, 3, 21), //Human rights day
                    new DateTime(value.Year, 4, 18), //Good friday
                    new DateTime(value.Year, 4, 21), //Family day
                    new DateTime(value.Year, 4, 27), //Freedom day
                    new DateTime(value.Year, 4, 28), //Public holiday
                    new DateTime(value.Year, 5, 1), //Workers day
                    new DateTime(value.Year, 6, 16), //Youth day
                    new DateTime(value.Year, 9, 24), //Heritage day
                    new DateTime(value.Year, 12, 16), //day of reconciliation
                    new DateTime(value.Year, 3, 25), //Christmas day
                    new DateTime(value.Year, 3, 26), //Day of Goodwill
                };

            return publicHols.Any(m => m.Date == value.Date);
        }

        public static List<DateTime> ThePublicHolidays(this DateTime value, DateTime max)
        {
            var publicHols = new List<DateTime>
                {
                    new DateTime(max.Year, 1, 1), //New Years
                    new DateTime(max.Year, 3, 21), //Human rights day
                    new DateTime(max.Year, 4, 18), //Good friday
                    new DateTime(max.Year, 4, 21), //Family day
                    new DateTime(max.Year, 4, 27), //Freedom day
                    new DateTime(max.Year, 4, 28), //Public holiday
                    new DateTime(max.Year, 5, 1), //Workers day
                    new DateTime(max.Year, 6, 16), //Youth day
                    new DateTime(max.Year, 9, 24), //Heritage day
                    new DateTime(max.Year, 12, 16), //day of reconciliation
                    new DateTime(max.Year, 12, 25), //Christmas day
                    new DateTime(max.Year, 12, 26), //Day of Goodwill
                };
            List<DateTime> holidayData = publicHols.Where(x => x.Date <= max.Date).ToList();
            //List<DateTime> holidayData = publicHols.Where(x => x.Date >= max.Date && x.Date <= value.Date).ToList();
            return holidayData;
        }

        public static List<DateTime> ThePublicHolidays(this DateTime value, DateTime max, DateTime min)
        {
            var publicHols = new List<DateTime>
                {
                    new DateTime(min.Year,1, 1), //New Years
                    new DateTime(min.Year,3, 21), //Human rights day
                    new DateTime(min.Year,3, 25),
                    new DateTime(min.Year,3, 28), //Public holiday
                    new DateTime(min.Year,4, 27), //Good friday
                    new DateTime(min.Year,5, 1), //Workers day
                    new DateTime(min.Year,5, 2), //Workers day
                    new DateTime(min.Year,6, 16), //Youth day
                    new DateTime(min.Year,8, 8), //Heritage day
                    new DateTime(min.Year,9, 24), //Heritage day
                    new DateTime(min.Year,12, 16), //day of reconciliation
                    new DateTime(min.Year,12, 25), //Day of Goodwill//Christmas day
                    new DateTime(min.Year, 12, 26), //Day of Goodwill  
                }; 


            var publicHolsMax = new List<DateTime>
                {
                    new DateTime(max.Year,1, 1), //New Years
                    new DateTime(max.Year,3, 21), //Human rights day
                    new DateTime(max.Year,3, 25),
                    new DateTime(max.Year,3, 28), //Public holiday
                    new DateTime(max.Year,4, 27), //Good friday
                    new DateTime(max.Year,5, 1), //Workers day
                    new DateTime(max.Year,5, 2), //Workers day
                    new DateTime(max.Year,6, 16), //Youth day
                    new DateTime(max.Year,8, 8), //Heritage day
                    new DateTime(max.Year,9, 24), //Heritage day
                    new DateTime(max.Year,12, 16), //day of reconciliation
                    new DateTime(max.Year,12, 25), //Day of Goodwill//Christmas day
                    new DateTime(max.Year,12, 26), //Day of Goodwill    
        }; 

            publicHols.AddRange(publicHolsMax);
          
            List<DateTime> holidayData = publicHols.Where(x => x.Date >= min.Date  &&
                                                               x.Date <= max.Date).ToList();
            // //List<DateTime> holidayData = publicHols.Where(x => x.Date >= max.Date && x.Date <= value.Date).ToList();
            return holidayData;
        }

        /// <summary>
        /// Calculate the difference in days between two dates
        /// NB:If the dates are the same, then that will count as a days
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="includeWeekends"></param>
        /// <param name="includehols"></param>
        /// <returns></returns>
        public static double DifferenceInDays(this DateTime start, DateTime end, bool includeWeekends = false,
                                              bool includehols = false)
        {

            if (start.Date > end.Date)
                throw new Exception("Start date cannot be greater than end date");

            var days = 0;

            var holidays = 0;
            var weekends = 0;

            while (start.Date != end.Date)
            {
                if (start.IsWeekend())
                    weekends++;

                if (start.IsPublicHoliday())
                    holidays++;

                days++;
                start = start.AddDays(1);
            }

            if (!includehols)
                days = days - holidays;
            if (!includeWeekends)
                days = days - weekends;

            return days;
        }

        /// <summary>
        /// Calculate the difference in days between two dates
        /// NB:If the dates are the same, then that will count as a days
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="includeWeekends"></param>
        /// <param name="includehols"></param>
        /// <returns></returns>
        public static double DifferenceInDaysPayroll(this DateTime start, DateTime end, bool includeWeekends = false,
                                              bool includehols = false)
        {

            if (start.Date > end.Date)
                throw new Exception("Start date cannot be greater than end date");

            var days = 1;

            var holidays = 0;
            var weekends = 0;

            while (start.Date != end.Date)
            {
                if (start.IsWeekend())
                    weekends++;

                if (start.IsPublicHoliday())
                    holidays++;

                days++;
                start = start.AddDays(1);
            }

            if (!includehols)
                days = days - holidays;
            if (!includeWeekends)
                days = days - weekends;

            return days;
        }



        /*
          public static double DifferenceInDays(this DateTime start, DateTime end, bool includeWeekends = false,
                                              bool includehols = false)
        {

            if (start.Date > end.Date)
                throw new Exception("Start date cannot be greater than end date");

            var days = 1;

            var holidays = 0;
            var weekends = 0;

            while (start.Date != end.Date)
            {
                if (start.IsWeekend())
                    weekends++;

                if (start.IsPublicHoliday())
                    holidays++;

                days++;
                start = start.AddDays(1);
            }

            if (!includehols)
                days = days - holidays;
            if (!includeWeekends)
                days = days - weekends;

            return days;
        }

         */

        #endregion

        #region String Extesnions
        public static string AppendNewLine(this string value, string appendValue)
        {
            return value + System.Environment.NewLine + appendValue;
        }
        #endregion

    }
}
