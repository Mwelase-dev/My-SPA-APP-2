using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class LinkCategoryMap : ClassMap<LinkCategoryModel>
    {
        public LinkCategoryMap()
        {
            Table("tblLinkCategories");
            Id(x => x.CategoryId    , "CategoryID"   );
            Map(x => x.CategoryDesc , "CategoryDesc" );
            Map(x => x.CategoryOrder, "CategoryOrder");
            Map(x => x.RecordStatus , "RecordStatus" );

            //Child Relationship
            HasMany(x => x.CategoryLinks).KeyColumn("CategoryID").Not.LazyLoad();
                //.Inverse()
                //.Cascade.All();
        }
    }
}