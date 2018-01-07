using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Intranet.Data.EF;
using Intranet.Data.EF;
using Intranet.Models;
using Intranet.Models.Enums;
using Utilities;
using Utilities.Helpers;

namespace Intranet.Messages
{

    public abstract class LeaveMessage
    {
        protected const string Applicant = "[Applicant]";
        public StaffLeaveModel LeaveModel { get; set; }
        public IEnumerable<StaffModel> StaffModels { get; set; }

        protected abstract MessagesEnum MessageType { get; }
        protected IList<string> ToEmailList { get; set; }
        protected IList<string> CcEmailList { get; set; }

        protected virtual string BuildOtherDetails() { return string.Empty; }
        protected abstract MessagesModel Fill();
        protected abstract void SetmailingList();

        protected double CalculateLeaveDays(DateTime start, DateTime end)
        {
            return start.DifferenceInDays(end);
        }

        protected double CalculateLeaveDays3(DateTime start, DateTime end)
        {
            var timespan = end - start;
            





            return start.DifferenceInDays(end);
            //TimeSpan leaveTimespan = end - start;
            //return leaveTimespan.Days;
        }


        protected double CalculateLeaveDays2(DateTime start, DateTime end)
        {
            TimeSpan theSpan = end - start;










            var leaveDays = 0;
            var weekendCounter = 0;

            var startValue = start;

            if (start == end)
                return leaveDays;

               while (startValue != end) {
                leaveDays += 1;

                //check if its a weekend
                var dayOfWeek = (int)startValue.DayOfWeek;
                if (dayOfWeek == 6 || dayOfWeek == 0)
                    weekendCounter += 1;

                //add a day to the start days

                startValue = startValue.AddDays(1);
            }

               return leaveDays - weekendCounter;
        }


        protected double CalculateLeaveHours(DateTime start, DateTime end)
        {
            TimeSpan leaveTimespan = end - start;
            if (leaveTimespan.Hours >= new TimeSpan(0,8,0,0).Hours)
            {
                return 8;
            }

            return leaveTimespan.Hours;
        }

        protected double CalculateLeaveMinutes(DateTime start, DateTime end)
        {
            //if (start != end)
            //    return 0;
            TimeSpan leaveTimespan = end - start;
            double leaveMinutes = (end - start).Minutes;
            return leaveTimespan.Minutes;
        }


        protected IEnumerable<StaffLeaveModel> OtherStaffGoingOnLeave(StaffModel staff, DateTime start, DateTime end)
        {
            return
                staff.StaffLeaveData.Where(
                    m =>
                    (m.LeaveDateStart.Date >= start.Date  &&
                    m.LeaveDateEnd.Date <= end.Date &&
                    m.LeaveStatus.Equals((int)LeaveStatus.Approved) || m.LeaveStatus.Equals((int)LeaveStatus.Pending)));
        }

        protected LeaveMessage()
        {
            ToEmailList = new List<string>();
            CcEmailList = new List<string>();
        }
        protected LeaveMessage(StaffLeaveModel leaveModel, IEnumerable<StaffModel> staff = null)
            : this(leaveModel)
        {
            StaffModels = staff;
        }
        protected LeaveMessage(StaffLeaveModel leave)
            : this()
        {
            LeaveModel = leave;
        }

        public bool SendMessage()
        {
            try
            {

                SetmailingList();

                var message = Fill();

                var mailer = new Emailer
                    {
                        subject = message.Subject,
                        body = ComposeFinalBody(message),

                    };

                ToEmailList.ToList().ForEach(mailer.TOList.Add);
                CcEmailList.ToList().ForEach(mailer.CCList.Add);

                mailer.SendEmail();

                return true;
            }
            catch (Exception exception)
            {
                //todo need to do some loggin here
                throw;
            }

        }

        protected MessagesModel GetLeaveMessage(MessagesEnum messageType)
        {
            var message = (int)messageType;

            var leavemsg = new DataContextEF().Messages.FirstOrDefault(m => m.MessageType.Equals(message));

            if (leavemsg == null)
                throw new Exception("Leave approved message not found");

            return leavemsg;
        }

