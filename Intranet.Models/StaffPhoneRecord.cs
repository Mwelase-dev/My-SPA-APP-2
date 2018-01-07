using System;
using System.Globalization;

namespace Intranet.Models
{
    public class StaffPhoneRecord
    {
        #region Private Methoods
        private double getCallCost(string Number, int duration)
        {
            double CostPerSecond = 0;
            var dialingCode = 0;
            if (Number.Length >= 3)
            {
                try
                {
                    var data = Number.Substring(0, 3);
                    Int32.TryParse(data, out dialingCode);

                    if (dialingCode < 90) CostPerSecond = 0.018;
                    if (dialingCode < 70) CostPerSecond = 0.007;
                    if (dialingCode == 87) CostPerSecond = 0.003;
                    if (dialingCode == 86) CostPerSecond = 0.009;
                    if (dialingCode == 80) CostPerSecond = 0.009;
                    if (dialingCode == 43) CostPerSecond = 0.007;

                    if (CostPerSecond == 0)
                    {
                        CostPerSecond = 0;
                    }
                }
                catch
                {

                }
            }
            return CostPerSecond * duration;
        }
        #endregion

        #region Public Variables
        public DateTime calldate { get; set; } // DateTime of the call
        public string clid { get; set; } // Caller Id (2000 or "Natalie")
        public string src { get; set; } // Source Channel
        public string dst { get; set; } // Destination
        public string dcontext { get; set; } // "From-Internal"
        public string channel { get; set; } // Channel involved (SIP/700-0000d1b1)
        public string dstchannel { get; set; } // Detination
        public string lastapp { get; set; } // Dialed, Reset CDR etc
        public string lastdata { get; set; } // SIP/RSH_087/0437427017,300,
        public string duration { get; set; } // Call duration (Not for Billing)
        public int    billsec { get; set; } // Call duration (for Billing)
        public string disposition { get; set; } // "ANSWERED" / "NO ANSWER"
        public string amaflags { get; set; } // No sure what this is!
        public string accountcode { get; set; } // Empty
        public string uniqueid { get; set; } // Unique ID
        public string userfield { get; set; } // Audio file label
        public string carrier { get; set; } //Trunk Name

        public string Extension
        {
            get
            {
                if (this.channel == null)
                    return String.Empty;
                else
                {
                    var liStart = this.channel.IndexOf("/") + 1;
                    var liEnd = this.channel.IndexOf("-") - liStart;
                    return this.channel.Substring(liStart, liEnd);
                }
            }
        }
        public string Destination
        {
            get
            {
                if (this.dstchannel == null)
                    return "";
                else
                {
                    var liStart = this.dstchannel.IndexOf("/") + 1;
                    var liEnd = this.dstchannel.IndexOf("-") - liStart;
                    return this.dstchannel.Substring(liStart, liEnd);
                }
            }
        }
        public int CallDuration
        {
            get { return billsec; }
        }
        public double CallCost
        {
            get { return getCallCost(this.dst, this.billsec); }
        }
        public string Carrier { get { return carrier; } }
        public string CallDurationString
        {
            get { return TimeSpan.FromSeconds(this.CallDuration).ToString(); }
        }

        #region Display fields

        public string DisplayCallDate
        {
            get { return calldate.ToString("dd/MM/yyyy HH:mm:ss"); }
        }

        public string DisplayCallCost
        {
            get { return CallCost.ToString("C",CultureInfo.CurrentCulture); }
        }

        public string DisplayDestination { get; set; }

        #endregion

        #endregion


    }

    public class StaffPhoneRecordDetail
    {
        public string Destination { get; set; }
        public string DisplayDestination { get; set; }
        public int    CallCount { get; set; }
        public double TotalCallCost { get; set; }
        public string DisplayTotalCallCost { get { return TotalCallCost.ToString("C", CultureInfo.CurrentCulture); } }
        public double CallDuration { get; set; }
        public double DisplayLength { get; set; }
        public string CallDurationString
        {
            get { return TimeSpan.FromSeconds(this.CallDuration).ToString(); }
        }
    }
}