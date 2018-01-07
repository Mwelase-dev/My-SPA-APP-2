using System;
using System.ComponentModel.DataAnnotations;

namespace Intranet.Models
{
    public class AnnouncementModel : BaseModel
    {
        public virtual Guid     AnnouncementId      { get; set; }
        [Required(AllowEmptyStrings = false,ErrorMessage = "Please enter a valid subject")]
        public virtual string   AnnouncementSubject { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a valid Announcement")]
        public virtual string   Announcement        { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your Name")]
        public virtual string   AnnouncementAuthor  { get; set; }
        public virtual DateTime AnnouncementDate    { get; set; }
    }
}