        protected IEnumerable<StaffModel> GetStaffManagers()
        {
            var managers =
           new DataContextEF().Staff.Where(
               m =>
               m.StaffId.Equals(LeaveModel.StaffMember.StaffManager1Id) ||
               m.StaffId.Equals(LeaveModel.StaffMember.StaffManager2Id));

            if (!managers.Any())
                throw new Exception("staff memmber has no managers");

            return managers.ToList();

        }

        private static string ComposeFinalBody(MessagesModel message)
        {
            //todo - jay - make this a string builder object
            var body = message.Greeting += "\n\n";
            body += message.Body + "\n";
            body += message.Signature;

            return body;
        }

    }

    public class LeaveApplications : LeaveMessage
    {

        protected override MessagesEnum MessageType
        {
            get
            {
                return MessagesEnum.LeaveApplication;
            }
        }

        #region Data filling points
        private const string Receiver       = "[Receiver]";
        private const string Leavetype      = "[LeaveType]";
        private const string FromDate       = "[FromDate]";
        private const string ToDate         = "[ToDate]";
        private const string Days           = "[LeaveDays]";
        private const string RequestDate    = "[RequestDate]";
        private const string Comments       = "[Comments]";
        private const string RedirectLink   = "[RedirectLink]";
        private const string OtherDetails   = "[otherDetails]";
        #endregion

        #region Public Properties
        // public StaffLeaveModel LeaveModel { get; set; }
        //public IEnumerable<StaffModel> Staff { get; set; }
        #endregion

        #region Constructors
        public LeaveApplications()
        {
        }

        public LeaveApplications(StaffLeaveModel leaveModel, IEnumerable<StaffModel> staff = null)
            : base(leaveModel, staff)
        {
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Fills the message with values
        /// </summary>
        protected sealed override MessagesModel Fill()
        {
            var leaveMsg = GetLeaveMessage(MessageType);

            leaveMsg.Greeting = leaveMsg.Greeting.Replace(Receiver, "Manager (s)");

            TimeSpan leaveSpan = (LeaveModel.LeaveDateEnd - LeaveModel.LeaveDateStart);
            var leaveDays = 0;
            var leaveHours = 0;
            var leaveMinutes = 0;

            leaveDays = leaveSpan.Days;
            leaveHours = leaveSpan.Hours;
            leaveMinutes = leaveSpan.Minutes;
            if (leaveSpan.Hours >= 8)
            {
                leaveDays++;
                leaveHours = 0;
            }

            //get require hours
            using (var contextEf = new DataContextEF())
            {
                var staffHours = contextEf.StaffHourData.Where(x => x.StaffId.Equals(LeaveModel.StaffId)).ToList();
                #region 

                //double requiredMinutes = 0;
                //double requiredHours = 0;


                //for (int i = 0; i < staffHours.Count; i++)
                //{
                //    if (staffHours[i].DayId == (int)DateTime.Now.DayOfWeek)
                //    {
                //        requiredHours = (staffHours[i].DayTimeEnd.TimeOfDay - staffHours[i].DayTimeStart.TimeOfDay).Hours;
                //        requiredMinutes = (staffHours[i].DayTimeEnd.TimeOfDay - staffHours[i].DayTimeStart.TimeOfDay).Minutes;
                //        break;
                //    }
                //}

                //if (leaveHours == requiredHours)
                //{
                //    leaveHours = 0;

                //    if (leaveDays == 0)
                //    {
                //        leaveDays = leaveDays + 1;

                //    }
                //    else if (leaveDays > 0)
                //    {
                //        leaveDays++;
                //    }
                //}

                #endregion
                leaveMsg.Body = leaveMsg.Body
                                        .Replace(Applicant, LeaveModel.StaffMember.StaffFullName)
                                        .Replace(Leavetype, EnumHelper.GetEnumDescriptions((LeaveType)LeaveModel.LeaveType))
                                        .Replace(FromDate, LeaveModel.LeaveDateStart.ToLongDateString() + " " + LeaveModel.LeaveDateStart.TimeOfDay/*.Add(addToHours)*/)
                                        .Replace(ToDate, LeaveModel.LeaveDateEnd.ToLongDateString() + " " + LeaveModel.LeaveDateEnd.TimeOfDay/*.Add(addToHours)*/)
                                        .Replace(RequestDate, LeaveModel.LeaveRequestDate.ToShortDateString())
                                        .Replace(Days, leaveDays +
                                          " Day(s), " + leaveHours
                                        + " Hour(s), " + leaveMinutes
                                        + " Minute(s) ")
                                        .Replace(Comments, LeaveModel.LeaveComments)
                                        .Replace(RedirectLink, "http://intranet/#/view_theleave/" + LeaveModel.LeaveId)
                                        .Replace(OtherDetails, StaffModels.Any() ? BuildOtherDetails() : string.Empty);

                return leaveMsg;
            }
           
        }

        /// <summary>
        /// Process staff in same divisions who are also on leave
        /// </summary>
        /// <param name="staff"></param>
        /// <returns></returns>
        protected sealed override string BuildOtherDetails()
        {
            var details = new StringBuilder();

            if (StaffModels == null)
                return String.Empty;
            details.Append(string.Format("<table><th>Staff Name</th><th>Leave Start</th><th>Leave End</th><th>Application Status</th>"));
            StaffModels.ToList().ForEach(m =>
                {
                    var otherStaffLeave = OtherStaffGoingOnLeave(m, LeaveModel.LeaveDateStart, LeaveModel.LeaveDateEnd).ToList();
                    if (otherStaffLeave.Any())
                    {
                        otherStaffLeave.ToList().ForEach(x =>
                                details.Append(string.Format("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>",
                                m.StaffFullName, x.LeaveDateStart.ToShortDateString(), x.LeaveDateEnd.ToShortDateString(), ((LeaveStatus)x.LeaveStatus).ToString())));
                    }

                });
             
             details.Append(string.Format("</table>"));

            return details.ToString();
        }

        protected sealed override void SetmailingList()
        {

            var managers =
                new DataContextEF().Staff.Where(
                    m =>
                    m.StaffId.Equals(LeaveModel.StaffMember.StaffManager1Id) ||
                    m.StaffId.Equals(LeaveModel.StaffMember.StaffManager2Id));

            if (!managers.Any())
                throw new Exception("staff memmber has no managers");

            //set to email list
            managers.ToList().ForEach(m => ToEmailList.Add(m.StaffEmail));

            CcEmailList.Add(LeaveModel.StaffMember.StaffEmail);
        }

        #endregion

    }

