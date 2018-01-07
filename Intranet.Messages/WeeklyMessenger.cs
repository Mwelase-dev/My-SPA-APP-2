using Intranet.Data.EF;
using Intranet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Models.Enums;
using Utilities;
using Utilities.Helpers;

namespace Intranet.Messages
{
    public static class WeeklyMessenger
    {
        public static bool EmialManager(string managerEmail, IList<StaffModel> staff)
        {
            const MessagesEnum messageType = MessagesEnum.WeeklyLeaveClockEmail;

            var message = new DataContextEF().Messages
                                             .FirstOrDefault(m => m.MessageType.Equals((int)messageType));
            //if (message == null)
            //    throw new Exception("Weekly message type not found");

            var mailer = new Emailer
                {
                    subject = "Pending Clock Records"
                };
            #region

#if DEBUG
            mailer.TOList.Add("mtshona@nvestholdings.co.za");
#else
            mailer.TOList.Add(managerEmail);
#endif
            #endregion

            mailer.body = BuildClockData(staff);
            if (!string.IsNullOrEmpty(mailer.body))
                mailer.SendEmail();

            return true;
        }

        private static string BuildClockData(IList<StaffModel> staff)
        {

            var message = new StringBuilder().AppendLine("");

            message.AppendFormat("<html><head><style>");

            message.AppendFormat("table {{	border-collapse: collapse;}}table, td, th {{	border: 1px; padding : 20px;}} th {{background-color: black; color: white; padding : 20px;}}");

            message.AppendFormat("</style>");

            message.AppendFormat("</head>" + "<p>To whom it may concern;</p> <p>Please find below a record of the amendments made to the NVest clock-in records made by " +
                                 "employees in your division that need to be approved or rejected. </p>" +
                                 "<div><p>Amendments pending:</p></div>");
            message.AppendFormat("<table><thead><th>Employee Name</th><th>Date In Question</th><th>Previous Clock Time(hh:mm)</th><th>Amendment</th><th>Employee Comments</th><th>System Comments</th></thead/>");

            if (!staff.Any())
                return string.Empty;

           
            IList<WeeklyUnapprovedClockData> weeklyUnapprovedClockData = new List<WeeklyUnapprovedClockData>();
            foreach (StaffModel staffModel in staff)
            {
               
                foreach (StaffClockModel staffClockModel in staffModel.StaffClockData)
                {
                    WeeklyUnapprovedClockData weeklyUnapproved = new WeeklyUnapprovedClockData();
                    weeklyUnapproved.Name = staffModel.StaffFullName;
                    string valueToFind = staffClockModel.Comments.Between("'", "'");

                    string commentReason = staffClockModel.Comments.Split('=').Last();
                    weeklyUnapproved.IsNewRecord = valueToFind == "NaN:NaN" ? true : false;

                    DateTime? nullDateTime = null;

                    weeklyUnapproved.OriginalRecordBeforeAmmendment = valueToFind == "NaN:NaN" ? nullDateTime : DateTime.Parse(valueToFind);
                    weeklyUnapproved.Comments = String.IsNullOrEmpty(commentReason) ? "No reason given" : commentReason;
                    weeklyUnapproved.Ammendment = staffClockModel.ClockDateTime.TimeOfDay.ToString();
                    weeklyUnapproved.DateInQuestion = staffClockModel.ClockDateTime.ToShortDateString();

                    string systemCommuication = valueToFind == "NaN:NaN" ? "User inserted clock-in or clock-out  that previously was not recorded" : "User amended a clock-in or clock-out time";
                    weeklyUnapproved.SystemComments = systemCommuication;
                     
                    weeklyUnapprovedClockData.Add(weeklyUnapproved);
                }
            }

            weeklyUnapprovedClockData.ToList().ForEach(x => message.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td> {3}</td><td> {4}</td><td> {5}</td></tr>", 
                x.Name, x.DateInQuestion, x.OriginalRecordBeforeAmmendment.Value, x.Ammendment, x.Comments, x.SystemComments));
             
            message.Append("</table>");
            message.Append("<br/><div>Please click <a href=\"http://Intranet/#/view_approveclockrecord\">Here</a> to go and approve or reject these amendments.</div>");

            message.Append("<p>Kind Regards</p><p>NVest Clocking System</p></html>");

            return message.AppendLine(string.Empty).ToString();
        }

        public static string Between(this string src, string findfrom, string findto)
        {
            int start = src.IndexOf(findfrom, StringComparison.Ordinal);
            int to = src.IndexOf(findto, start + findfrom.Length, StringComparison.Ordinal);
            if (start < 0 || to < 0) return "";
            string s = src.Substring(
                           start + findfrom.Length,
                           to - start - findfrom.Length);
            return s;
        }
    }

    public class WeeklyUnapprovedClockData
    {
        public String Name { get; set; }
        public Boolean IsNewRecord { get; set; }
        public DateTime? OriginalRecordBeforeAmmendment { get; set; }
        public String Ammendment { get; set; }
        public String Comments { get; set; }
        public String DateInQuestion { get; set; }
        public String SystemComments { get; set; }
    }
}
