CREATE TABLE [dbo].[tblStaffMembersClockData] (
    [ClockID]       UNIQUEIDENTIFIER NOT NULL, 
    [StaffID]       UNIQUEIDENTIFIER NOT NULL,
    [ClockDateTime] DATETIME         NOT NULL,
    [RecordStatus]  VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblStaffClockData] PRIMARY KEY CLUSTERED ([ClockID], [ClockDateTime], [StaffID]),
    CONSTRAINT [FK_StaffClock_Staff] FOREIGN KEY ([StaffID]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID]) 
);

