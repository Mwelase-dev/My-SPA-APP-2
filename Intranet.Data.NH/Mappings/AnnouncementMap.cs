using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class AnnouncementMap : ClassMap<AnnouncementModel>
    {
        public AnnouncementMap()
        {
            Table("tblAnnouncements");
            Id(x => x.AnnouncementId      , "AnnouncementID"     );
            Map(x => x.AnnouncementSubject, "AnnouncementSubject");
            Map(x => x.Announcement       , "Announcement"       );
            Map(x => x.AnnouncementAuthor , "AnnouncementAuthor" );
            Map(x => x.AnnouncementDate   , "AnnouncementDate"   );
            Map(x => x.RecordStatus       , "RecordStatus"       );
        }
    }
}