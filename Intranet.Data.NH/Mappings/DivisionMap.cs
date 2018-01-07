using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class DivisionMap : ClassMap<DivisionModel>
    {
        public DivisionMap()
        {
            Table("tblBranchDivisions");
            Id(x => x.DivisionId    , "DivisionID"      );
            Map(x => x.BranchId     , "DivisionBranchID");
            Map(x => x.DivisionName , "DivisionName"    );
            Map(x => x.RecordStatus , "RecordStatus"    );

            //parent
            HasOne(x => x.DivisionBranch);
            //References(x => x.DivisionBranch);//.ForeignKey("DivisionBranchID");

            //Child Relationship
            //HasMany(x => x.DivisionStaff).KeyColumns.Add("DivisionID");//.Not.LazyLoad().OrderBy("StaffSurname");
        }
    }
}