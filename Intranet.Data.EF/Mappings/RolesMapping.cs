using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class RolesModelMapping : EntityTypeConfiguration<RolesModel>
    {
        public RolesModelMapping()
        {
            ToTable("tblRoles");
            HasKey(x => x.RoleId);
            Property(x => x.RoleId).HasColumnName("RoleId");

            Property(x => x.Role).HasColumnName("Role");
            Property(x => x.Description).HasColumnName("Description");

            HasMany(x => x.UserRoles)
                .WithRequired(x => x.Role)
                .HasForeignKey(x => x.RoleId);
        }
    }

    public class UserRolesModelMapping : EntityTypeConfiguration<UserRolesModel>
    {
        public UserRolesModelMapping()
        {
            ToTable("tblUserRoles");
            HasKey(x => new { x.StaffId, x.RoleId });

            Property(x => x.RoleId).HasColumnName("RoleId");
            Property(x => x.StaffId).HasColumnName("StaffId");

          //  HasRequired(m => m.StaffRoleModel)
          //     .WithMany(x => x.Roles)
           //    .HasForeignKey(y => y.StaffRoleModel);
        }
    }
}
