CREATE TABLE [dbo].[tblStaffMembersContacts] (
    [StaffID]            UNIQUEIDENTIFIER NOT NULL,
    [ContactID]          UNIQUEIDENTIFIER NOT NULL,
    [ContactName]        VARCHAR (100)    NOT NULL,
    [ContactSurname]     VARCHAR (100)    NOT NULL,
    [ContactNumber]      VARCHAR (50)     NOT NULL,
    [ContactDescription] VARCHAR (250)    NULL,
    [RecordStatus]       VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblStaffMembersContacts] PRIMARY KEY CLUSTERED ([ContactID] ASC),
    CONSTRAINT [FK_Contacts_Staff] FOREIGN KEY ([StaffID]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID])
);

