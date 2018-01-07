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
using Utilities;

namespace Intranet.UI.Models
{
    public class CompanyLeaveReport
    {
        public string CompanyName { get; set; }
        public Guid CompanyId { get; set; }
        public List<DivisionLeaveReportModel> CompanyDivisions { get; set; }
        public double CompanyTotal { get; set; }

        //Leave records


        public CompanyLeaveReport()
        {
            CompanyDivisions = new List<DivisionLeaveReportModel>();
        }

        public CompanyLeaveReport(Guid compId, string compayName, IEnumerable<DivisionModel> branchDivisions, string startdate, string enddate, string leavetype)
            : this()
        {
            CompanyId = compId;
            CompanyName = compayName;
            CompanyDivisions = branchDivisions
                .ToList()
                .ConvertAll(
                    m => new DivisionLeaveReportModel(
                        m.DivisionId,
                        m.DivisionName,
                        m.BranchId,
                        m.DivisionStaff,startdate,enddate,leavetype));

            CompanyTotal = CompanyDivisions.Select(x => x.DivisionTotal).Sum().Round(2);

        }

    }

    public class DivisionLeaveReportModel
    {
        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; }
        public Guid BranchId { get; set; }
        public IList<StaffLeaveReportModel> DivisionStaff { get; set; }
        public double DivisionTotal { get; set; }



        public DivisionLeaveReportModel()
        {
            DivisionStaff = new List<StaffLeaveReportModel>();
        }

