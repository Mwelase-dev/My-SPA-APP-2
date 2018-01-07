CREATE TABLE [dbo].[tblStaffMembersExtended] (
    [StaffID]               UNIQUEIDENTIFIER NOT NULL,
    [StaffJoinDate]         DATETIME         NOT NULL,
    [StaffLeaveAllowed]     INT              NULL,
    [StaffSickLeaveAllowed] INT              NULL,
    [StaffClockReminders]   INT              NULL,
    [StaffLeaveIncrement]   NUMERIC (15, 2)  NOT NULL,
    [StaffIsClockingMember] VARCHAR (1)      NULL,
    [StaffClockID]          INT              IDENTITY (1, 1) NOT NULL,
    [StaffManager1]         UNIQUEIDENTIFIER NULL,
    [StaffManager2]         UNIQUEIDENTIFIER NULL,
    [StaffPhoneStatus]      VARCHAR (50)     NULL,
    [StaffPhoneIP]          VARCHAR (50)     NULL,
    [StaffPhonePass]        VARCHAR (20)     NULL,
    [StaffPhoneMac]         VARCHAR (20)     NULL,
    [StaffPrinterDefault]   UNIQUEIDENTIFIER NULL,
    [RecordStatus]          VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK__tblStaff__96D4AAF7160F4887] PRIMARY KEY CLUSTERED ([StaffID] ASC),
    CONSTRAINT [FK_StaffExt_StaffExt1] FOREIGN KEY ([StaffManager1]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID]),
    CONSTRAINT [FK_StaffExt_StaffExt2] FOREIGN KEY ([StaffManager2]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID]),
    CONSTRAINT [FK_StaffExtended] FOREIGN KEY ([StaffID]) REFERENCES [dbo].[tblStaffMembers] ([StaffID])
);

