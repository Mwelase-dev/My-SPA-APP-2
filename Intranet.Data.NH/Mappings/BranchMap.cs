using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class BranchMap : ClassMap<BranchModel>
    {
        public BranchMap()
        {
            Table("tblBranches");
            Id(x => x.BranchId        , "BranchID"       );
            Map(x => x.BranchName     , "BranchName"     );
            Map(x => x.BranchShortName, "BranchShortName");
            Map(x => x.RecordStatus   , "RecordStatus"   );

            //Child Relationship
            HasMany(x => x.BranchDivisions).KeyColumn("DivisionBranchID");//.KeyColumn("DivisionBranchID");//.Not.LazyLoad().OrderBy("DivisionName");
        }
    }
}