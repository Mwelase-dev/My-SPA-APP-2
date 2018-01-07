using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Intranet.Data.EF;
using Intranet.Business.Data_Model;
using Intranet.Models;

namespace Intranet.Business
{
    public class UowPhone
    {
        public static string GetPhoneSettings(string phoneMacAddress)
        {

            if (string.IsNullOrEmpty(phoneMacAddress))
                return string.Empty;

            using (var store = new DataContextEF())
            {
                //Get a user's phone details
                var phoneDet = store.PhoneDetails
                                    .Include("StaffMember")
                                    .Include("StaffMember.StaffContactData")
                                    .FirstOrDefault(
                                        m => m.RecordStatus.Equals("Active") && m.StaffPhoneMac.Equals(phoneMacAddress));

                if (phoneDet == null)
                    throw new Exception("Staff phone details not found");

                phoneDet.StaffMember.StaffContactData = phoneDet.StaffMember.StaffContactData.Where(x => x.RecordStatus.Equals("Active")).ToList();

                var phoneSettings = new PhoneSettings(phoneDet);

               store.Staff.Where(m => m.StaffId != phoneDet.StaffMember.StaffId && m.RecordStatus.Equals("Active")).OrderBy(x => x.StaffName)
                        .ToList() .ForEach((m) => phoneSettings.Directory.PhoneList.Add(new PhoneEntry
                               {
                                   ContactId = m.StaffId,
                                   UserName = string.Empty,
                                   FirstName = m.StaffName,
                                   LastName = m.StaffSurname,
                                   Number = m.StaffTellExt,
                                   NumberType = "sip",
                               
                               }));

                var count = 1;
                var sortedData = phoneSettings.Directory.PhoneList.OrderBy(x => x.LastName).ThenBy(x=>x.FirstName);
                foreach (PhoneEntry phoneEntry in sortedData)
                {
                    phoneEntry.Index = count;
                    count++;
                }
                phoneSettings.Directory.PhoneList =  new List<PhoneEntry>(phoneSettings.Directory.PhoneList.OrderBy(x => x.LastName).ThenBy(x => x.FirstName));
                return phoneSettings.ToString();

                #region 

                /*var workContactsToStaffContactsModel = new List<StaffContactModel>();

                workContacts.ForEach((m) => workContactsToStaffContactsModel.Add(new StaffContactModel
                {
                    ContactDescription = m.StaffName,
                    ContactId          =  m.StaffId,
                    ContactName        = m.StaffName,
                    ContactNumber      = m.StaffTellExt,
                    ContactSurname     = m.StaffSurname,
                    StaffId            = m.StaffId,
                    StaffMember        = m
                }));


               // workContactsToStaffContactsModel.AddRange(personalContacts); 

               // var phoneSettings = new PhoneSettings(phoneDet);

                store.Staff.Where(m => m.StaffId != phoneDet.StaffMember.StaffId && m.RecordStatus.Equals("Active")).OrderBy(x => x.StaffName)
                           .ToList()
                           .ForEach((m) => phoneSettings.Directory.PhoneList.Add(new PhoneEntry
                               {
                                   ContactId = m.StaffId,
                                   UserName = string.Empty,

                                   FirstName = m.StaffName,
                                   LastName = m.StaffSurname,
                                   Number = m.StaffTellExt,
                                   NumberType = "sip",
                                   Index = phoneSettings.Directory.PhoneList.Count + 1
                               }));

                return phoneSettings.ToString();*/

                #endregion

            }
        }

        public static string UpdatePhoneStatus(string phoneStatus, string mac)
        {
            using (var dtx = new DataContextEF())
            {
                var staffPhone = dtx.PhoneDetails.Include("StaffMember").FirstOrDefault(x => x.RecordStatus.Equals("Active") && x.StaffPhoneMac.Equals(mac));
                if (staffPhone != null)
                {
                    staffPhone.StaffMember.StaffPhoneStatus = phoneStatus;
                    //staffPhone.StaffMember.PhoneDataStatus = Convert.ToInt32(phoneStatus);
                }
                dtx.SaveChanges();

                if (staffPhone != null && staffPhone.StaffMember != null)
                    return "Hello " + staffPhone.StaffMember.StaffFullName + "....... You called me from the phone";

            }

         
            return "Hello Anonymous";
        }
    }
}
#region
/*
 * using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Intranet.Data.EF;
using Intranet.Business.Data_Model;

namespace Intranet.Business
{
    public class UowPhone
    {
        public static string GetPhoneSettings(string phoneMacAddress)
        {

            if (string.IsNullOrEmpty(phoneMacAddress))
                return string.Empty;

            using (var store = new DataContextEF())
            {
                //Get a user's phone details
                var phoneDet = store.PhoneDetails
                                    .Include("StaffMember")
                                    .Include("StaffMember.StaffContactData")
                                    .FirstOrDefault(
                                        m => m.RecordStatus.Equals("Active") && m.StaffPhoneMac.Equals(phoneMacAddress));

                if (phoneDet == null)
                    throw new Exception("Staff phone details not found");

                var phoneSettings = new PhoneSettings(phoneDet);

                store.Staff.Where(m => m.StaffId != phoneDet.StaffMember.StaffId && m.RecordStatus.Equals("Active"))
                           .ToList()
                           .ForEach((m) => phoneSettings.Directory.PhoneList.Add(new PhoneEntry
                               {
                                   ContactId = m.StaffId,
                                   UserName = string.Empty,

                                   FirstName = m.StaffName,
                                   LastName = m.StaffSurname,
                                   Number = m.StaffTellExt,
                                   NumberType = "sip",
                                   Index = phoneSettings.Directory.PhoneList.Count + 1
                               }));

                return phoneSettings.ToString();
            }
        }

        public static string UpdatePhoneStatus(string phoneStatus, string mac)
        {
            using (var dtx = new DataContextEF())
            {
                var staffPhone = dtx.PhoneDetails.Include("StaffMember").FirstOrDefault(x => x.RecordStatus.Equals("Active") && x.StaffPhoneMac.Equals(mac));
                if (staffPhone != null) staffPhone.StaffMember.StaffPhoneStatus = phoneStatus;
                dtx.SaveChanges();

                if (staffPhone != null && staffPhone.StaffMember != null)
                    return "Hello " + staffPhone.StaffMember.StaffFullName + "....... You called me from the phone";

            }

         
            return "Hello Anonymous";
        }
    }
}

*/
#endregion