        public DivisionLeaveReportModel(Guid divisionId, string divisionName, Guid branchId, IEnumerable<StaffModel> branchStaff, string startdate, string enddate, string leavetype)
            : this()
        {
            DivisionId = divisionId;
            DivisionName = divisionName;
            BranchId = branchId;
             
            DivisionStaff = branchStaff
                .ToList()
                .ConvertAll(
                    m => new StaffLeaveReportModel(
                        m.StaffId,
                        m.StaffFullName,
                        m.DivisionId,
                        m.StaffLeaveData,startdate,enddate,leavetype));
            DivisionTotal = DivisionStaff.Select(x => x.DynamicRunningTotal).Sum().Round(2);
        }

         
    }

    public class StaffLeaveReportModel
    {
        public Guid StaffId { get; set; }
        public string Fullname { get; set; }
        public Guid DivisionId { get; set; }
        public double RunningTotal { get; set; }
        public double DynamicRunningTotal { get; set; }
        public ICollection<StaffLeaveModel> StaffLeaveRecords { get; set; }
        public double DaysAccrued { get; set; }
        public List<LeaveDates> LeaveDateTimes { get; set; }
        public double DaysTaken { get; set; }
        public double DaysTakenAfterEndDateInRange { get; set; }
        public double DaysAtEndOfMonth { get; set; }


        public StaffLeaveReportModel()
        {
            StaffLeaveRecords = new List<StaffLeaveModel>();
        }

        public StaffLeaveReportModel(Guid staffId, string fullName, Guid divisionId, ICollection<StaffLeaveModel> leaveRecords, string startdate, string enddate, string leavetype)
            : this()
        {
            DaysAtEndOfMonth = UoWStaffLeave.GetDaysAtEndOfMonth(DateTime.Parse(enddate), staffId);
            DaysTakenAfterEndDateInRange = UoWStaffLeave.GetDaysTakenAfterEndDateInRange(DateTime.Parse(enddate), staffId);

            StaffId = staffId;
            Fullname = fullName;
            DivisionId = divisionId;
            StaffLeaveRecords = leaveRecords;
            LeaveDateTimes = new List<LeaveDates>();
            foreach (StaffLeaveModel staffLeaveModel in leaveRecords)
            {
                var leaveDates = new LeaveDates
                {
                    StartDate = staffLeaveModel.LeaveDateStart,
                    EndDate = staffLeaveModel.LeaveDateEnd
                };
                LeaveDateTimes.Add(leaveDates);
                
            }

            double totalDaysDue = UoWStaffLeave.GetDaysDue(StaffId); //leaveRecords.Select(staffLeaveRecord => staffLeaveRecord.StaffMember.DaysDue).FirstOrDefault();
            RunningTotal = totalDaysDue.Round(2);
             
            var acru = UoWStaffLeave.GetIncrement(StaffId).Round(2);
            DaysAccrued = acru;

            double dtotal = leaveRecords.Sum(staffLeaveModel => staffLeaveModel.LeaveDateStart.DifferenceInDaysPayroll(staffLeaveModel.LeaveDateEnd));
            DaysTaken = dtotal;

            var daysDue = UoWStaffLeave.GetDaysAccumulated(StaffId);
            var daysDueAsAtRange = (DaysAtEndOfMonth + DaysAccrued) - dtotal.Round(2);
            DynamicRunningTotal = (daysDueAsAtRange.Round(2) - DaysTakenAfterEndDateInRange).Round(2);
             
             #region //Casting and  //Reflection  and //Assigning
            //var listofobjTarynWants = new Collection<StaffLeaveModel>();
            //var objTarynWants = UoWStaffLeave.GetLaveData(staffId,startdate,enddate,leavetype);
            //listofobjTarynWants.AddRange(objTarynWants);

         // listofobjTarynWants;
 
          
            //var leaveDetails = new List<StaffLeaveModel>();
            //var obj = UoWStaffLeave.GetLeaveSummaryDetails(staffId).ToList();

           

            //foreach (object prop in obj)
            //{
            //    //Casting
            //    IList<object> list = new List<object>();
            //    list.Add(new { Allocated = 0, LeaveType = (LeaveType)0, Taken = 0, CarryOver = 0, RunningTotal = 0 });
            //    object anon = new { Year = 0, Data = list };
            //    var x = Cast(prop, anon);

            //    //Reflection
            //    Type t = x.GetType();
            //    PropertyInfo p = t.GetProperty("Data");
            //    PropertyInfo p1 = t.GetProperty("Year");
            //    object v = p.GetValue(x);
            //    object q = p1.GetValue(x);

            //    //Assigning
            //    foreach (object dataT in (IEnumerable)p.GetValue(x))
            //    {
            //        var leave = new LeaveDetails();
            //        Type zz = dataT.GetType();
            //        PropertyInfo p2 = zz.GetProperty("Year");
            //        PropertyInfo p3 = zz.GetProperty("LeaveType");
            //        PropertyInfo p4 = zz.GetProperty("LeaveCarriedOver");
            //        PropertyInfo p5 = zz.GetProperty("LeaveTaken");
            //        PropertyInfo p6 = zz.GetProperty("LeaveAllocation");

            //        object v2 = p2.GetValue(dataT);
            //        object v3 = p3.GetValue(dataT);
            //        object v4 = p4.GetValue(dataT);
            //        object v5 = p5.GetValue(dataT);
            //        object v6 = p6.GetValue(dataT);

            //        leave.Year = (int)v2;
            //        leave.LeaveType = (LeaveType)v3;
            //        leave.LeaveCarriedOver = (double)v4;
            //        leave.LeaveTaken = (int)v5;
            //        leave.LeaveAllocation = (double)v6;

            //        leaveDetails.Add(leave);
            //    }

            //public static T Cast<T>(object obj, T type)
            //{
            //    return (T)obj;
            //}

            #endregion
        }

        /*
          public StaffLeaveReportModel(Guid staffId, string fullName, Guid divisionId, ICollection<StaffLeaveModel> leaveRecords, string startdate, string enddate, string leavetype)
            : this()
        {
            DaysAtEndOfMonth = UoWStaffLeave.GetDaysAtEndOfMonth(DateTime.Parse(enddate), staffId);
            StaffId = staffId;
            Fullname = fullName;
            DivisionId = divisionId;
            StaffLeaveRecords = leaveRecords;
            LeaveDateTimes = new List<LeaveDates>();
            foreach (StaffLeaveModel staffLeaveModel in leaveRecords)
            {
                var leaveDates = new LeaveDates
                {
                    StartDate = staffLeaveModel.LeaveDateStart,
                    EndDate = staffLeaveModel.LeaveDateEnd
                };

                LeaveDateTimes.Add(leaveDates);
                
            }

            double totalDaysDue = UoWStaffLeave.GetDaysDue(StaffId); //leaveRecords.Select(staffLeaveRecord => staffLeaveRecord.StaffMember.DaysDue).FirstOrDefault();
            RunningTotal = totalDaysDue.Round(2);
             
            var acru = UoWStaffLeave.GetIncrement(StaffId);
            DaysAccrued = (double)acru;


            double dtotal = leaveRecords.Sum(staffLeaveModel => staffLeaveModel.LeaveDateStart.DifferenceInDaysPayroll(staffLeaveModel.LeaveDateEnd));
            DaysTaken = dtotal;


            var daysDue = UoWStaffLeave.GetDaysAccumulated(StaffId);
            var daysDueAsAtRange = (daysDue - dtotal).Round(2);

            DynamicRunningTotal = daysDueAsAtRange.Round(2);
             
             #region //Casting and  //Reflection  and //Assigning
            //var listofobjTarynWants = new Collection<StaffLeaveModel>();
            //var objTarynWants = UoWStaffLeave.GetLaveData(staffId,startdate,enddate,leavetype);
            //listofobjTarynWants.AddRange(objTarynWants);

         // listofobjTarynWants;
 
          
            //var leaveDetails = new List<StaffLeaveModel>();
            //var obj = UoWStaffLeave.GetLeaveSummaryDetails(staffId).ToList();

           

            //foreach (object prop in obj)
            //{
            //    //Casting
            //    IList<object> list = new List<object>();
            //    list.Add(new { Allocated = 0, LeaveType = (LeaveType)0, Taken = 0, CarryOver = 0, RunningTotal = 0 });
            //    object anon = new { Year = 0, Data = list };
            //    var x = Cast(prop, anon);

            //    //Reflection
            //    Type t = x.GetType();
            //    PropertyInfo p = t.GetProperty("Data");
            //    PropertyInfo p1 = t.GetProperty("Year");
            //    object v = p.GetValue(x);
            //    object q = p1.GetValue(x);

            //    //Assigning
            //    foreach (object dataT in (IEnumerable)p.GetValue(x))
            //    {
            //        var leave = new LeaveDetails();
            //        Type zz = dataT.GetType();
            //        PropertyInfo p2 = zz.GetProperty("Year");
            //        PropertyInfo p3 = zz.GetProperty("LeaveType");
            //        PropertyInfo p4 = zz.GetProperty("LeaveCarriedOver");
            //        PropertyInfo p5 = zz.GetProperty("LeaveTaken");
            //        PropertyInfo p6 = zz.GetProperty("LeaveAllocation");

            //        object v2 = p2.GetValue(dataT);
            //        object v3 = p3.GetValue(dataT);
            //        object v4 = p4.GetValue(dataT);
            //        object v5 = p5.GetValue(dataT);
            //        object v6 = p6.GetValue(dataT);

            //        leave.Year = (int)v2;
            //        leave.LeaveType = (LeaveType)v3;
            //        leave.LeaveCarriedOver = (double)v4;
            //        leave.LeaveTaken = (int)v5;
            //        leave.LeaveAllocation = (double)v6;

            //        leaveDetails.Add(leave);
            //    }

            //public static T Cast<T>(object obj, T type)
            //{
            //    return (T)obj;
            //}

            #endregion
        }
         */
    }
}