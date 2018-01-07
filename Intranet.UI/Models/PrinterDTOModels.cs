using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Intranet.UI.Models
{
    public class PropertiesDto
    {
        public Guid ColourId { get; set; }
        public string Color { get; set; }
        public bool Order { get; set; }
        public bool CanBeOrdered { get { return Order == false; } }

        public PropertiesDto(Guid colourId, string colour, bool ordered = false)
        {
            ColourId = colourId;
            Color = colour;
            Order = ordered;
        }
    }

    public class TonerOrdersDto
    {
        public Guid OrderDetailsId { get; set; }

        public Guid PrinterId { get; set; }
        public Guid PropertyId { get; set; }

        public string TonerServiceName  { get; set; }
        public string StaffName { get; set; }
        
        public DateTime OrderDate { get; set; }
        public string DisplayOrderDate { get { return OrderDate.ToString("yyy/MM/dd"); } }

        public TonerOrdersDto()
        {
            
        }

        public TonerOrdersDto(Guid orderId,Guid printerId,Guid propertyId, string printerName,string propertyName,string staffName,DateTime orderDate)
            :this()
        {
            OrderDetailsId = orderId;
            PrinterId = printerId;
            PropertyId = propertyId;

            TonerServiceName = printerName + " - " + propertyName;
            StaffName = staffName;
            OrderDate = orderDate;
        }
    }
}