CREATE TABLE [dbo].[tblMenuItems] (
    [MenuID]       UNIQUEIDENTIFIER NOT NULL,
    [MenuName]     VARCHAR (50)     NOT NULL,
    [MenuOrder]    INT              NULL,
    [RecordStatus] VARCHAR (20)     NULL,
    [MenuTemplate] VARCHAR (100)    NULL,
    CONSTRAINT [PK_tblMenuItems] PRIMARY KEY CLUSTERED ([MenuID] ASC)
);

