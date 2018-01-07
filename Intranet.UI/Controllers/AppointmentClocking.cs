using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Intranet.Data.EF;
using Intranet.Models;
//using NHibernate.Hql.Ast.ANTLR;
//using Org.BouncyCastle.Asn1.Ocsp;
using Utilities;

namespace Intranet.UI.Controllers
{
    public class AppointmentClockingController : ApiController
    {
        readonly ContextProvider ctx = new ContextProvider();
        [HttpPost]
        public bool RemoteClockIn(StaffClockModel remoteClock)
        {
            remoteClock.RecordStatus = "Active";
            remoteClock.StaffId = GetStaffIdByEmail(remoteClock.Email);
            remoteClock.Comments = remoteClock.Comments +
                                   (". This clock in is of an appointment the staff member was in");

            try
            {
                using (DataContextEF dataContext = new DataContextEF())
                {
                    dataContext.StaffClockData.Add(remoteClock);
                    dataContext.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        [HttpGet]
        public object PeOfficeClockIn(string remoteClockData)
        {
            using (var dtx = new DataContextEF())
            {
                StaffClockModel remoteClock =
                    dtx.StaffClockData.First(x => x.Staff.StaffClockCardNumber.Equals(remoteClockData.Split('-').Last()));

                if (remoteClock != null)
                {
                    remoteClock.DataStatus = 0;
                    remoteClock.ClockDateTime = DateTime.Now;
                    remoteClock.RecordStatus = "Active";
                    remoteClock.StaffId = GetStaffIdByCardNumber(remoteClock.StaffId);
                    remoteClock.Comments = remoteClock.Comments +
                                           ("PE office clock in");

                    dtx.StaffClockData.Add(remoteClock);
                    dtx.SaveChanges();

                    
                    object obj = new
                    {
                        Name = remoteClock.Staff.StaffFullName,
                        Time = DateTime.Now.TimeOfDay,
                        CardNumber = remoteClock.Staff.StaffClockCardNumber

                    };

                    return Request.CreateResponse(HttpStatusCode.OK, obj);
                }
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }

        private Guid GetStaffIdByEmail(string staffEmail)
        {
            using (var dataContext = new DataContextEF())
            {
                var staffId = from staffmember in dataContext.Staff
                              where staffmember.StaffEmail.Equals(staffEmail)
                              select staffmember.StaffId;

                return staffId.FirstOrDefault();
            }
        }

        private Guid GetStaffIdByCardNumber(Guid staffid)
        {
            using (var dataContext = new DataContextEF())
            {
                var staffId = from staffmember in dataContext.Staff
                              where staffmember.StaffId.Equals(staffid)
                              select staffmember.StaffId;

                return staffId.FirstOrDefault();
            }
        }
    }

}