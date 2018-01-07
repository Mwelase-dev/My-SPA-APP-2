CREATE TABLE [dbo].[tblStaffMembers] (
    [StaffDivisionID]   UNIQUEIDENTIFIER NULL,
    [StaffID]           UNIQUEIDENTIFIER NOT NULL,
    [StaffName]         VARCHAR (100)    NOT NULL,
    [StaffSurname]      VARCHAR (100)    NOT NULL,
    [StaffIDNumber]     VARCHAR (50)     NOT NULL,
    [StaffJoinDate]     DATETIME         NOT NULL,
    [StaffTelExt]       VARCHAR (13)     NULL,
    [StaffTelDirect]    VARCHAR (13)     NULL,
    [StaffCellphone]    VARCHAR (13)     NULL,
    [StaffFaxNumber]    VARCHAR (13)     CONSTRAINT [DF_tblStaffMembers_StaffFaxNumber] DEFAULT ('') NULL,
    [StaffEmailAddress] VARCHAR (250)    NULL,
    [StaffNTName]       VARCHAR (50)     NULL,
    [RecordStatus]      VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblStaffMembers] PRIMARY KEY CLUSTERED ([StaffID] ASC),
    CONSTRAINT [FK_Staff_Division] FOREIGN KEY ([StaffDivisionID]) REFERENCES [dbo].[tblBranchDivisions] ([DivisionID])
);