    public class DeclineLeaveApplication : LeaveMessage
    {
        protected override MessagesEnum MessageType
        {
            get
            {
                return MessagesEnum.LeaveApplicationDeclined;
            }
        }

        #region Data filling points
        private const string DeclineReason = "[DeclineReason]";
        #endregion

        #region Constructors
        public DeclineLeaveApplication()
        {
        }

        public DeclineLeaveApplication(StaffLeaveModel leaveModel, IEnumerable<StaffModel> staff = null)
            : base(leaveModel, staff) { }
        #endregion

        #region Private methods
        /// <summary>
        /// Fills the message with values
        /// </summary>
        protected sealed override MessagesModel Fill()
        {
            var leaveMsg = GetLeaveMessage(MessageType);

            leaveMsg.Greeting = leaveMsg.Greeting.Replace(Applicant, LeaveModel.StaffMember.StaffFullName);

            leaveMsg.Body = leaveMsg.Body
                                    .Replace(Applicant, LeaveModel.StaffMember.StaffFullName)
                                    .Replace(DeclineReason, LeaveModel.LeaveComments);

            leaveMsg.Body += "\n\n" + leaveMsg.Signature;

            return leaveMsg;
        }

        protected sealed override void SetmailingList()
        {
            ToEmailList.Add(LeaveModel.StaffMember.StaffEmail);

            var managers =
           new DataContextEF().Staff.Where(
               m =>
               m.StaffId.Equals(LeaveModel.StaffMember.StaffManager1Id) ||
               m.StaffId.Equals(LeaveModel.StaffMember.StaffManager2Id));

            if (!managers.Any())
                throw new Exception("staff memmber has no managers");

            //set to email list
            managers.ToList().ForEach(m => CcEmailList.Add(m.StaffEmail));
        }
        #endregion
    }

    public class ApproveLeaveApplication : LeaveMessage
    {
        protected override MessagesEnum MessageType
        {
            get
            {
                return MessagesEnum.LeaveApplicationApproved;
            }
        }

        #region Data filling points

        private const string StartDate = "[StartDate]";
        private const string EndDate = "[EndDate]";

        #endregion

        #region Constructor

        public ApproveLeaveApplication()
        {

        }

