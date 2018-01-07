using System;
using System.Collections.Generic;

namespace Intranet.Models
{
    public class LinkCategoryModel : BaseModel
    {
        public virtual Guid   CategoryId    { get; set; }
        public virtual String CategoryDesc  { get; set; }
        public virtual int    CategoryOrder { get; set; }

        // Related Entities
        public virtual IList<LinkModel> CategoryLinks { get; set; }
    }
}
