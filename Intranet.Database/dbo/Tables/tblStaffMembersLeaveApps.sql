CREATE TABLE [dbo].[tblStaffMembersLeaveApps] (
    [StaffID]          UNIQUEIDENTIFIER NOT NULL,
    [LeaveID]          UNIQUEIDENTIFIER NOT NULL,
    [LeaveDateStart]   DATETIME         NOT NULL,
    [LeaveDateEnd]     DATETIME         NOT NULL,
    [LeaveComments]    VARCHAR (500)    CONSTRAINT [DF_tblStaffLeaveApps_LeaveComments] DEFAULT ('') NULL,
    [LeaveApprovedBy1] UNIQUEIDENTIFIER NULL,
    [LeaveApprovedBy2] UNIQUEIDENTIFIER NULL,
    [LeaveType]        VARCHAR (50)     NOT NULL,
    [LeaveStatus]      VARCHAR (20)     NOT NULL,
    [LeaveRequestDate] DATETIME         NOT NULL,
    [RecordStatus]     VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblStaffLeaveApps] PRIMARY KEY CLUSTERED ([LeaveID] ASC),
    CONSTRAINT [FK_StaffLeave_Staff] FOREIGN KEY ([StaffID]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID]),
    CONSTRAINT [FK_StaffLeaveApproved1] FOREIGN KEY ([LeaveApprovedBy1]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID]),
    CONSTRAINT [FK_StaffLeaveApproved2] FOREIGN KEY ([LeaveApprovedBy2]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID])
);

