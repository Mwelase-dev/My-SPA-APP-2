using System.ComponentModel;

namespace Intranet.Models.Enums
{
    public enum RecordStatusEnum
    {
        [Description("Invalid")]
        Default = 0,
        [Description("Active")]
        Active = 1,
        [Description("Deleted")]
        Deleted = 2,
    }
}
