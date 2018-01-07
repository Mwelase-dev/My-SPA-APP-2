using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Intranet.Models
{
    public class DivisionModel : BaseModel
    {
        public virtual Guid BranchId { get; set; }
        public virtual Guid DivisionId { get; set; }
        public virtual String DivisionName { get; set; }

        //Related entities
        public virtual BranchModel DivisionBranch { get; set; }
        public virtual IList<StaffModel> DivisionStaff { get; set; }

        //Phone records
        public double TotalCallCost { get; private set; }

        public string DisplayTotalCallCost
        {
            get
            {
                if (DivisionStaff == null)
                    return String.Empty;

                foreach (var staffModel in DivisionStaff)
                {
                    TotalCallCost += staffModel.TotalCallCost;
                }
                return TotalCallCost.ToString("C", CultureInfo.CurrentCulture);
            }
        }

        public DivisionModel()
        {
            DivisionStaff = new List<StaffModel>();
        }
    }
}
