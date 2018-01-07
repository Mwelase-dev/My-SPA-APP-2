using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class ThoughtMap : EntityTypeConfiguration<ThoughtModel>
    {
        public ThoughtMap()
        {
            ToTable("tblThoughts");
            HasKey(x => x.ThoughtId);
            Property(x => x.ThoughtId    ).HasColumnName("ThoughtID"    );
            Property(x => x.Thought      ).HasColumnName("Thought"      );
            Property(x => x.ThoughtAuthor).HasColumnName("ThoughtAuthor");
            Property(x => x.RecordStatus ).HasColumnName("RecordStatus" );
        }
    }
}