        public ApproveLeaveApplication(StaffLeaveModel leaveModel, IEnumerable<StaffModel> staff = null)
            : base(leaveModel, staff)
        {

        }
        #endregion

        #region private methods

        protected override MessagesModel Fill()
        {
            var addTwoHours = new TimeSpan(0,2,0,0);
            var leavemsg = GetLeaveMessage(MessageType);

            leavemsg.Greeting = leavemsg.Greeting.Replace(Applicant, LeaveModel.StaffMember.StaffFullName);
            leavemsg.Body = leavemsg.Body
                                    .Replace(StartDate, (LeaveModel.LeaveDateStart.ToLongDateString() + " " + LeaveModel.LeaveDateStart.TimeOfDay.Add(addTwoHours)))
                                    .Replace(EndDate, (LeaveModel.LeaveDateEnd.ToLongDateString() + " " + LeaveModel.LeaveDateEnd.TimeOfDay.Add(addTwoHours)));

            return leavemsg;
        }

        protected override void SetmailingList()
        {
            ToEmailList.Add(LeaveModel.StaffMember.StaffEmail);

            GetStaffManagers()
                .ToList()
                .ForEach(m => CcEmailList.Add(m.StaffEmail));
        }

        #endregion
    }

    public class CancelLeaveApplication : LeaveMessage
    {
        protected override MessagesEnum MessageType
        {
            get { return MessagesEnum.LEaveApplicationCancelled; }
        }
        #region Data filling points
        private const string Receiver = "[Receiver]";
        private const string Leavetype = "[LeaveType]";
        private const string FromDate = "[FromDate]";
        private const string ToDate = "[ToDate]";
        private const string Days = "[LeaveDays]";
        private const string RequestDate = "[RequestDate]";
        private const string Comments = "[Comments]";
        private const string RedirectLink = "[RedirectLink]";
        
        
       
        private const string DeclineReason = "[DeclineReason]";
        #endregion

        #region Constructors
        public CancelLeaveApplication()
        {

        }

        public CancelLeaveApplication(StaffLeaveModel leaveModel)
            : base(leaveModel)
        {

        }
        #endregion

        #region Private/Inherited methods

        protected override MessagesModel Fill()
        {
            var message = GetLeaveMessage(MessageType);
            message.Greeting = message.Greeting.Replace(Receiver, LeaveModel.StaffMember.StaffFullName);
            message.Greeting = message.Greeting.Replace(Applicant, LeaveModel.StaffMember.StaffFullName);
            TimeSpan leaveSpan = (LeaveModel.LeaveDateEnd - LeaveModel.LeaveDateStart);
            var leaveDays = 0;
            var leaveHours = 0;
            var leaveMinutes = 0;

            leaveDays = leaveSpan.Days;
            leaveHours = leaveSpan.Hours;
            leaveMinutes = leaveSpan.Minutes;
            if (leaveSpan.Hours >= 8)
            {
                leaveDays++;
                leaveHours = 0;
            }

            message.Body = message.Body
                .Replace(Applicant, LeaveModel.StaffMember.StaffFullName)
                .Replace(Leavetype, EnumHelper.GetEnumDescriptions((LeaveType) LeaveModel.LeaveType))
                .Replace(FromDate,
                    LeaveModel.LeaveDateStart.ToLongDateString() + " " + LeaveModel.LeaveDateStart.TimeOfDay
                )
                .Replace(ToDate, LeaveModel.LeaveDateEnd.ToLongDateString() + " " + LeaveModel.LeaveDateEnd.TimeOfDay
                )
                .Replace(RequestDate, LeaveModel.LeaveRequestDate.ToShortDateString())
                .Replace(Days, leaveDays +
                               "Days, " + leaveHours
                               + "Hours, " + leaveMinutes
                               + "Minutes ")
                .Replace(Comments, LeaveModel.LeaveComments)
                .Replace(RedirectLink, "http://intranet/#/view_theleave/" + LeaveModel.LeaveId)
                .Replace(DeclineReason, LeaveModel.ReasonForAction);
                               

            return message;
        }

        protected override void SetmailingList()
        {
            ToEmailList.Add(LeaveModel.StaffMember.StaffEmail);

            GetStaffManagers()
                .ToList()
                .ForEach(m => CcEmailList.Add(m.StaffEmail));
        }
        #endregion
    }
}
