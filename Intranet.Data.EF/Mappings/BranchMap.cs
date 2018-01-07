using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class BranchMap : EntityTypeConfiguration<BranchModel>
    {
        public BranchMap()
        {
            ToTable("tblBranches");
            HasKey(x => x.BranchId);
            Property(x => x.BranchId       ).HasColumnName("BranchID"       );
            Property(x => x.BranchName     ).HasColumnName("BranchName"     );
            Property(x => x.BranchShortName).HasColumnName("BranchShortName");

            HasMany(x => x.BranchDivisions);

            Ignore(x => x.StaffCallRecords);
             Ignore(x => x.DisplayTotalCallCost);
        }
    }
}