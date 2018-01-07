using System.ComponentModel;

namespace Intranet.Models.Enums
{
    public enum LeaveType
    {
        Invalid = 0,
        [Description("Annual_Paid")]
        Annual = 1,
        [Description("Sick")]
        Sick = 2,
        [Description("Study")]
        Study = 3,
        [Description("Family_Responsibility")]
        Family = 4,
        [Description("Annual_Unpaid")]
        AnnualUnpaid = 5,
        [Description("Working_OffSite")]
        OffSite = 6,
        [Description("Special")]
        Special = 7
    }

    public enum LeaveStatus
    {
        Invalid = 0,
        [Description("Approved")]
        Approved = 1,
        [Description("Pending")]
        Pending = 2,
        [Description("Declined")]
        Declined = 3,
        [Description("Cancelled")]
        Cancelled = 4,
    }
}
