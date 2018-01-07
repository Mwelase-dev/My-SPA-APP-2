using FluentNHibernate.Mapping;
using Intranet.Models;

namespace Intranet.Data.NH.Mappings
{
    public class SuggestionMap : ClassMap<SuggestionModel>
    {
        public SuggestionMap()
        {
            Table("tblStaffSuggestions");
            Id(x => x.SuggestionId      , "SuggestionID"     );
            Map(x => x.SuggestionSubject, "suggestionSubject");
            Map(x => x.RecordStatus     , "RecordStatus"     );

            //HasOne(x => x.StaffMember);
        }
    }
}