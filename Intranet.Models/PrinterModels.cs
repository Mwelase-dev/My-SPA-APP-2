using System;
using System.Collections.Generic;

namespace Intranet.Models
{
    public class PrinterModel : BaseModel
    {
        public virtual string PrinterMake { get; set; }
        public virtual string Model { get; set; }
        public virtual string Location { get; set; }
       


        public Guid PrinterId { get; set; }
        public string SerialNumber { get; set; }
        public Guid ProviderId { get; set; }
        public virtual PrinterServiceProviderModel PrinterProvider { get; set; }
        public virtual ICollection<PrinterPropertiesPrinterModel> Properties { get; set; }
        public string PrinterDescription { get; set; }
        // public virtual ICollection<StaffModel> StaffModels { get; set; }
         
    }

    public sealed class PrinterServiceProviderModel : BaseModel
    {
        public PrinterServiceProviderModel()
        {
            Printers = new LinkedList<PrinterModel>();
        }
     

        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
        public string ProviderEmail { get; set; }

        public ICollection<PrinterModel> Printers { get; set; }

    }

    public class PrinterPropertyModel : BaseModel
    {
        public virtual Guid PropertyId { get; set; }
        public virtual string PropertyDescription { get; set; }
 

        public virtual ICollection<PrinterPropertiesPrinterModel> PrinterProperties { get; set; }
    }

    public class TonerOrdersModel : BaseModel
    {
        public Guid OrderId { get; set; }
        public Guid PrinterId { get; set; }
        public Guid StaffId { get; set; }
        public DateTime OrderDate { get; set; }
        public virtual PrinterModel Printer { get; set; }
        public virtual StaffModel Staff { get; set; }
        public virtual int OrderStatus { get; set; }
 

        public virtual ICollection<TonerOrderDetailsModel> OrderDetails { get; set; }

    }

    public class TonerOrderDetailsModel : BaseModel
    {
        public Guid DetailsId { get; set; }
     
        public Guid OrderId { get; set; }
        public Guid PropertyId { get; set; }
        public Guid PrinterId { get; set; }

        public virtual TonerOrdersModel TonerOrder { get; set; }
        public virtual PrinterPropertiesPrinterModel PrinterProperty { get; set; }

        public int OrderStatus { get; set; }
        public virtual int TonerOrderReminder { get; set; }
        public DateTime? LastTonerOrderReminder { get; set; }
    }

    //This is a linking table
    public class PrinterPropertiesPrinterModel : BaseModel
    {

        public virtual Guid PrinterId { get; set; }
        public virtual Guid PropertyId { get; set; }

        //related entities
        public  PrinterModel Printer { get; set; }
       public  PrinterPropertyModel Property { get; set; }

        //public virtual List<TonerOrderDetailsModel> OrderDetails { get; set; } 
    }

    public class PrinterPropertiesProperties : BaseModel
    {
        public virtual Guid PrinterId { get; set; }
        public virtual Guid PropertyId { get; set; }
    }
}
