//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Intranet.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Link : BaseModel
    {
        public System.Guid LinkCategory { get; set; }
        public System.Guid LinkCategoryRecordID { get; set; }
    
        public virtual LinkCategory LinkCategory1 { get; set; }
    }
}