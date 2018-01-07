using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class HolidayMap : ClassMap<HolidayModel>
    {
        public HolidayMap()
        {
            Table("tblHolidays");
            Id(x => x.HolidayId          , "HolidayID"         );
            Map(x => x.HolidayDate       , "HolidayDate"       );
            Map(x => x.HolidayDescription, "HolidayDescription");
            Map(x => x.RecordStatus      , "RecordStatus"      );
        }
    }
}