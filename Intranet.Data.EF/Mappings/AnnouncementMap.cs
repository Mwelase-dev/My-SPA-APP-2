using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class AnnouncementMap : EntityTypeConfiguration<AnnouncementModel>
    {
        public AnnouncementMap()
        {
            ToTable("tblAnnouncements");
            HasKey(x => x.AnnouncementId);
            Property(x => x.AnnouncementId     ).HasColumnName("AnnouncementID"     );
            Property(x => x.AnnouncementSubject).HasColumnName("AnnouncementSubject");
            Property(x => x.Announcement       ).HasColumnName("Announcement"       );
            Property(x => x.AnnouncementAuthor ).HasColumnName("AnnouncementAuthor" );
            Property(x => x.AnnouncementDate   ).HasColumnName("AnnouncementDate"   );
            Property(x => x.RecordStatus       ).HasColumnName("RecordStatus"       );
        }
    }
}