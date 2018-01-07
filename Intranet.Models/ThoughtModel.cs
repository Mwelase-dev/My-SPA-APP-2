using System;

namespace Intranet.Models
{
    public class ThoughtModel : BaseModel
    {
        public virtual Guid   ThoughtId     { get; set; }
        public virtual String Thought       { get; set; }
        public virtual String ThoughtAuthor { get; set; }
    }
}