using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class MenuMap : ClassMap<MenuModel>
    {
        public MenuMap()
        {
            Table("tblMenuItems");
            Id(x => x.MenuId       , "MenuID"      );
            Map(x => x.MenuName    , "MenuName"    );
            Map(x => x.MenuTemplate, "MenuTemplate");
            Map(x => x.MenuOrder   , "MenuOrder"   );
            Map(x => x.RecordStatus, "RecordStatus");
        }
    }
}