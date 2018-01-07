using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class ClockDeviceMapping : EntityTypeConfiguration<ClockDeviceModel>
    {
        public ClockDeviceMapping()
        {
            ToTable("tblClockDevices");
            HasKey(x => x.ClockDeviceId);
            Property(x => x.ClockDeviceLocation).HasColumnName("ClockDeviceLocation");
            Property(x => x.ClockDeviceIp).HasColumnName("ClockDeviceIp");
            Property(x => x.DeviceNumber).HasColumnName("DeviceNumber");

        }
    }
}
