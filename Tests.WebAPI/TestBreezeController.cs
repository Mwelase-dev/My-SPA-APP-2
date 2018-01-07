using System;
using System.Diagnostics;
using Intranet.UI.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.WebAPI
{
    [TestClass]
    public class TestBreezeController
    {
        /// <summary>
        /// Tests the Breeze Metadata (EF and NH)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_Metadata()
        {
            var api = new BreezeDataController();
            var meta = api.MetaData();
            Assert.IsTrue(meta != String.Empty);
            Debug.WriteLine(meta);
        }

        /// <summary>
        /// Tests the Metadata generation from Breeze
        /// </summary>
        [TestMethod]
        public void Test_API_Get_Menu()
        {
            var api = new BreezeDataController();
            var menuList = api.Menus();
            Assert.IsNotNull(menuList);
            foreach (var menuModel in menuList)
            {
                Debug.WriteLine(menuModel.MenuName);
            }
        }

        /// <summary>
        /// Tests the API call for getting a random thought
        /// </summary>
        [TestMethod]
        public void Test_API_Get_ThoughtRandom()
        {
            var api = new BreezeDataController();
            var thought = api.ThoughtRandom();
            Assert.IsNotNull(thought);
            Debug.WriteLine(thought.Thought);
        }

        /// <summary>
        /// Test the API method for retrieving a full list of thoughts (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_ThoughtList()
        {
            var api = new BreezeDataController();
            var thoughts = api.ThoughtList();
            Assert.IsNotNull(thoughts);
            foreach (var thoughtModel in thoughts)
            {
                Debug.WriteLine(thoughtModel.Thought);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of link categories (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_LinkCategories()
        {
            var api = new BreezeDataController();
            var linkCats = api.LinkCategories();
            Assert.IsNotNull(linkCats);
            foreach (var data in linkCats)
            {
                Debug.WriteLine(data.CategoryDesc);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of links (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_Links()
        {
            var api = new BreezeDataController();
            var links = api.Links();
            Assert.IsNotNull(links);
            foreach (var data in links)
            {
                Debug.WriteLine(data.LinkDesc);
                //Debug.WriteLine(data.LinkCategory.CategoryDesc);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of Branches (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_BranchList()
        {
            var api = new BreezeDataController();
            var data = api.BranchList();
            Assert.IsNotNull(data);
            foreach (var branch  in data)
            {
                Debug.WriteLine(branch.BranchName);
                foreach (var division in branch.BranchDivisions)
                {
                    Debug.WriteLine("\t"+division.DivisionName);
                    foreach (var staffMem in division.DivisionStaff)
                    {
                        Debug.WriteLine("\t\t" + staffMem.StaffFullName);
                    }
                }
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of Branch Divisions (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_DivisionList()
        {
            var api = new BreezeDataController();
            var data = api.DivisionList();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.DivisionName);
                //Debug.WriteLine(item.DivisionBranch.BranchName);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of (Brief) Staff members (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_Staff()
        {
            var api = new BreezeDataController();
            var data = api.Staff();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.StaffFullName);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a full list of (Brief) Staff members (For maintenance)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_StaffExtended()
        {
            var api = new BreezeDataController();
            var data = api.Staff();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.StaffFullName);
                //Debug.WriteLine(item.St .StaffIsOnLeave);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a list of Staff birthdays (Read only)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_StaffBirthdays()
        {
            var api = new BreezeDataController();
            var data = api.StaffBirthdays();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.StaffFullName);
                Debug.WriteLine(item.StaffBirthday.ToString());
            }
        }

        /// <summary>
        /// Test the API method for retrieving a list of Staff suggestions (For main page UI)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_StaffSuggestions_BySuggestion()
        {
            var api = new BreezeDataController();
            var data = api.StaffSuggestions();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.SuggestionSubject);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a list of photo Galleries (Read Only)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_GalleryList()
        {
            var api = new BreezeDataController();
            var data = api.GalleryList();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.GalleryDesc);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a list of Gallery Images (Read Only)
        /// </summary>
        [TestMethod]
        public void Test_API_Get_GalleryImageList()
        {
            var api = new BreezeDataController();
            var data = api.GetGalleryImageList("Testing");
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.FileName);
            }
        }

        /// <summary>
        /// Test the API method for retrieving a list of Public Holidays
        /// </summary>
        [TestMethod]
        public void Test_API_Get_HolidayList()
        {
            var api = new BreezeDataController();
            var data = api.HolidaysList();
            Assert.IsNotNull(data);
            foreach (var item in data)
            {
                Debug.WriteLine(item.HolidayDescription);
            }
        }

        /// <summary>
        /// Test the API method for getting the current AD user
        /// </summary>
        [TestMethod]
        public void Test_API_Get_CurrentUser()
        {
            var api = new BreezeDataController();
            var data = api.CurrentUser();
            Assert.IsNotNull(data);
            Debug.WriteLine(data);
        }

        /// <summary>
        /// Test the API method for checking if the user is in the IntranetAdmins group
        /// </summary>
        [TestMethod]
        public void Test_API_Get_CurrentUserIsAdmin()
        {
            var api = new BreezeDataController();
            var data = api.CurrentUserIsAdmin();
            Assert.IsNotNull(data);
            Debug.WriteLine(data);
        }
        
        /// <summary>
        /// Test to retrieve the clocking data from the devices and save them to the DB
        /// </summary>
        [TestMethod]
        public void Test_API_ProcessClockingData()
        {
            var api = new BreezeDataController();
            Assert.IsTrue(api.ProcessClockingData());
        }

        [TestMethod]
        public void Test_API_Get_StaffClockModelWithStaffMemberExpanded()
        {
            var api = new BreezeDataController();
            var data = api.StaffClockModelAll();
            Assert.IsNotNull(data);
            Debug.WriteLine(data);
        }


        [TestMethod]
        public void Test_API_Get_JustToProveStaffModelIsNotWorkingWithClockData()
        {
            var api = new BreezeDataController();
            var data = api.StaffLeave();
            Assert.IsNotNull(data);
            Debug.WriteLine(data);
        }


    }
}
