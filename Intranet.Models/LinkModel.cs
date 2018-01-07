using System;

namespace Intranet.Models
{
    public class LinkModel : BaseModel
    {
        public virtual Guid   LinkId     { get; set; }
        public virtual Guid   CategoryId { get; set; }
        public virtual String LinkDesc   { get; set; }
        public virtual String LinkUrl    { get; set; }
        
        // Parent
        public virtual LinkCategoryModel LinkCategory { get; set; }
    }
}