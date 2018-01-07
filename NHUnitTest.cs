using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Intranet.Data.NH;
using Intranet.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Mapping;
using NHibernate.Metadata;
using Intranet.UI.Controllers;

namespace Tests.NHDataAccess
{
    [TestClass]
    public class NHUnitTest
    {
        [TestMethod]
        public void AllNHibernateMappingAreOkay()
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
        
        [TestMethod]
        public void Test_NH_ReadingStaffClockingData()
        {
            //var ctx = new IntranetNhContext();
            //foreach (var staffExtendedModel in ctx.Context.StaffExtended.ToList())
            //{
            //    foreach (var staffClockModel in staffExtendedModel.StaffClockData)
            //    {
            //        //
            //        Debug.WriteLine("--");
            //    }
            //}
        }
    }
}
