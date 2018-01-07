using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Data.EF;
using Intranet.Models;
using Intranet.Models.Enums;
using Utilities;

namespace Intranet.Business
{
    public class UoWStaffLeave
    {
        static readonly DataContextEF ctx = new DataContextEF();

        public static IList<StaffLeaveModel> GetLaveData(Guid staffId, string startdate, string enddate, string leavetype)
        {
            var leaveList = ctx.StaffLeaveData
                .Include("StaffMember").ToList();

            if (leaveList.Any())
            {
                leaveList = leaveList.Where(
                    m =>
                        m.LeaveDateStart.Date >= DateTime.Parse(startdate).Date &&
                        m.LeaveDateEnd.Date <= DateTime.Parse(enddate).Date &&
                
                        m.LeaveType != 7).ToList();
            }

            return leaveList;
        }

        public static IEnumerable<object> GetLeaveSummaryDetails(Guid staffId)
        {
          
                var leaveList = ctx.StaffLeaveData
               .Include("StaffMember")
               .Where(x => x.StaffId.Equals(staffId) && x.RecordStatus.Equals("Active") && x.LeaveStatus.Equals(1))//.Where(x => x.RecordStatus.Equals("Active"))
               .OrderByDescending(x => x.LeaveRequestDate)
               .ToList()
               .Select(g => new
               {
                   g.LeaveDateStart,
                   g.LeaveDateEnd,
                   g.LeaveId,
                   g.LeaveRequestDate,
                   g.LeaveStatus,
                   g.LeaveType,
                   g.StaffId,
                   g.StaffMember,
                   LeaveTaken = CalculateLeaveTaken(g.LeaveDateStart, g.LeaveDateEnd)
               });

                var leaveDetails = (from l in leaveList
                                    group l by new { l.LeaveDateStart.Year, l.LeaveType, l.StaffMember }
                                        into yearGroup
                                        select new
                                        {
                                            yearGroup.Key.Year,
                                            LeaveType = ((LeaveType)yearGroup.Key.LeaveType),
                                            LeaveTaken = yearGroup.Sum(l => l.LeaveTaken),
                                            LeaveAllocation = yearGroup.Key.StaffMember,
                                        })
                                        .ToList()
                                        .Select(g => new
                                        {
                                            g.Year,
                                            g.LeaveType,
                                            g.LeaveTaken,
                                            LeaveAllocation = StaffLeaveAllocation((int)g.LeaveType, g.LeaveAllocation).Round(2),
                                        }).ToList()
                                        .Select(g => new
                                        {
                                            g.Year,
                                            g.LeaveType,
                                            g.LeaveTaken,
                                            g.LeaveAllocation,
                                            LeaveCarriedOver = (g.LeaveAllocation - g.LeaveTaken).Round(2)
                                        });

                var leaveDetailsPerYear = (from l in leaveDetails
                                           group l by l.Year
                                               into yearGroup
                                               select new { Year = yearGroup.Key, Data = yearGroup.ToList() }).ToList();

                return leaveDetailsPerYear.AsQueryable().OrderBy(x => x.Year);
           
            
        }

        //public static IEnumerable<LeaveDetails> GetLeaveSummaryDetailsKnown(Guid staffId)
        //{

        //    var leaveList = ctx.StaffLeaveData
        //   .Include("StaffMember")
        //   .Where(x => x.StaffId.Equals(staffId) && x.RecordStatus.Equals("Active"))//.Where(x => x.RecordStatus.Equals("Active"))
        //   .OrderByDescending(x => x.LeaveRequestDate)
        //   .ToList()
        //   .Select(g => new
        //   {
        //       g.LeaveDateStart,
        //       g.LeaveDateEnd,
        //       g.LeaveId,
        //       g.LeaveRequestDate,
        //       g.LeaveStatus,
        //       g.LeaveType,
        //       g.StaffId,
        //       g.StaffMember,
        //       LeaveTaken = CalculateLeaveTaken(g.LeaveDateStart, g.LeaveDateEnd)
        //   });

        //    var leaveDetails = (from l in leaveList
        //                        group l by new   { l.LeaveDateStart.Year, l.LeaveType, l.StaffMember }
        //                            into yearGroup
        //                            select new  
        //                            {
        //                                yearGroup.Key.Year,
        //                                LeaveType = ((LeaveType)yearGroup.Key.LeaveType),
        //                                LeaveTaken = yearGroup.Sum(l => l.LeaveTaken),
        //                                LeaveAllocation = yearGroup.Key.StaffMember,
        //                            })
        //                            .ToList()
        //                            .Select(g => new
        //                            {
        //                                g.Year,
        //                                g.LeaveType,
        //                                g.LeaveTaken,
        //                                LeaveAllocation = StaffLeaveAllocation((int)g.LeaveType, g.LeaveAllocation),
        //                            }).ToList()
        //                            .Select(g => new
        //                            {
        //                                g.Year,
        //                                g.LeaveType,
        //                                g.LeaveTaken,
        //                                g.LeaveAllocation,
        //                                LeaveCarriedOver = g.LeaveAllocation - g.LeaveTaken
        //                            });

        //    var leaveDetailsPerYear = (from l in leaveDetails
        //                               group l by l.Year
        //                                   into yearGroup
        //                                   select new { Year = yearGroup.Key, Data = yearGroup.ToList() }).ToList();

        //    return leaveDetailsPerYear.AsQueryable().OrderBy(x => x.Year);


        //}

        public static int CalculateLeaveTaken(DateTime leaveStart, DateTime leaveEnd)
        {
            //if (leaveStart.TimeOfDay.TotalSeconds > 1 || leaveEnd.TimeOfDay.TotalSeconds > 1)
            //{


                var leaveDaysTaken = 1;
                var weekendHolidayCounter = 0;

                DateTime startValue = leaveStart;

                var holidayList = ctx.Holidays
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
            //}
            return 1;
        }

