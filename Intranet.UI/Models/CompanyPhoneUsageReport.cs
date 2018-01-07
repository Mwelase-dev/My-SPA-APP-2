using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Web;
using Intranet.Models;

namespace Intranet.UI.Models
{
    public class CompanyReportModel
    {
        //Phone records
        public double TotalCallCost { get; set; }
        public string DisplayTotalCallCost
        {
            get
            {
                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }

        public string CompanyName { get; set; }
        public Guid  CompanyId { get; set; }
        public List<DivisionReportModel> CompanyDivisions { get; set; }

        public CompanyReportModel()
        {
            CompanyDivisions = new List<DivisionReportModel>();
        }
        public CompanyReportModel(Guid compId, string compayName,IEnumerable<DivisionModel> branchDivisions):this()
        {
            CompanyId = compId;
            CompanyName = compayName;

            CompanyDivisions =
                branchDivisions.ToList().ConvertAll(
                    m => new DivisionReportModel(m.DivisionId, m.DivisionName, m.BranchId, m.DivisionStaff));

            TotalCallCost = CompanyDivisions.Select(m => m.TotalCallCost).Sum();
        }

    }

    public class DivisionReportModel
    {
        //Phone records
        public double TotalCallCost { get; private set; }
        public string DisplayTotalCallCost
        {
            get
            {
                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }

        public Guid DivisionId { get; set; }
        public string DivisionName { get; set; }
        public Guid BranchId { get; set; }
        public List<StaffReportModel> DivisionStaff { get; set; }
        public StaffModel StaffMember { get; set; }

        public DivisionReportModel()
        {
            DivisionStaff = new List<StaffReportModel>();
        }
        public DivisionReportModel(Guid divisionId,string divisionName,Guid branchId,IEnumerable<StaffModel> branchStaff ):this()
        {
            DivisionId = divisionId;
            DivisionName = divisionName;
            BranchId = branchId;

            DivisionStaff = branchStaff.ToList().ConvertAll(m => new StaffReportModel(
                                                            m.StaffId, m.StaffFullName, m.DivisionId, m.StaffCallRecords));

            //StaffMember = branchStaff.
            TotalCallCost = DivisionStaff.Select(m => m.TotalCallCost).Sum();
        }
    }

    public class StaffReportModel
    {
        public Guid StaffId { get; set; }
        public string Fullname { get; set; }
        public Guid DivisionId { get; set; }
        public double TotalCallCost { get; private set; }
        public double TotalDaysOnLeave { get; private set; }
        public ICollection<StaffPhoneRecord> StaffCallRecords { get; set; }
        public string DisplayTotalCallCost
        {
            get
            {
                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }



        public StaffReportModel()
        {
            StaffCallRecords = new Collection<StaffPhoneRecord>();
        }
        public StaffReportModel(Guid staffId,string fullName,Guid divisionId,ICollection<StaffPhoneRecord> callRecords ):this()
        {
            StaffId = staffId;
            Fullname = fullName;
            StaffCallRecords = callRecords;
            DivisionId = divisionId;
            TotalCallCost = callRecords.Select(m => m.CallCost).Sum();
        }
 

    }
}