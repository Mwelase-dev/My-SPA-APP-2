CREATE TABLE [dbo].[tblHolidays] (
    [HolidayID]          UNIQUEIDENTIFIER NOT NULL,
    [HolidayDate]        DATETIME         NOT NULL,
    [HolidayDescription] VARCHAR (100)    NOT NULL,
    [RecordStatus]       VARCHAR (20)     NOT NULL,
    [HolidayIsAnnual]    VARCHAR (1)      NOT NULL,
    CONSTRAINT [PK_tblHolidays] PRIMARY KEY CLUSTERED ([HolidayID] ASC)
);

