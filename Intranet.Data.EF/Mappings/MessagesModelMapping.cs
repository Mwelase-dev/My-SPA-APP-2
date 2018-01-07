using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intranet.Models;

namespace Intranet.Data.EF.Mappings
{
   public class MessagesModelMapping:EntityTypeConfiguration<MessagesModel>
    {
       public MessagesModelMapping()
       {
           ToTable("tblMessages");

           HasKey(m => m.MessageId);
           Property(m => m.MessageId).HasColumnName("MessageId");

           Property(m => m.Body).HasColumnName("MessageBody");
           Property(m => m.Greeting).HasColumnName("MessageGreeting");
           Property(m => m.MessageType).HasColumnName("MessageType");
           Property(m => m.Signature).HasColumnName("MessageSignature");
           Property(m => m.Subject).HasColumnName("MessageSubject");
           Property(m => m.RecordStatus).HasColumnName("RecordStatus");
       }
    }
}
