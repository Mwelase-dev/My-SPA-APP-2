using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Intranet.Data.NH;
using Intranet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Mapping;
using NHibernate.Metadata;
using Intranet.UI.Controllers;
using Intranet.Business;

namespace Tests.NHDataAccess
{
    [TestClass]
    public class NHUnitTest
    {
        //[TestMethod]
        public void Test_NH_AllNHibernateMappingAreOkay()
        {
            var data = DataContextNH.SessionFactory;
            var obj = data.OpenSession();

            var allClassMetadata = obj.SessionFactory.GetAllClassMetadata();
            foreach (var entry in allClassMetadata)
            {
                obj
                    .CreateCriteria(entry.Key)
                    .SetMaxResults(0)
                    .List();
            }
        }

        //[TestMethod]
        public void Test_NH_ReadingStaffClockingData()
        {
            var ctx = new IntranetNhContext();
            foreach (var staff in ctx.Context.Staff.ToList())
            {
                foreach (var staffClockModel in staff.StaffClockData)
                {
                    //
                    Debug.WriteLine("--");
                }
            }
        }

        /// <summary>
        /// Tests to ensure we can pull the data from the clocking devices, based on IP's
        /// (in the config file) and saves it to the DB.
        /// </summary>
        //[TestMethod]
        public void Test_NH_ProcessClockingData()
        {
            using (var ctx = new IntranetNhContext())
            {
                var isHoliday = ctx.IsHolidayToday();
                UoWStaff.ProcessClocking();
                //ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Tests to ensure we can push data by using the "file download " option
        /// from the clocking devices and pass in the TextFile for processing and saves it to the DB.
        /// </summary>
        //[TestMethod]
        public void Test_NH_ProcessClockingDataFromFilesingle()
        {
            using (var ctx = new IntranetNhContext())
            {
                String lsFileName = @"..\..\Clock Data Files\TIME005.TXT";

                var isHoliday = ctx.IsHolidayToday();
                UoWStaff.ProcessClocking(lsFileName);
                //ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Tests to ensure we can push data by using "file download" option from the clocking devices
        /// and pass in a list of TextFiles for processing and saves it to the DB.
        /// </summary>
        //[TestMethod]
        public void Test_NH_ProcessClockingDataFromFileList()
        {
            using (var ctx = new IntranetNhContext())
            {
                IList<String> fileList = new List<String>();
                fileList.Add(@"..\..\Clock Data Files\TIME005.TXT");
                fileList.Add(@"..\..\Clock Data Files\TIME006.TXT");
                fileList.Add(@"..\..\Clock Data Files\TIME009.TXT");

                var isHoliday = ctx.IsHolidayToday();
                UoWStaff.ProcessClocking(fileList);
                //ctx.SaveChanges();
            }
        }

        /// <summary>
        /// Test that "STaffExtended" computed properties are accessible
        /// </summary>
        //[TestMethod]
        public void Test_NH_StaffFullnameMapping()
        {
            var ctx = new IntranetNhContext();

            foreach (var staff in ctx.Staff)
            {
                Debug.WriteLine("********************************************");
                
                Debug.WriteLine("* Fullname - "      + staff.StaffFullName  + " *"     );
                Debug.WriteLine("* Date of birth - " + staff.StaffDob       + "      *");
                Debug.WriteLine("* Birthday - "      + staff.StaffBirthday  + " *"     );
                Debug.WriteLine("* Birthday - "      + staff.StaffIsOnLeave + " *"     );
                Debug.WriteLine("********************************************");
            }
        }
    
        //[TestMethod]
        public void Test_NH_StaffManager()
        {
            var ctx = new IntranetNhContext();
            foreach (var staff in ctx.Staff)
            {
                Debug.WriteLine("********************************************");
                Debug.WriteLine("* Manager - " + staff.StaffManager1.StaffFullName);
                Debug.WriteLine("********************************************");
                break;
            }
            
        }
    
        //[TestMethod]
        public void Test_NH_StaffDivisionBranch()
        {
            var ctx = new IntranetNhContext();
            foreach (var staff in ctx.Staff)
            {
                Debug.WriteLine(String.Format("Branch: {0} | Division: {1} | Name: {2}",
                    //staff.StaffDivision.DivisionBranch.BranchName,
                    //staff.StaffDivision.DivisionName,
                    staff.StaffFullName));
                break;
            }
        }
    }
}
