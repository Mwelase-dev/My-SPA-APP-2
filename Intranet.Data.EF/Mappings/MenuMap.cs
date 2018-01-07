using System.Data.Entity.ModelConfiguration;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
    public class MenuMap : EntityTypeConfiguration<MenuModel>
    {
        public MenuMap()
        {
            ToTable("tblMenuItems");
            HasKey(x => x.MenuId);
            Property(x => x.MenuId      ).HasColumnName("MenuID"      );
            Property(x => x.MenuName    ).HasColumnName("MenuName"    );
            Property(x => x.MenuTemplate).HasColumnName("MenuTemplate");
            Property(x => x.MenuOrder   ).HasColumnName("MenuOrder"   );
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
        }
    }
}