        public static double StaffLeaveAllocation(int type, StaffModel staff)
        {
            double allocatedDays = 0;

            switch (type)
            {
                case 1:
                    allocatedDays = staff.LeaveDaysAccumulated;
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

        public static IEnumerable<object> GetAllLeaveSummaryDetails()
        {

            var leaveList = ctx.StaffLeaveData
             .Include("StaffMember")
             .Where(x => x.RecordStatus.Equals("Active") && x.LeaveStatus.Equals(1) && x.LeaveType.Equals(1))//.Where(x => x.RecordStatus.Equals("Active"))
             .OrderByDescending(x => x.LeaveRequestDate)
             .ToList()
             .Select(g => new
             {
                 g.LeaveDateStart,
                 g.LeaveDateEnd,
                 g.LeaveId,
                 g.LeaveRequestDate,
                 g.LeaveStatus,
                 g.LeaveType,
                 g.StaffId,
                 g.StaffMember,
                 LeaveTaken = CalculateLeaveTaken(g.LeaveDateStart, g.LeaveDateEnd)
             });

            var leaveDetails = (from l in leaveList
                                group l by new { l.LeaveDateStart.Year, l.LeaveType, l.StaffMember }
                                    into yearGroup
                                    select new
                                    {
                                        yearGroup.Key.Year,
                                        LeaveType = ((LeaveType)yearGroup.Key.LeaveType),
                                        LeaveTaken = yearGroup.Sum(l => l.LeaveTaken),
                                        LeaveAllocation = yearGroup.Key.StaffMember,
                                    })
                                    .ToList()
                                    .Select(g => new
                                    {
                                        g.Year,
                                        g.LeaveType,
                                        g.LeaveTaken,
                                        LeaveAllocation = StaffLeaveAllocation((int)g.LeaveType, g.LeaveAllocation).Round(2),
                                    }).ToList()
                                    .Select(g => new
                                    {
                                        g.Year,
                                        g.LeaveType,
                                        g.LeaveTaken,
                                        g.LeaveAllocation,
                                        LeaveCarriedOver = (g.LeaveAllocation - g.LeaveTaken).Round(2)
                                    });

            var leaveDetailsPerYear = (from l in leaveDetails
                                       group l by l.Year
                                           into yearGroup
                                           select new { Year = yearGroup.Key, Data = yearGroup.ToList() }).ToList();

            return leaveDetailsPerYear.AsQueryable().OrderBy(x => x.Year);
           
        }

        public static double GetDaysAccumulated(Guid staffId)
        {
            StaffModel staff = ctx.Staff.First(x => x.StaffId.Equals(staffId));
            return staff.LeaveDaysAccumulated;
        }

        public static double GetDaysDue(Guid staffId)
        {
            StaffModel staff = ctx.Staff.First(x => x.StaffId.Equals(staffId));
            return staff.DaysDue;
        }

        public static double GetIncrement(Guid staffId)
        {
            StaffModel staff = ctx.Staff.Include("StaffLeaveData").Include("LeaveCounters").First(x => x.StaffId.Equals(staffId));
            return (double) staff.StaffLeaveIncrement;
        }

        public static double GetOpenningBalance(Guid staffId)
        {
            StaffModel staff = ctx.Staff.Include("StaffLeaveData").Include("LeaveCounters").First(x => x.StaffId.Equals(staffId));
            return (double)staff.LeaveScheduleCarriedOver;
        }

        public static double GetDaysAtEndOfMonth(DateTime endDate, Guid staffId)
        {
            var deploymentDate = new DateTime(2015, 10, 31, 0, 0, 0);
            double total = 0;
            double acu = 0;
            if (endDate >= deploymentDate)
            {
                acu += ((endDate.Subtract(deploymentDate).Days / (365.25 / 12)) * (double)GetIncrement(staffId)) + GetOpenningBalance(staffId);
            }
            //else if (endDate.Date.Equals(new DateTime(2015,10,31)))
            //{
            //   acu = GetOpenningBalance(staffId);
            //}
            else
            {
                throw new Exception("Leave days are captured as at end of October");
            }

            total = acu.Round(2);

            return total;
        }

        public static double GetDaysTakenAfterEndDateInRange(DateTime endDate, Guid staffId)
        {
            var days = 0;
            StaffModel staff = ctx.Staff.Include("StaffLeaveData").Include("LeaveCounters").First(x => x.StaffId.Equals(staffId));
            var data = staff.StaffLeaveData.Where(x => x.LeaveDateStart.Date < endDate.Date && x.LeaveType!= 7 && x.LeaveType != 6).ToList();
            if (data.Any())
            {
                data = data.OrderByDescending(x => x.LeaveDateStart).ToList();
                //days = (int)data.Sum(x => x.LeaveDateStart.DifferenceInDaysPayroll(x.LeaveDateEnd));

                var test = (int)data.Sum(x => x.LeaveDateStart.DifferenceInDays(x.LeaveDateEnd));

                for (int i = 0; i < data.Count; i++)
                {
                    days += (int)data[i].LeaveDateStart.Date.DifferenceInDaysPayroll(data[i].LeaveDateEnd.Date);
                }

                if (data.Any(x => x.LeaveDateStart.Date.Year <= endDate.Date.Year && x.LeaveDateStart.Date.Month <= endDate.Date.Month))
                {
                    days = 0;
                }
                //if (data.Any(x => x.LeaveDateStart.Date >= endDate.Date))
                //{
                //    days = (int)data.Sum(x => x.LeaveDateStart.DifferenceInDaysPayroll(x.LeaveDateEnd));
                //}
                    
            }
            return days;
        }
    }
}
