CREATE TABLE [dbo].[tblStaffMembersHours] (
    [StaffID]        UNIQUEIDENTIFIER NOT NULL,
    [DayID]          INT              NOT NULL,
    [DayTimeStart]   DATETIME         NULL,
    [DayTimeEnd]     DATETIME         NULL,
    [DayLunchLength] INT              NULL,
    [RecordStatus]   VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblStaffMembersHours] PRIMARY KEY CLUSTERED ([StaffID] ASC, [DayID] ASC),
    CONSTRAINT [FK_StaffHours_Staff] FOREIGN KEY ([StaffID]) REFERENCES [dbo].[tblStaffMembersExtended] ([StaffID])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The ForeignKey of the staff member.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tblStaffMembersHours', @level2type = N'COLUMN', @level2name = N'StaffID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The IF of the weekday. There can only be 7. (Mon - Sun)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tblStaffMembersHours', @level2type = N'COLUMN', @level2name = N'DayID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The time that the staffmemember starts on this particular day.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tblStaffMembersHours', @level2type = N'COLUMN', @level2name = N'DayTimeStart';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The time that the staffmemember ends on this particular day.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tblStaffMembersHours', @level2type = N'COLUMN', @level2name = N'DayTimeEnd';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'The amount of Lunch time allowed for this staff member on the day (in minutes)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'tblStaffMembersHours', @level2type = N'COLUMN', @level2name = N'DayLunchLength';

