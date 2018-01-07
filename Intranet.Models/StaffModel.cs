using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Intranet.Models.Enums;
using Utilities;

namespace Intranet.Models
{
    public class StaffModel : BaseModel
    {

        #region Properties
        // Personal Info
        public virtual Guid DivisionId { get; set; }
        public virtual Guid StaffId { get; set; }
        public virtual String StaffName { get; set; }
        public virtual String StaffSurname { get; set; }
        public virtual String StaffIdNumber { get; set; }
        public virtual String StaffEmail { get; set; }
        public virtual String StaffTellExt { get; set; }
        public virtual String StaffTelDirect { get; set; }
        public virtual String StaffCellPhone { get; set; }
        public virtual String StaffFaxNumber { get; set; }
        public virtual String StaffNtName { get; set; }
        public virtual Guid? ClockDevice { get; set; }

        //Clock Data
        public virtual Int32 ClockDataId { get; set; }
        public virtual Int32? StaffClockCardNumber { get; set; }

        // Leave Data
        public virtual DateTime StaffJoinDate { get; set; }

        public virtual double LeaveScheduleCarriedOver { get; set; }

        // Clocking Data
        public virtual bool StaffIsClockingMember { get; set; }
        public virtual int StaffClockId { get; set; }
        public virtual int StaffClockReminders { get; set; }

        // HR Data
        public virtual Guid StaffManager1Id { get; set; }
        public virtual Guid StaffManager2Id { get; set; }
        public virtual String StaffDesignation { get; set; }

        // Phone Data
        public virtual string StaffPhoneIp { get; set; }
        public virtual string StaffPhonePass { get; set; }
        public virtual string StaffPhoneMac { get; set; }
        public virtual string StaffPhoneStatus { get; set; }
        public int PhoneDataStatus { get; set; }
         //css styling
        public string PhoneDataStatusClass
        {
            get
            {
                switch (StaffPhoneStatus)
                {
                    case "Busy":
                        return "busy";
                    
                    case "Available" :
                        return "available";

                    default:
                        return "";
                }
            }
        }

        /*
            public string PhoneDataStatusClass
        {
            get
            {
                switch ((PhoneStatus)PhoneDataStatus)
                {
                    case PhoneStatus.Offhook:
                        return "busy";
                    
                    case PhoneStatus.Onhook :
                        return "available";

                    default:
                        return "";
                }
            }
        }
         */

        //User Roles
        public virtual ICollection<UserRolesModel> Roles { get; set; }

        //Staff phone details
        public virtual StaffPhoneDetailModel PhoneDetails { get; set; }

        public virtual bool RecieveSystemMail { get; set; }
        public virtual bool OffsiteApproval { get; set; }

        #endregion

        //public List<StaffModel> ManagerFullNameStaffFullName
        //{
        //    get
        //    {
        //        var managerAndAssistant = new List<StaffModel>();

        //        managerAndAssistant.Add(this);
        //        managerAndAssistant.Add(StaffManager1);

        //        return managerAndAssistant;
        //    }
        //}

        #region to display on the leave summary
        #region Leave data

        #region Public Properties/Methods
        public virtual ICollection<StaffLeaveSummary> LeaveSummary { get; set; }
        public double AnnualDays
        {
            get
            {
                if (LeaveSummary == null || !LeaveSummary.Any())
                    return 0;
                var staffLeaveSummary = LeaveSummary.FirstOrDefault(m => m.LeaveType.Equals(1));

                return (double)(staffLeaveSummary == null ? 0 : (staffLeaveSummary.Increment * 12));
            }
        }
        public double SickDaysTaken
        {
            get
            {
                double days = 0;
                 const int approvedLeave = 1;

                if (StaffLeaveData == null)
                    return days;

                //this is the int representation of the sick leave enum
                const int leavetype = 2;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype) && m.LeaveStatus.Equals(approvedLeave)))
                {
                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                        days += 1; //todo-jay: cater for half days.
                    else
                        days += (leaveModel.LeaveDateStart.Date - leaveModel.LeaveDateEnd.Date).TotalDays;
                }

