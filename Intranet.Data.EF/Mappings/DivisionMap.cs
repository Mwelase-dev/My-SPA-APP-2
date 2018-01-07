using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class DivisionMap : EntityTypeConfiguration<DivisionModel>
    {
        public DivisionMap()
        {
            ToTable("tblBranchDivisions");

            HasKey(x => x.DivisionId);
            Property(x => x.BranchId     ).HasColumnName("DivisionBranchID");
            Property(x => x.DivisionName ).HasColumnName("DivisionName"    );
            Property(x => x.RecordStatus ).HasColumnName("RecordStatus"    );

            // Parent
            HasRequired(x => x.DivisionBranch);

            // Related Entities
            HasMany(x => x.DivisionStaff);

             Ignore(x => x.TotalCallCost);
            Ignore(x => x.DisplayTotalCallCost);
        }
    }
}