using System.ComponentModel;

namespace Intranet.Models.Enums
{
    public enum MessagesEnum
    {
        [Description("Leave Application")]
        LeaveApplication = 1,
        [Description("Leave Application Denied")]
        LeaveApplicationDeclined = 2,
        [Description("Leave Application approved")]
        LeaveApplicationApproved = 3,
        [Description("Leave Applciation Cancelled")]
        LEaveApplicationCancelled = 4,

        [Description("Weekly clocking/leave")]
        WeeklyLeaveClockEmail = 5,
    }
}
