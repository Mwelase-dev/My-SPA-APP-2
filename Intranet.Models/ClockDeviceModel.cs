using System;
using System.Collections.Generic;

namespace Intranet.Models
{
    public class ClockDeviceModel : BaseModel
    {
        public virtual Guid ClockDeviceId { get; set; }
        public virtual string ClockDeviceLocation { get; set; }
        public virtual string ClockDeviceIp { get; set; }
        public virtual int DeviceNumber { get; set; }
    }
}
