CREATE TABLE [dbo].[tblLinks] (
    [CategoryID]   UNIQUEIDENTIFIER NOT NULL,
    [LinkID]       UNIQUEIDENTIFIER NOT NULL,
    [LinkDesc]     VARCHAR (100)    NOT NULL,
    [LinkURL]      VARCHAR (200)    NOT NULL,
    [RecordStatus] VARCHAR (20)     NOT NULL,
    CONSTRAINT [PK_tblLinks] PRIMARY KEY CLUSTERED ([CategoryID] ASC, [LinkID] ASC),
    CONSTRAINT [FK_tblLinks] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tblLinkCategories] ([CategoryID])
);

