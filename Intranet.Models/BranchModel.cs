using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Intranet.Models
{
    public sealed class BranchModel : BaseModel
    {
        public Guid BranchId { get; set; }
        public String BranchName { get; set; }
        public String BranchShortName { get; set; }

        public BranchModel()
        {
            StaffCallRecords = new List<StaffPhoneRecord>();
            BranchDivisions = new List<DivisionModel>();
            //StaffLeaveRecords = new List<StaffLeaveModel>();
        }

        //Related entities
        public IList<DivisionModel> BranchDivisions { get; set; }

        //Phone records
        public IList<StaffPhoneRecord> StaffCallRecords { get; set; }

        //Leave records
        //public IList<StaffLeaveModel> StaffLeaveRecords { get; set; }

        private double TotalCallCost { get; set; }
        public string DisplayTotalCallCost
        {
            get
            {
                foreach (var staff in BranchDivisions.SelectMany(division => division.DivisionStaff).Where(staff => staff != null))
                {
                    TotalCallCost += staff.TotalCallCost;
                }

                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }

    }
}