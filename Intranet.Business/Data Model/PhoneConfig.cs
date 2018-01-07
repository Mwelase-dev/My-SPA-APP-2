using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Intranet.Models;

namespace Intranet.Business.Data_Model
{
    [XmlRootAttribute(ElementName = "settings")]
    public class PhoneSettings
    {
        #region Properties

        [XmlElement("phone-settings")]
        public PhoneConfig Config { get; set; }

        [XmlElement("tbook")]
        public PhoneBook Directory { get; set; }
        #endregion

        public PhoneSettings()
        {
            Directory = new PhoneBook();
        }

        public PhoneSettings(StaffPhoneDetailModel phoneDetail)
            : this()
        {
            Config = new PhoneConfig(phoneDetail);
            ProcessStaffContacts(phoneDetail);
        }

        #region Overrides
        //-- Returns serialized version of object -----------------------------------------
        public override string ToString()
        {
            return Serialize(this);
        }
        #endregion

        #region Methods

        //-- Converts a "Object" into XML string ------------------------------------------
        private static string Serialize<T>(T classObject)
        {
            if (classObject == null)
                return String.Empty;
            else
            {
                MemoryStream stream = null;//= new MemoryStream();
                StreamWriter writer = null;// = new StreamWriter(stream, Encoding.UTF8);
                try
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");

                    stream = new MemoryStream();
                    writer = new StreamWriter(stream, Encoding.UTF8);

                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, classObject, ns);

                    var count = (int)stream.Length;
                    var arr = new byte[count];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(arr, 0, count);

                    var utf = new UTF8Encoding();
                    return utf.GetString(arr).Trim();
                }
                catch (Exception ex)
                {
                    return string.Empty;
                }
                finally
                {
                    if (stream != null) stream.Close();
                    if (writer != null) writer.Close();
                }
            }
        }

        private void ProcessStaffContacts(StaffPhoneDetailModel phoneDetail)
        {

            //var count = 1;

            Directory.PhoneList = phoneDetail.StaffMember.StaffContactData.ToList().ConvertAll(m => new PhoneEntry()
                {
                    ContactId = m.ContactId,
                    UserName = string.Empty,

                    FirstName = m.ContactName,
                    LastName = m.ContactSurname,
                    Number = m.ContactNumber,
                    NumberType = "fixed",
                    //Index = count++,

                });
        }

        #endregion

    }

    public class PhoneConfig
    {

        #region Constant variables
        //  public const string Model = "snom300";
        #endregion

        #region Public variables

        [XmlElement("dns_domain")]
        public string DnsDomain { get; set; }

        [XmlElement("netmask")]
        public string NetMask { get; set; }

        [XmlElement("DHCP")]
        public string Dhcp = "Off";


        [XmlElement("rtp_encryption")]
        public string Rtp = "Off";

 

        [XmlElement("dhcp")]
        public RegData DHCP { get; set; }

        //[XmlElement("ip_adr")]
        //public RegData IpAddress { get; set; }
        [XmlElement("ip_adr")]
        public string IpAddress { get; set; }

        [XmlElement("gateway")]
        public string Gateway { get; set; }

        [XmlElement("dns_server1")]
        public string DnsServer1 { get; set; }

        [XmlElement("dns_server2")]
        public string DnsServer2 { get; set; }

        [XmlElement("language")]
        public string Language = "English";

        [XmlElement("ntp_server")]
        public string NtpServer = "172.16.0.5";

        [XmlElement("ntp_refresh_timer")]
        public float NtpRefreshTimer = 3600;

        [XmlElement("timezone")]
        public string Timezone = "CAT+2";

        [XmlElement("display_method")]
        public string DisplayMethod = "display_name";

        [XmlElement("auto_dial")]
        public float AutoDial = 5;

        [XmlElement("tone_scheme")]
        public string ToneScheme = "GBR";

        [XmlElement("subscription_delay")]
        public float SubscriptionDelay = 30;

        [XmlElement("no_dnd")]
        public string NoDnd = "no";

        [XmlElement("phone_name")]
        public string PhoneName { get; set; }

        [XmlElement("setting_server")]
        public string SettingServer = "http://Intranet/api/breezedata/StaffPhone?id=$mac";//http://172.16.1.75/api/breezedata/StaffPhone?id=$mac //"id=$mac"//http://172.16.1.75/api/breezedata/StaffPhone/

        [XmlElement("settings_refresh_timer")]
        public float SettingsRefreshTimer = 3600;

        [XmlElement("update_policy")]
        public string UpdatePolicy = "auto_update";

        [XmlElement("user_realname")]
        public RegData UserRealname { get; set; }

        [XmlElement("user_name")]
        public RegData Usename { get; set; }

        [XmlElement("user_host")]
        public RegData UserHost { get; set; }

        //[XmlElement("user_srtp!")]
        //public string Srtp1 ="off"; //Added by Mwelase 2015-11-30 T 18h17

        [XmlElement("user_pname")]
        public RegData UserPname { get; set; }

        [XmlElement("user_srtp")]
        public RegData UserSrtp = new RegData{Idx = "1", Perm = "",Value = "off"};

        [XmlElement("user_pass")]
        public RegData UserPass { get; set; }

        [XmlElement("user_mailbox")]
        public RegData UserMailbox = new RegData { Perm = "RW", Value = "off" };

        [XmlElement("user_ringer")]
        public RegData UserRinger { get; set; }

        [XmlElement("user_outbound")]
        public RegData UserOutbound { get; set; }

        [XmlElement("action_offhook_url")]
        public string ActionOffhookUrl { get; set; }

        [XmlElement("action_onhook_url")]
        public string ActionOnhookUrl { get; set; }

        


        #endregion

        public PhoneConfig()
        {
            #region Init Properties

            DnsDomain = string.Empty;
            NetMask = string.Empty;
            DHCP = new RegData();
             //IpAddress = new RegData();
            Gateway = string.Empty;
            DnsServer1 = string.Empty;
            DnsServer2 = string.Empty;
            UserRealname = new RegData();

            Usename = new RegData();
            UserHost = new RegData();
            UserPname = new RegData();
            UserPass = new RegData();
            UserRinger = new RegData();
            UserOutbound = new RegData();
            

            #endregion
        }

        public PhoneConfig(StaffPhoneDetailModel phoneDetail)
            :this()
        {
            DnsDomain = phoneDetail.StaffPhoneDomain;
            NetMask = phoneDetail.StaffPhoneNetMask;
            Gateway = phoneDetail.StaffPhoneGateway;
            DnsServer1 = phoneDetail.StaffPhoneDNS1;
            DnsServer2 = phoneDetail.StaffPhoneDNS2;
            IpAddress = phoneDetail.StaffPhoneIp;

            PhoneName = string.Concat("Ext-", phoneDetail.StaffMember.StaffTellExt);
            ActionOnhookUrl = "http://Intranet/api/breezedata/UpdatePhoneStatusOnhook?id=$mac";//"http://172.16.1.75/api/breezedata/UpdatePhoneStatusOnhook?id=$mac"
            ActionOffhookUrl = "http://Intranet/api/breezedata/UpdatePhoneStatusOffhook?id=$mac";//http://172.16.1.75/api/breezedata/UpdatePhoneStatusOffhook?id=$mac

        

            UserRealname = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffMember.StaffFullName };
            Usename = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffMember.StaffTellExt };
            UserPname = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffMember.StaffTellExt };
            UserPass = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffPhonePass };
            UserRinger = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffPhoneRinger };
            UserHost = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffPhoneHost };
            UserOutbound = new RegData() { Idx = "1", Perm = "RW", Value = phoneDetail.StaffPhoneOutBound };

            DHCP = new RegData { Idx = "1", Perm = "RW", Value = "off" };
        }
    }

    public class RegData
    {
        [XmlAttribute]
        public string Idx { get; set; }

        [XmlAttribute]
        public string Perm { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    [XmlRootAttribute(ElementName = "item")]
    public class PhoneEntry
    {
        [XmlIgnore]
        public String UserName { get; set; }

        [XmlIgnore]
        public Guid ContactId { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public Int64 Index { get; set; }

        [XmlAttribute("mod")]
        public bool Mod { get; set; }

        [XmlAttribute("fav")]
        public bool Fav { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("context")]
        public string Context { get; set; }

        [XmlElement("number")]
        public string Number { get; set; }

        [XmlElement("number_type")]
        public string NumberType { get; set; }

        [XmlElement("first_name")]
        public string FirstName { get; set; }

        [XmlElement("last_name")]
        public string LastName { get; set; }

        public PhoneEntry()
        {
            Type = string.Empty;
            Context = string.Empty;
        }
    }

    public class PhoneBook
    {
        [XmlAttribute("complete")] public bool Complete = true;

        [XmlElement("item")]
        public List<PhoneEntry> PhoneList { get; set; }

        public PhoneBook()
        {

        }
    }

}
