using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class LinkMap : EntityTypeConfiguration<LinkModel>
    {
        public LinkMap()
        {
            ToTable("tblLinks");
            HasKey(x => x.LinkId);
            Property(x => x.LinkId      ).HasColumnName("LinkID"      );
            Property(x => x.CategoryId  ).HasColumnName("CategoryID"  );
            Property(x => x.LinkDesc    ).HasColumnName("LinkDesc"    );
            Property(x => x.LinkUrl     ).HasColumnName("LinkURL"     );
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");

            // For getting to the parent.
            HasRequired(x => x.LinkCategory);
        }
    }
}