CREATE TABLE [dbo].[tblPrinterProperties] (
    [PrinterID]  UNIQUEIDENTIFIER NOT NULL,
    [PropertyID] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_tblPrinterProperties] PRIMARY KEY CLUSTERED ([PrinterID] ASC, [PropertyID] ASC)
);

