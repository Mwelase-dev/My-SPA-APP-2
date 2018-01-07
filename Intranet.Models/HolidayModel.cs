using System;

namespace Intranet.Models
{
    public class HolidayModel : BaseModel
    {
        public virtual Guid     HolidayId          { get; set; }
        public virtual DateTime HolidayDate        { get; set; } // TODO: Need to build the holiday date if it is annual. Just not sure how NHibernate will react here?
        public virtual String   HolidayDescription { get; set; }
        public virtual String   HolidayIsAnnual    { get; set; }
    }
}