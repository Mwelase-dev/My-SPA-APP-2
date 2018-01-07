using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Optimization;
using Intranet.Business;
using Intranet.Models;
using Intranet.Models.Enums;
using Utilities;

namespace Intranet.UI.Models
{
    public class CompanyClockSummariesReport
    {
        public string CompanyName { get; set; }
        public Guid CompanyId { get; set; }
        public List<DivisionClockSummariesReportModel> CompanyDivisions { get; set; }

        public CompanyClockSummariesReport()
        {
            CompanyDivisions = new List<DivisionClockSummariesReportModel>();
        }
        public CompanyClockSummariesReport(Guid compId, string compayName, IEnumerable<DivisionModel> branchDivisions,double numberOfDaysInRange, string startDate, string endDate): this()
        {
            CompanyId = compId;
            CompanyName = compayName;
            CompanyDivisions = branchDivisions
                .ToList()
                .ConvertAll(
                    m => new DivisionClockSummariesReportModel(
                        m.DivisionId,
                        m.DivisionName,
                        m.BranchId,
                        m.DivisionStaff, numberOfDaysInRange,startDate,endDate));
        }
    }

    public class DivisionClockSummariesReportModel
    {
        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; }
        public Guid BranchId { get; set; }
        public List<StaffClockSummariesReportModel> DivisionStaff { get; set; }
        public StaffModel CurrStaffModel { get; set; }

        public DivisionClockSummariesReportModel()
        {
            DivisionStaff = new List<StaffClockSummariesReportModel>();
        }
        public DivisionClockSummariesReportModel(Guid divisionId, string divisionName, Guid branchId, IEnumerable<StaffModel> branchStaff, double numberOfDaysInRange,string startDate, string endDate)
            : this()
        {
            
            DivisionId = divisionId;
            DivisionName = divisionName;
            BranchId = branchId;
            DivisionStaff = branchStaff.ToList().ConvertAll(m => new StaffClockSummariesReportModel(m.StaffId, m.StaffFullName, m.DivisionId, m.StaffClockData, m, numberOfDaysInRange,startDate,endDate));
        }
    }

    public class StaffClockSummariesReportModel
    {
        public Guid StaffId { get; set; }
        public string Fullname { get; set; }
        public Guid DivisionId { get; set; }
        public int Year { get; set; }
        public LeaveType LeaveType { get; set; }
        public double Allocated { get; set; }
        public double Taken { get; set; }
        public double CarryOver { get; set; }
        public double RunningTotal { get; set; }
        public List<StaffClockModel> StaffClockRecords { get; set; }

        public String Name { get; set; }
        public Double TimeWorked { get; set; }
        public Double TimeDebt { get; set; }
        public Double Overtime { get; set; }
        public Double RequiredHours { get; set; }
        public Double OnleaveHours { get; set; }

        public IList<String> UnevenReordDates { get; set; }
        public int NumberOfDaysWithUnevenClockData { get; set; }


        public IList<String> AbsentDates { get; set; }
        public int NumberOfDaysWithNoClockDataAndNoLeave { get; set; }


         
        public StaffClockSummariesReportModel()
        {
            StaffClockRecords = new List<StaffClockModel>();
        }
        public StaffClockSummariesReportModel(Guid staffId, string fullName, Guid divisionId, ICollection<StaffClockModel> clockRecords,StaffModel staff, double numberOfDaysInRange, string startDate, string endDate): this()
        {
            StaffId = staffId;
            Fullname = fullName;
            DivisionId = divisionId;
            
                  IQueryable<StaffClockingContainer> clockData = null;

            if(staff.StaffClockData.Count > 0)
                clockData = TimeKeepingContainer.GetStaffClockingData(staff).AsQueryable();
            UnevenReordDates = new List<String>();


            var dateWithNoData = GetDateWithoutClockData(DateTime.Parse(startDate), DateTime.Parse(endDate),
                  clockData);

            var dateWithNoLeaveData = GetDateWithoutLeaveData(DateTime.Parse(startDate), DateTime.Parse(endDate),staff.StaffLeaveData);

            if (dateWithNoData.Any(x => DateTime.Parse(x) != default(DateTime)) &&dateWithNoData.Any() && dateWithNoLeaveData.Any())
            {
                NumberOfDaysWithNoClockDataAndNoLeave = dateWithNoData.Count;
                AbsentDates = dateWithNoData;
            }

            if (clockData != null)
            {
                foreach (var d in clockData)
                {
                    if (d.TimeKeepingItems.Count % 2 != 0)
                    {
                        NumberOfDaysWithUnevenClockData++;
                        UnevenReordDates.Add(d.DisplayClockDate);
                    }
                }
            }
           

            var clockInSummary = new WeekClockInSummary();
            clockInSummary = TimeKeepingContainer.GetStaffClockInSummaries(staff);
            if(clockInSummary == null)
                return;

            TimeWorked      = clockInSummary.TimeWorked;
            TimeDebt        = clockInSummary.TimeDebt;
            Overtime        = clockInSummary.Overtime;
            RequiredHours   = clockInSummary.RequiredHours;
            OnleaveHours    = clockInSummary.OnleaveHours;
        }

        public IList<string> GetDateWithoutClockData(DateTime start, DateTime end,IQueryable<StaffClockingContainer> clockData)
        {
            var theDate = new List<string>();
            var publicHolidays = DateTime.Now.ThePublicHolidays(end,start);

            if(clockData != null){
                while (start.Date < end.Date)
                {
                    var dateWithNoData = !clockData.Any(x => x.ClockDate.Date.Equals(start.Date));
                    if (dateWithNoData && start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday && publicHolidays.Any(x=>x.Date.Month != start.Date.Month && x.Date.Day != start.Date.Day))
                    {
                        theDate.Add(start.Date.ToString("dddd MMMM dd, yyyy"));
                        start = start.AddDays(1);
                    }
                    else
                    {
                        start = start.AddDays(1);
                    }

                }
        
            }
            return theDate;
        }

        public IList<string> GetDateWithoutLeaveData(DateTime start, DateTime end, ICollection<StaffLeaveModel> staffLeaveData)
        {
            var theDate = new List<string>();
            while (start.Date < end.Date)
            {
                var dateWithNoData = staffLeaveData.Any(x => x.LeaveDateStart.Date.Equals(start.Date));
                if (dateWithNoData && start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday)
                {
                    theDate.Add(start.Date.ToShortDateString());
                    start = start.AddDays(1);
                }
                else
                {
                    start = start.AddDays(1);
                }

            }
            return theDate;
        }
    }
}