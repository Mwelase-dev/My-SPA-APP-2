using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using Intranet.Models;


namespace Intranet.Data.EF.Mappings
{
    public class PrinterModelMapping : EntityTypeConfiguration<PrinterModel>
    {
        public PrinterModelMapping()
        {
            ToTable("tblPrinters");
            HasKey(x => x.PrinterId);

            //Property(x => x.PrinterId)
            //    .HasColumnName("PrinterId")
            //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.PrinterMake).HasColumnName("PrinterMake");
            Property(x => x.Model).HasColumnName("Model");
            Property(x => x.Location).HasColumnName("Location");




            Property(x => x.SerialNumber).HasColumnName("SerialNumber");
            Property(x => x.ProviderId).HasColumnName("ProviderId");

            Property(x => x.PrinterDescription).HasColumnName("PrinterDescription");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");

             //HasMany(x => x.Properties).WithRequired(m => m.Printer).HasForeignKey(m => m.PrinterId);
            

            //HasRequired(x => x.PrinterProvider)
            //    .WithMany(x => x.Printers)
            //    .HasForeignKey(x => x.PrinterId);


            //printer to printer properties many to many mapping
             HasMany(m => m.Properties)
                 .WithRequired(m => m.Printer)
                 .HasForeignKey(m => m.PrinterId);

            //Staff member default printers
            //  HasMany(x => x.StaffModels)
            //      .WithOptional(x => x.Printer)
            //      .HasForeignKey(x => x.StaffDefaultPrinter);
        }
    }

    public class PrinterServiceProviderModelMapping : EntityTypeConfiguration<PrinterServiceProviderModel>
    {
        public PrinterServiceProviderModelMapping()
        {
            ToTable("tblPrinterServiceProvider");
            HasKey(x => x.ProviderId);

            Property(x => x.ProviderId)
                .HasColumnName("ProviderId")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.ProviderName).HasColumnName("ProviderName");
            Property(x => x.ProviderEmail).HasColumnName("ProviderEmail");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
          
             HasMany(x => x.Printers)
                  .WithRequired(x => x.PrinterProvider)
                  .HasForeignKey(x => x.ProviderId);

            

        }
    }

    public class PrinterPropertiesModelMapping : EntityTypeConfiguration<PrinterPropertyModel>
    {
        public PrinterPropertiesModelMapping()
        {
            ToTable("tblPrinterProperties");
            HasKey(x => x.PropertyId);
            Property(x => x.PropertyId)
                .HasColumnName("PropertyId")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.PropertyDescription).HasColumnName("PropertyDescription");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
   

            //Linking table(Printer properties printer)
              HasMany(m => m.PrinterProperties)
                  .WithRequired(m => m.Property)
                  .HasForeignKey(m => m.PropertyId);
            
        }
    }

    public class TonerOrdersModelMapping : EntityTypeConfiguration<TonerOrdersModel>
    {
        public TonerOrdersModelMapping()
        {
            ToTable("tblTonerOrders");
            HasKey(x => x.OrderId);
            //Property(x => x.OrderId)
            //    .HasColumnName("OrderId")
            //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.OrderDate)
                .HasColumnName("OrderDate")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            Property(x => x.StaffId).HasColumnName("StaffId");
            Property(x => x.PrinterId).HasColumnName("PrinterId");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
           

            HasMany(x => x.OrderDetails)
                .WithRequired(x => x.TonerOrder)
                .HasForeignKey(x => x.OrderId);

        }
    }

    public class TonerOrderDetailsModelMapping : EntityTypeConfiguration<TonerOrderDetailsModel>
    {
        public TonerOrderDetailsModelMapping()
        {
            ToTable("tblTonerOrderDetails");
            HasKey(x => x.DetailsId);
            //Property(x => x.DetailsId)
            //    .HasColumnName("DetailsId")
            //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.OrderId).HasColumnName("OrderId");
            Property(x => x.PropertyId).HasColumnName("PropertyId");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
            Property(x => x.TonerOrderReminder).HasColumnName("TonerOrderReminder");
            Property(x => x.LastTonerOrderReminder).HasColumnName("LastTonerOrderReminder");
            

        }
    }

    public class PrinterPropertiesPrinterMapping : EntityTypeConfiguration<PrinterPropertiesPrinterModel>
    {
        public PrinterPropertiesPrinterMapping()
        {
            ToTable("tblPrinterPropertiesPrinter");
            HasKey(x => new { x.PrinterId, x.PropertyId });

            Property(x => x.PrinterId).HasColumnName("PrinterId");
            //Property(x => x.PropertyId).HasColumnName("PropertyId");
            Property(x => x.RecordStatus).HasColumnName("RecordStatus");
        
            
           //Ignore(x=>x.Property);
           // Ignore(x=>x.Printer);

            //  HasRequired(m => m.Printer)
            //     .WithMany(m => m.Properties)
            //      .HasForeignKey(m => m.PrinterId);

            //HasRequired(m => m.Property)
            ///    .WithMany(m => m.PrinterProperties)
              //  .HasForeignKey(m => m.PropertyId);
        }
    }

    //public class PrinterPropertiesPropertiesMapping : EntityTypeConfiguration<PrinterPropertiesProperties>
    //{
    //    public PrinterPropertiesPropertiesMapping()
    //    {
    //        ToTable("tblPrinterPropertiesPrinter");
    //        HasKey(x => new { x.PrinterId, x.PropertyId });

    //        Property(x => x.PrinterId).HasColumnName("PrinterId");
    //        Property(x => x.RecordStatus).HasColumnName("RecordStatus");
         
    //    }
    //}
    
}
