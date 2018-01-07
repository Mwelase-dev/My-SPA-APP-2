using System;
using System.Text;

namespace Intranet.Messages
{
    //TODO - Jay Make this db Drive
    public static class MessageList
    {
        #region Messages for Clocking system
        public static string ClockingLate_Subject  { get { return "You have not clocked in yet."; } }
        public static string ClockingLate_Body
        { 
            get
            {
                var message = new StringBuilder().AppendLine("");
                message.AppendFormat("<html><body>");

                message.Append("<p>You appear not to have clocked in yet. Please clock in to avoid a day being automatically deducted as annual leave. </p>");
                message.Append("<p>Please click  <a href= \"http://Intranet/#/view_timekeeping_sheet\" >here</a> to clock in online.</p>");
                message.Append("<p>If you have already done so, please ignore this message. If you keep receiving the message after you have clocked in, please log a support ticket by sending an email to: support@nvestholdings.co.za</p>");


                message.Append("<p>Kind Regards</p><p>NVest Clocking System</p></body></html>");

                return message.ToString();
            }
        }
                                                                                                                                                                                                    
        public static string ClockingLeave_Subject { get { return "You have not clocked in yet. A leave day has been added to your account."; } }
        public static string ClockingLeave_Body 
        { 
            get 
            {
                var message = new StringBuilder().AppendLine("");
                message.AppendFormat("<html><body>");

                message.Append("<p>Because you have not clocked in yet, an automatic day's leave has been added to you account</p>");
                message.Append("<p>If this allocation is incorrect, please ask your immediate manager to update the record as soon as possible</p>");

                message.Append("<p>Kind Regards</p><p>NVest Clocking System</p></body></html>");
                return message.ToString();
            }
        }

        public static string IncompleteDayClockData_Subject
        {
            get { return "Incomplete day clock data"; }
        }
        public static string IncompleteDayClockData
        {
            get
            {
                var message = new StringBuilder().AppendLine("");
                message.AppendFormat("<html><body>");
                message.AppendFormat(
                    "<p>You appear to have missed a clock-in or clock-out on {0} – please check and correct and necessary</p><p></p>", DateTime.Today.Date.ToShortDateString());
                message.Append("<p>Kind Regards</p><p>NVest Clocking System</p></body></html>");

                return message.ToString();
            } 
        }

        public static string AutomaticLeaveApplication_Subject { get { return "Automatic leave application"; } }

        public static string AutomaticLeaveApplication
        {
            get
            {
                return "<html><body><p> An automated annual leave application has been approved as a result of you not clocking in today. Should this not be correct, then you will need to amend appropriately and please remember to clock in and out in future or ensure that if you are working outside of the office for the day, that you record it appropriately so that the system is aware that you are working on the applicable days</p><p>Kind Regards</p><p>NVest Clocking System</p></body></html>";
            }
        }
         
        #endregion

        #region Web request

        public static string WebRequestSubject {
            get { return "Website access request"; }
        }

        /// <summary>
        /// Returns the web request's body. Please formart the strings
        /// </summary>
        public static string WebRequestBody
        {
            get
            {
                //var message = new StringBuilder().AppendLine("");
                //message.AppendFormat("<html><body>");

                //message.Append("<p></p>");
                //message.Append("<p></p>");
                //message.Append("<p></p>");

                return "<html><p>Website access request from '{0}'. </p>" +
                                                           "<p>The Site: {1} </p>" +
                                                           "<p>Motivation: {2} </p>" +
                                                           "<p>Please email <a href = mailto:{3}>{4}</a> with your approval or  if the request is denied</p>" +
                                                           "</html>"; 
            }
        }


        public static string Open_Toner_Orders_Subject { get { return "There are toner orders still open"; } }
        public static string Open_Toner_Orders_Body { get
        {
            return "<html><body><p>Hi I.T </p> <p>This serves as a reminder to close open toner orders that have been open for a long time.<p>Kind Regards</p><p>NVest Clocking System</p></p></body></html>";
        } }

        #endregion


        // Forced change for Jay
    }
}
