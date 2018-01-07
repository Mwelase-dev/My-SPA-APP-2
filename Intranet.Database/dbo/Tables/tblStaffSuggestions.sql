CREATE TABLE [dbo].[tblStaffSuggestions] (
    [StaffID]           UNIQUEIDENTIFIER NOT NULL,
    [SuggestionID]      UNIQUEIDENTIFIER NOT NULL,
    [SuggestionSubject] VARCHAR (50)     NOT NULL,
    [Suggestion]        VARCHAR (500)    NOT NULL,
    [SuggestionDate]    DATETIME         NOT NULL,
    [RecordStatus]      VARCHAR (50)     CONSTRAINT [DF_tblStaffSuggestions_RecordStatus] DEFAULT ('Active') NOT NULL,
    CONSTRAINT [PK_tblStaffSuggestions] PRIMARY KEY CLUSTERED ([SuggestionID] ASC)
);

