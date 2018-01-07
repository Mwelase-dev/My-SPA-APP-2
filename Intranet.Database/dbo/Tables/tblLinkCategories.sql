CREATE TABLE [dbo].[tblLinkCategories] (
    [CategoryID]    UNIQUEIDENTIFIER NOT NULL,
    [CategoryDesc]  VARCHAR (100)    NOT NULL,
    [CategoryOrder] INT              NULL,
    [RecordStatus]  VARCHAR (20)     NULL,
    CONSTRAINT [PK_tblLinkCategories] PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);

