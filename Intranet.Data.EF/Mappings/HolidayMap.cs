using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class HolidayMap : EntityTypeConfiguration<HolidayModel>
    {
        public HolidayMap()
        {
            ToTable("tblHolidays");
            HasKey(x => x.HolidayId);
            Property(x => x.HolidayId         ).HasColumnName("HolidayID"         );
            Property(x => x.HolidayDate       ).HasColumnName("HolidayDate"       );
            Property(x => x.HolidayDescription).HasColumnName("HolidayDescription");
            Property(x => x.RecordStatus      ).HasColumnName("RecordStatus"      );
        }
    }
}