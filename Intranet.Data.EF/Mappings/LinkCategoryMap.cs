using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class LinkCategoryMap : EntityTypeConfiguration<LinkCategoryModel>
    {
        public LinkCategoryMap()
        {
            ToTable("tblLinkCategories");
            HasKey(x => x.CategoryId);
            Property(x => x.CategoryId   ).HasColumnName("CategoryID"   );
            Property(x => x.CategoryDesc ).HasColumnName("CategoryDesc" );
            Property(x => x.CategoryOrder).HasColumnName("CategoryOrder");
            Property(x => x.RecordStatus ).HasColumnName("RecordStatus" );
            
            // Children
            HasMany(x => x.CategoryLinks); //.WithOptional();//.HasForeignKey(x => x.CategoryId);
        }
    }
}