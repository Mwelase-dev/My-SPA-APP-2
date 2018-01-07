using System;

namespace Intranet.Models
{
    public class MenuModel : BaseModel
    {
        public virtual Guid   MenuId       { get; set; }
        public virtual String MenuName     { get; set; }
        public virtual String MenuTemplate { get; set; }
        public virtual int    MenuOrder    { get; set; }
    }
}