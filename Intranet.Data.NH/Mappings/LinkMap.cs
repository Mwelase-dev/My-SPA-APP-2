using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class LinkMap : ClassMap<LinkModel>
    {
        public LinkMap()
        {
            Table("tblLinks");
            Id(x => x.LinkId       , "LinkID"      );
            Map(x => x.CategoryId  , "CategoryID"  );
            Map(x => x.LinkDesc    , "LinkDesc"    );
            Map(x => x.LinkUrl     , "LinkURL"     );
            Map(x => x.RecordStatus, "RecordStatus");

            //Parent Relationship
            References(x => x.LinkCategory, "CategoryID").Not.LazyLoad();//.Cascade. .Cascade .None().Not.LazyLoad();
        }
    }
}