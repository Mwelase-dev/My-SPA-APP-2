using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intranet.Models
{
    public class MessagesModel:BaseModel
    {
        public Guid MessageId { get; set; }
        public string Subject { get; set; }
        public string Greeting { get; set; }
        public string Body { get; set; }
        public string Signature { get; set; }
        public int MessageType { get; set; }
    }
}