                return Math.Abs(days);
            }
        }

        public double SickCycleNumber
        {
            get
            {

                var sickcycleterm = 1;
                if (DateTime.Now.Year - StaffJoinDate.Year <= 3)
                {
                    return sickcycleterm;
                }
                if (DateTime.Now.Year - StaffJoinDate.Year > 3)
                {
                    Int32 term = (DateTime.Now.Year - StaffJoinDate.Year) / 3;

                    return term + 1;
                }

                return sickcycleterm;
            }
        }

        public DateTime SickCycleStart
        {
            get
            {


                if (DateTime.Now.Year == StaffJoinDate.Year)
                {
                    return StaffJoinDate;
                }

                var addYears = DateTime.Now.Year - StaffJoinDate.Year;
                if (addYears < 3)
                {
                    return StaffJoinDate;
                }
                if (addYears > 3)
                {
                    TimeSpan tenure = DateTime.Now - StaffJoinDate;
                    var tenureYears = tenure.Days / 365.25;

                    int term = (DateTime.Now.Year - StaffJoinDate.Year);

                    if (tenureYears > term)
                        term++;

                    return StaffJoinDate.AddYears(term - 3);
                }


                //var testDate = StaffJoinDate;
                //while (testDate.Year < DateTime.Now.Year)
                //{
                //    testDate = testDate.AddYears(2);//.AddMonths(11).AddDays();
                //    testDate = testDate.AddMonths(11);
                //    testDate = testDate.AddDays(1);

                //}

                //if (addYears > 3)
                //{
                //    int term = (DateTime.Now.Year - StaffJoinDate.Year)/3;
                //    term++;
                //    return StaffJoinDate.AddYears(term);
                //}
                return StaffJoinDate;

            }
        }

        public DateTime SickCycleEnd
        {
            get
            {
                //get { return CycleStart.AddMonths(11).AddDays(30); }


                var theSickCycleEnd = SickCycleStart.AddYears(2);
                theSickCycleEnd = theSickCycleEnd.AddMonths(11);
                theSickCycleEnd = theSickCycleEnd.AddDays(29);

                if (theSickCycleEnd < DateTime.Now)
                {

                }

                return theSickCycleEnd;
            }
        }


        public virtual double SickCycleDays
        {
            get { return 30; }
        }

        public double SickDaysAvilable
        {
            get
            {
                return SickCycleDays - SickDaysTaken;
            }
        }

        public double StudyLeaveDays
        {
            get { return 16; }
        }

        public double StudyLeaveTaken
        {
            get
            {
                double days = 0;
                if (StaffLeaveData == null)
                    return days;

                const int leavetype = 3;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype)))
                {
                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                        days += 1;
                    else
                        days += leaveModel.LeaveDateStart.DifferenceInDays(leaveModel.LeaveDateEnd);
                        //days += (leaveModel.LeaveDateStart.Date - leaveModel.LeaveDateEnd).TotalDays;
                }
                return Math.Abs(days);
            }
        }

        public double FamilyResponsibilityLeaveDays
        {
            get { return 3; }
        }

        public double FamilyResponsibilityLeaveTaken
        {
            get
            {
                double days = 0;
                if (StaffLeaveData == null)
                    return days;

                const int leavetype = 4;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype)))
                {
                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                        days += 1;
                    else
                        days += (leaveModel.LeaveDateStart.Date - leaveModel.LeaveDateEnd).TotalDays;
                }
                return Math.Abs(days);
            }
        }

        public double FamilyResponsibilityAvilable
        {
            get
            {
                return FamilyResponsibilityLeaveDays - FamilyResponsibilityLeaveTaken;
            }
        }

        public double AnnualUnpaid
        {
            get { return 31; }
        }

        public double AnnualUnpaidTaken
        {
            get
            {
                double days = 0;
                if (StaffLeaveData == null)
                    return days;

                const int leavetype = 5;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype)))
                {
                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                        days += 1;
                    else
                        days += (leaveModel.LeaveDateStart.Date - leaveModel.LeaveDateEnd).TotalDays;
                }
                return Math.Abs(days);
            }
        }

        //TODO: check this again tomorrow
        //public DateTime SickCycleEnd
        //{
        //    get { return SickCycleStart.AddMonths(36); }
        //}

        #endregion


        #endregion
        public virtual bool IsDirector { get; set; }
        public virtual ICollection<StaffLeaveCounterModel> LeaveCounters { get; set; }
        public virtual decimal StaffLeaveIncrement
        {
            get
            {
                double increment = 0;
                var days = LeaveCounters;
                var leaveDaysAccum = 0;
                if (LeaveCounters == null)
                    return 0;
                StaffLeaveCounterModel counters = LeaveCounters.FirstOrDefault(x => x.RecordStatus.Equals("Active"));
                if (counters != null) increment = counters.Accumulator;
                return (decimal) increment;

                //foreach (StaffLeaveCounterModel staffLeaveCounterModel in days)
                //{
                //    leaveDaysAccum += (int)((DateTime.Now.Subtract(staffLeaveCounterModel.StartPeriod).Days / (365.25 / 12)) * staffLeaveCounterModel.Accumulator);

                //}


                //if (DateTime.Now.Year - StaffJoinDate.Year > 5)
                //{
                //    if (IsDirector)
                //    {
                //        return (decimal)2.17;
                //    }
                //    return (decimal)1.67;
                //}

                //return (decimal)1.33;
            }
        }
        public int CurrentCycleNumber
        {
            get
            {
                //if (StaffLeaveData == null)
                //    return 1;

                //var leaveData = StaffLeaveData.FirstOrDefault(m => m.LeaveType.Equals(1));
                //if (leaveData == null)
                //    return 0;

                if (DateTime.Now.Year - StaffJoinDate.Year <= 0)
                {
                    return 1;
                }
                if (DateTime.Now.Year - StaffJoinDate.Year > 0)
                {

                    return DateTime.Now.Year - StaffJoinDate.Year == 0 ? 1 : (DateTime.Now.Year - StaffJoinDate.Year + 1);
                }

                return 1;
            }
        }


        public DateTime CycleStart
        {
            get
            {
                //if (StaffLeaveData == null)
                //{
                //    return DateTime.Now;
                //}
                //var leaveData = StaffLeaveData.FirstOrDefault(m => m.LeaveType.Equals(1));
                //if (leaveData == null)
                //    return DateTime.Now;
                if (DateTime.Now.Year == StaffJoinDate.Year)
                {
                    return StaffJoinDate;
                }
                if (DateTime.Now.Month < StaffJoinDate.Month)
                {
                    return StaffJoinDate.AddYears((CurrentCycleNumber == 1 ? 0 : (CurrentCycleNumber - 1)));
                }
                var addYears = DateTime.Now.Year - StaffJoinDate.Year;
                return StaffJoinDate.AddYears(CurrentCycleNumber == 1 ? 0 : addYears);

            }
        }
        public DateTime CycleEnd
        {
            get { return CycleStart.AddMonths(11).AddDays(30); }
            //get { return CycleStart.AddMonths(11); }
        }
        public double CycleDays
        {
            get
            {
                if (LeaveSummary == null)
                    return 0;
                var leavedata = LeaveSummary.FirstOrDefault(x => x.LeaveType.Equals(1));
                if (leavedata == null)
                    return 0;

                double val = System.Convert.ToDouble(leavedata.Increment);
                return (((CycleEnd - CycleStart).Days / 30) * val).Round(0);

            }
        }

        public double LeaveDaysAccumulated
        {
            get
            {
                //if (LeaveScheduleCarriedOver != null)
                //    return (CalculateAccumulatedLeaveDays() + (double)LeaveScheduleCarriedOver).Round(2);
                return CalculateAccumulatedLeaveDays2();
                //return CalculateAccumulatedLeaveDays().Round(2);
            }
        }
        private double CalculateAccumulatedLeaveDays()
        {
            var leaveDaysAccumulated = (DateTime.Now.Subtract(StaffJoinDate).Days / (365.25 / 12)) * (double)StaffLeaveIncrement;

            return leaveDaysAccumulated.Round(2);
        }

        private double CalculateAccumulatedLeaveDays2()
        {
            var deploymentDate = new DateTime(2015, 11, 1, 0, 0, 0);
            double total = 0;
            double acu = 0;
            if (DateTime.Now.Date > deploymentDate)
            {
                acu += ((DateTime.Now.Subtract(deploymentDate).Days / (365.25 / 12)) * (double)StaffLeaveIncrement) + LeaveScheduleCarriedOver;
            }

            total = acu.Round(2);

            return total;
        }
        public double DaysDue
        {
            get
            {
               return ((LeaveDaysAccumulated) - AnnualDaysTaken).Round(2);
            }
        }
        public double AnnualDaysTaken
        {
            get
            {
                double days = 0;
                if (StaffLeaveData == null)
                    return days;

                //this is the int representation of the annual leave enum
                const int leavetype = 1;
                const int approvedLeave = 1;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype) && m.LeaveStatus.Equals(approvedLeave)))
                {
                    if (StaffIsOnLeave)
                    {

                    }

                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                        days += 1; //todo-jay: cater for half days.
                    else
                    {
                        try
                        {
                            days += 1;
                            days += leaveModel.LeaveDateStart.DifferenceInDays(leaveModel.LeaveDateEnd);
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }

                return days;
            }
        }

        public double AnnualLeaveHoursAccumulated
        {
            get { return CalculateAccumulatedLeaveHours(); }
        }
        private double CalculateAccumulatedLeaveHours()
        {


            if (this.StaffHoursData == null || !this.StaffHoursData.Any())
            {
                return 0;
            }

            var hoursOfStaff = StaffHoursData.FirstOrDefault();
            if (DateTime.Now.Hour == CycleStart.Hour)
            {
                //return 0;
            }
            double accumulatedLeaveHours = 0;

            if (hoursOfStaff != null)
                accumulatedLeaveHours = ((hoursOfStaff.DayTimeEnd - hoursOfStaff.DayTimeStart).Hours - 1) * LeaveDaysAccumulated;

            return accumulatedLeaveHours.Round(2);
        }
        public double HoursDue
        {
            //get
            //{
            //    return (AnnualLeaveHoursAccumulated - AnnualLeaveHoursTaken).Round(2);
            //}
            get
            {
                if (this.StaffHoursData == null || !this.StaffHoursData.Any())
                {
                    return 0;
                }
                var hoursOfStaff = StaffHoursData.FirstOrDefault();

                double accumulatedLeaveHours = 0;
                if (hoursOfStaff != null)
                    accumulatedLeaveHours = ((hoursOfStaff.DayTimeEnd - hoursOfStaff.DayTimeStart).Hours - 1) * DaysDue;

                return accumulatedLeaveHours.Round(2);
            }
        }
        public double AnnualLeaveHoursTaken
        {
            get
            {
                double hours = 0;

                if (StaffLeaveData == null)
                    return hours;

                //this is the int representation of the annual leave enum
                const int leavetype = 1;
                const int approvedLeave = 1;

                foreach (var leaveModel in StaffLeaveData.Where(m => m.LeaveType.Equals(leavetype) && m.LeaveStatus.Equals(approvedLeave)))
                {
                    if (StaffIsOnLeave)
                    {
                    }
                    if (leaveModel.LeaveDateStart.Date == leaveModel.LeaveDateEnd.Date)
                    {
                        if (leaveModel.LeaveDateStart.TimeOfDay != leaveModel.LeaveDateEnd.TimeOfDay)
                        {
                            TimeSpan span = leaveModel.LeaveDateEnd - leaveModel.LeaveDateStart;
                            hours += span.TotalHours;
                        }
                    }
                    else
                    {
                        if (leaveModel.LeaveDateEnd.TimeOfDay != leaveModel.LeaveDateStart.TimeOfDay)
                        {
                            TimeSpan span = leaveModel.LeaveDateEnd.TimeOfDay - leaveModel.LeaveDateStart.TimeOfDay;
                            hours += span.Hours;
                        }
                        TimeSpan dayspan = leaveModel.LeaveDateEnd - leaveModel.LeaveDateStart;
                        hours += (dayspan.TotalHours - 22);
                    }
                }

                return hours;
            }
        }
        #endregion

        #region Do not Map these Properties with EF.
        //*** Important!! ***
        // For these to be available in client side Javascript, we need the case to match
        // We also need the object extended in its constructor in JavaScript
        // More info here: http://stackoverflow.com/questions/16524073/handling-calculated-properties-with-breezejs-and-web-api
        public virtual String StaffFullName
        {
            get { return String.Format("{0} {1}", StaffName, StaffSurname); }
        }
        public DateTime StaffDob
        {
            get
            {
                if ((StaffIdNumber != null) && (!StaffIdNumber.Equals(string.Empty)))
                {
                    // Use the system setting to determine the 4 digit year. (30 = 1930 or 2030?)
                    var item = CultureInfo.CurrentCulture.Calendar;

                    //780418
                    var strYear = StaffIdNumber.Substring(0, 2); //78
                    var strMonth = StaffIdNumber.Substring(2, 2); //04
                    var strDay = StaffIdNumber.Substring(4, 2); //18
                    var dob = new DateTime(item.ToFourDigitYear(int.Parse(strYear)), int.Parse(strMonth), int.Parse(strDay));
                    return dob;
                }
                return new DateTime();
            }
        }
        public virtual DateTime StaffBirthday
        {
            get
            {
                if ((StaffIdNumber != null) && (!StaffIdNumber.Equals(string.Empty)))
                {
                    var strMonth = StaffIdNumber.Substring(2, 2); //04
                    var strDay = StaffIdNumber.Substring(4, 2); //18
                    var dob = new DateTime(DateTime.Today.Year, int.Parse(strMonth), int.Parse(strDay));
                    return dob;
                }
                return new DateTime();
            }
        }
        public virtual bool StaffIsOnLeave
        {
            get
            {
                try
                {
                    //this valriable is used avoid circular referencing
                    var approvedLeave = 1;

                    // Look for a leave day/ Range that overlaps or is equal to today.
                    var lst = StaffLeaveData.FirstOrDefault(x => (x.LeaveDateStart <= DateTime.Today) &&
                                                                 (x.LeaveDateEnd >= DateTime.Today) &&
                                                                 (x.LeaveStatus == approvedLeave));
                    return lst != null;
                }
                catch
                {
                    return false;
                }
            }
        }
        //public virtual DateTime CurrentLeaveCycle { get { return this.LeaveCounters != null && LeaveCounters.Any() ? GetCurrentLeaveCycle(StaffJoinDate) : DateTime.Now; } }

        #endregion

        public virtual bool BroadcastBirthday{ get; set; }

        #region Extended Subclasses/Entities
        // Parent
        public virtual DivisionModel StaffDivision { get; set; }

        // Child Entities
        public virtual StaffModel StaffManager1 { get; set; }
        public virtual StaffModel StaffManager2 { get; set; }
        public virtual ICollection<StaffClockModel> StaffClockData { get; set; }
        public virtual ICollection<StaffHoursModel> StaffHoursData { get; set; }
        public virtual ICollection<StaffLeaveModel> StaffLeaveData { get; set; }
        public virtual ICollection<StaffContactModel> StaffContactData { get; set; }
        public virtual ICollection<SuggestionModel> Suggestions { get; set; }

        // Printer
        public virtual Guid? StaffDefaultPrinter { get; set; }
        public virtual ICollection<TonerOrdersModel> TonerOrders { get; set; }

        //Call records
        public double TotalCallCost { get; private set; }
        public ICollection<StaffPhoneRecord> StaffCallRecords { get; set; }
        public string DisplayTotalCallCost
        {
            get
            {
                TotalCallCost = StaffCallRecords != null ? (from m in StaffCallRecords select m.CallCost).Sum() : 0;
                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }


        #endregion
    }

    public class StaffClockModel : BaseModel
    {

        //public virtual int PeStaffClockId { get; set; }
        public virtual int ClockDataId { get; set; }
        public virtual Guid StaffId { get; set; }
        public virtual DateTime OriginalClockDateTime { get; set; }
        public virtual DateTime ClockDateTime { get; set; }
        public virtual DateTime PrevClockDateTime { get; set; }
        public virtual int DataStatus { get; set; }
        public int DataStatusHighlight { get; set; }
        public virtual string Comments { get; set; }

        public virtual StaffModel Staff { get; set; }



        //NB Do not do ORM mapping
        #region Data manipulation properties.
        public bool IsLeaveRecord { get; set; }

        public int? LeaveType { get; set; }
        public string Email { get; set; }
        public bool? IsPublicHoldiday { get; set; }

        #endregion

        #region Constructor
        public StaffClockModel()
        {
            // Nothing for now
        }

        // Constructor - Override
        //TODO: Ref Jay: Are the constructor overloads being used?
        public StaffClockModel(Guid staffId, DateTime clockDateTime)
        {
            StaffId = staffId;
            ClockDateTime = clockDateTime;
            OriginalClockDateTime = clockDateTime;
            RecordStatus = "Active"; //TODO Need to make a Enum here
        }

        public StaffClockModel(Guid staffId, DateTime clockDateTime, bool isLeaveRecord = false)
            : this(staffId, clockDateTime)
        {
            RecordStatus = "Active"; //TODO Need to make a Enum here
            // IsLeaveRecord = isLeaveRecord;
        }

        public StaffClockModel(Guid staffId, DateTime clockDateTime, int? leaveType)
            : this(staffId, clockDateTime, leaveType.HasValue)
        {
            // LeaveType = leaveType;
        }
        #endregion

        #region Overrides
        //TODO: Ref Jay: Are these being used?
        public override bool Equals(object obj)
        {
            var input = (StaffClockModel)obj;
            bool isEqual = (StaffId == input.StaffId) && (ClockDateTime == input.ClockDateTime);
            //            #region debug code
            //#if ClockData
            //                Trace.WriteLine("--- input / comparer ---");
            //                Trace.WriteLine(input.ToString());
            //                Trace.WriteLine(this.ToString());
            //                Trace.WriteLine(isEqual);
            //#endif
            //            #endregion
            return isEqual;
            //          return true;
        }
        public override string ToString()
        {
            return string.Format("{0} - {1} - {2}", StaffId, ClockDateTime.ToString("dd/MM/yyyy HH:mm:ss"), RecordStatus);
        }
        public override int GetHashCode()
        {
            return (string.Format("{0} | {1}", StaffId, ClockDateTime.ToString("dd/MM/yyyy HH:mm:ss"))).GetHashCode();
        }
        #endregion
    }

    public class WeekClockInSummary
    {
        public String Name { get; set; }
        public Double TimeWorked { get; set; }
        public Double TimeDebt { get; set; }
        public Double Overtime { get; set; }
        public Double RequiredHours { get; set; }
        public Double OnleaveHours { get; set; }

    }

    public class StaffHoursModel : BaseModel
    {
        public virtual Guid StaffId { get; set; }
        public virtual int DayId { get; set; }
        public virtual DateTime DayTimeStart { get; set; }
        public virtual DateTime DayTimeEnd { get; set; }
        public virtual int DayLunchLength { get; set; }
        public virtual double DayHoursRequired
        {
            get
            {
                TimeSpan? variable = DayTimeEnd - DayTimeStart;
                return variable.Value.TotalMinutes;
                //convert the minutes to hours
                //return DayTimeEnd.Hour - DayTimeStart.Hour; //This did not factor in minutes
            }
        }
        public virtual double WeekHoursRequired
        {
            get
            {
                TimeSpan? variable = DayTimeEnd - DayTimeStart;
                return variable.Value.TotalMinutes * 5;
                //convert the minutes to hours
                //return DayTimeEnd.Hour - DayTimeStart.Hour; //This did not factor in minutes
            }
        }

        public virtual StaffModel Staff { get; set; }

        #region Overrides
        public override bool Equals(object obj)
        {
            var input = (StaffHoursModel)obj;
            bool isEqual = (DayId == input.DayId) && (StaffId == input.StaffId);
            return isEqual;
        }
        public override int GetHashCode()
        {
            return (string.Format("{0} | {1}", StaffId, DayId.ToString(CultureInfo.InvariantCulture))).GetHashCode();
        }
        #endregion
    }
    public class StaffLeaveModel : BaseModel
    {


        public virtual Guid StaffId { get; set; }
        public virtual Guid LeaveId { get; set; }
        public virtual DateTime LeaveDateStart { get; set; }
        public virtual DateTime LeaveDateEnd { get; set; }
        public virtual String LeaveComments { get; set; }
        public virtual int LeaveType { get; set; }
        public virtual int LeaveStatus { get; set; }
        public virtual DateTime LeaveRequestDate { get; set; }
        public virtual Guid? ApprovedBy1 { get; set; }
        public virtual Guid? ApprovedBy2 { get; set; }

        public virtual StaffModel StaffMember { get; set; }
        public virtual String ReasonForAction { get; set; }

        public IEnumerable<string> PublicHolidays 
        {
            get
            {
                var value = DateTime.Now;
                var publicHols = new List<DateTime>
                {
                    new DateTime(value.Year, 1, 1), //New Years
                    new DateTime(value.Year, 3, 21), //Human rights day
                    new DateTime(value.Year, 3, 25),
                    new DateTime(value.Year, 3, 28), //Public holiday
                    new DateTime(value.Year, 4, 27), //Good friday
                    //new DateTime(value.Year, 5, 1), //Workers day
                    new DateTime(value.Year, 5, 2), //Workers day
                    new DateTime(value.Year, 6, 16), //Youth day
                    new DateTime(value.Year, 8, 8), //Heritage day
                    new DateTime(value.Year, 9, 24), //Heritage day
                    new DateTime(value.Year, 12, 16), //day of reconciliation
                    new DateTime(value.Year, 12, 25), //Day of Goodwill//Christmas day
                    new DateTime(value.Year, 12, 26), //Day of Goodwill
                };

                return publicHols.Select(x => x.ToShortDateString());
            }
        }
        //public int LeaveApplicationCycle
        //{
        //    get
        //    {
        //         var cycle = DateTime.Now.Year - StaffMember.StaffJoinDate.Year == 0 ? 1 : (DateTime.Now.Year - StaffMember.StaffJoinDate.Year);
        //        var currentLeaveApp = DateTime.Now.Year - LeaveDateStart.Year
        //        return 0;
        //    }
        //}

        #region Leave Templates
        public static StaffLeaveModel ForcedLeave(Guid authorisedBy)
        {
 

            //TODO - Jay need a better solution to avoid circualr referencing
            // Create Leave day template
            return new StaffLeaveModel
            {
                LeaveId = Guid.NewGuid(),
                LeaveRequestDate = DateTime.Now,
                LeaveComments = "Annual leave that was automatically added as a result of not clocking in by  ",
                LeaveStatus = (int)Enums.LeaveStatus.Approved,
                LeaveType = (int)Enums.LeaveType.Annual,
                RecordStatus = "Active", //TODO: Need to make a enum here,
                 
                //ApprovedBy1      = //TODO: Need solution here
                //ApprovedBy2      = //TODO: Need solution here
            };
        }
        #endregion
    }
    public class StaffLeaveCounterModel : BaseModel
    {
        public virtual Guid RecordId { get; set; }
        public virtual Guid StaffId { get; set; }
        public virtual DateTime StartPeriod { get; set; }
        public virtual DateTime EndPeriod { get; set; }
        public virtual double Accumulator { get; set; }

        public virtual StaffModel Staff { get; set; }
    }
    public class StaffContactModel : BaseModel
    {
        public virtual Guid StaffId { get; set; }
        public virtual Guid ContactId { get; set; }
        public virtual String ContactName { get; set; }
        public virtual String ContactSurname { get; set; }
        public virtual String ContactNumber { get; set; }
        public virtual String ContactDescription { get; set; }

        public virtual String ContactFullName
        {
            get { return String.Concat(ContactName, String.IsNullOrEmpty(ContactSurname) ? "" : " " + ContactSurname); }
        }

        public virtual StaffModel StaffMember { get; set; }
    }
    public class StaffPhoneDetailModel : BaseModel
    {
        public virtual Guid StaffId { get; set; }

        public virtual string StaffPhoneRinger { get; set; }
        public virtual string StaffPhoneDomain { get; set; }
        public virtual string StaffPhoneNetMask { get; set; }
        public virtual string StaffPhoneGateway { get; set; }
        public virtual string StaffPhoneDNS1 { get; set; }
        public virtual string StaffPhoneDNS2 { get; set; }
        public virtual string StaffPhoneHost { get; set; }
        public virtual string StaffPhoneOutBound { get; set; }
        public virtual string StaffPhoneIp { get; set; }
        public virtual string StaffPhonePass { get; set; }
        public virtual string StaffPhoneMac { get; set; }

        //public virtual Guid DetailId
        //{
        //    get { return Guid.NewGuid(); }
        //}

        public virtual StaffModel StaffMember { get; set; }

    }

    public enum PhoneStatus
    {
        [Description("Available")]
        Onhook = 0,

        [Description("Busy")]
        Offhook = 1
    }

    public class StaffLeaveSummaryModel : BaseModel
    {
        public virtual Guid RecId { get; set; }
        public virtual Guid StaffId { get; set; }
        public virtual Decimal Increment { get; set; }
        public virtual DateTime StaffJoinDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual int LeaveType { get; set; }
        public virtual int DaysAvailable { get; set; }
        public virtual int DaysTaken { get; set; }
        public virtual StaffModel StaffMember { get; set; }
    }

    public class StaffLeaveSummary : BaseModel
    {
        public virtual Guid RecId { get; set; }
        public virtual Guid StaffId { get; set; }
        public virtual Decimal Increment { get; set; }
        public virtual DateTime StaffJoinDate { get; set; }
        public virtual DateTime EndDate { get; set; }
        public virtual int LeaveType { get; set; }
        public virtual int DaysAvailable { get; set; }
        public virtual int DaysTaken { get; set; }
        public virtual StaffModel StaffMember { get; set; }

    }

    public class StaffClockingDataExport
    {
        public string Markup { get; set; }
    }

    public class LeaveDetails
    {
        public int Year { get; set; }
        public LeaveType LeaveType { get; set; }
        public double LeaveAllocation { get; set; }
        public double LeaveTaken { get; set; }
        public double LeaveCarriedOver { get; set; }
        public double RunningTotal { get; set; }

    }

    public class Prop
    {
        public int Year { get; set; }
        public List<LeaveDetails> Data { get; set; }
    }

    public class StaffLeave
    {
        public StaffModel StaffMember { get; set; }
        public double LeaveIncreement { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}