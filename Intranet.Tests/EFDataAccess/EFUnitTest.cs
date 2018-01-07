using System;
using System.Diagnostics;
using System.Linq;
using Intranet.Data.EF;
using Intranet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Intranet.Business;

namespace EFDataAccessTests
{
    [TestClass]
    public class EFUnitTest
    {
        [TestMethod]
        public void Test_EF_Create_Staff()
        {
            var ctx = new DataContextEF();
            var staff = new StaffModel
                {
                    StaffId       = Guid.NewGuid(),
                    StaffName     = "Quentin",
                    StaffSurname  = "Barnard",
                    StaffIdNumber = "780418",
                    RecordStatus  = "Active",
                    StaffDivision = ctx.BranchDivisions.FirstOrDefault()
                };
            ctx.Staff.Add(staff);
            ctx.SaveChanges();
        }

        [TestMethod]
        public void Test_EF_ReadingStaffFullname()
        {
            var ctx = new DataContextEF();
            foreach (var staffModel in ctx.Staff.ToList())
            {
                Debug.WriteLine(staffModel.StaffFullName);
            }
        }

        [TestMethod]
        public void Test_EF_ReadingStaffClockingData()
        {
            var ctx = new DataContextEF();
            foreach (var staff in ctx.Staff.ToList())
            {
                foreach (var staffClockModel in staff.StaffClockData)
                {
                    //
                    Debug.WriteLine("--");
                    Debug.WriteLine(staffClockModel.Staff.StaffFullName);
                }
            }
        }

        [TestMethod]
        public void Test_EF_ReadingStaffRandomOtherStaffData()
        {
            IQueryable<StaffModel> test; 
            var ctx = new DataContextEF();
            foreach (var staff in ctx.Staff.Include("StaffClockData").ToList())
            {
                foreach (var staffClockModel in staff.StaffClockData.Where(x=>x.DataStatus.Equals(2)))
                {
                    
                    Debug.WriteLine("--");
                    Debug.WriteLine(staffClockModel.ClockDateTime);

                     
                }
            }
        }




        [TestMethod]
        public void Test_EF_CreatingStaffWorkingHours()
        {
            var ctx = new DataContextEF();
            foreach (var staff in ctx.Staff.ToList())
            {
                if ((staff.StaffHoursData == null) || (staff.StaffHoursData.Count == 0))
                {
                    for (int i = 0; i < 7; i++)
                    {
                        staff.StaffHoursData.Add(
                            new StaffHoursModel
                                {
                                    DayId          = i + 1,
                                    DayTimeStart   = Convert.ToDateTime("08:00:00"),
                                    DayLunchLength = 1,
                                    DayTimeEnd     = Convert.ToDateTime("17:00:00"),
                                    RecordStatus   = "Active",
                                    StaffId        = staff.StaffId
                                }
                            );
                    }
                }
            }
            ctx.SaveChanges();
        }
    
        [TestMethod]
        public void Test_EF_ProcessClockingData()
        {
            using (var ctx = new DataContextEF())
            {
                var isHoliday = ctx.IsHolidayToday();
                UoWStaff.ProcessClocking();
                ctx.SaveChanges();
            }
        }

        [TestMethod]
        public void Test_EF_GetStaffSuggestions()
        {
            using (var ctx = new DataContextEF())
            {
                var staffList = ctx.Staff.ToList();
                foreach (var item in staffList)
                {
                    Debug.WriteLine(item.StaffFullName);
                    Debug.WriteLine(item.Suggestions.Count());
                    foreach (var suggestionModel in item.Suggestions)
                    {
                        Debug.WriteLine(suggestionModel.SuggestionSubject);
                    }
                }
            }
        }

        [TestMethod]
        public void Test_EF_LeaveData()
        {
            using (var ctx = new DataContextEF())
            {
                var staffList = ctx.Staff.ToList();
                foreach (var staff in staffList)
                {
                    Debug.WriteLine(staff.StaffFullName);
                    Debug.WriteLine(staff.StaffIsOnLeave.ToString());

                    Debug.WriteLine(staff.StaffLeaveData.Count);
                    if (staff.StaffLeaveData.Count > 0)
                    {
                        foreach (var staffLeaveModel in staff.StaffLeaveData)
                        {
                            //Debug.WriteLine(staffLeaveModel.ApprovedBy1.StaffFullName);
                            //Debug.WriteLine(staffLeaveModel.ApprovedBy2.StaffFullName);
                        }
                    }
                    Debug.WriteLine("");
                }
            }
        }


       
    }
}
