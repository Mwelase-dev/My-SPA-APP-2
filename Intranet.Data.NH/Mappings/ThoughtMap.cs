using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class ThoughtMap : ClassMap<ThoughtModel>
    {
        public ThoughtMap()
        {
            Table("tblThoughts");
            Id(x => x.ThoughtId     , "ThoughtID"    );
            Map(x => x.Thought      , "Thought"      );
            Map(x => x.ThoughtAuthor, "ThoughtAuthor");
            Map(x => x.RecordStatus , "RecordStatus" );
        }
    }
}