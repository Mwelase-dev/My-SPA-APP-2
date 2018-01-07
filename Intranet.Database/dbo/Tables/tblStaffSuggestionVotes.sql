CREATE TABLE [dbo].[tblStaffSuggestionVotes] (
    [SuggestionID]  UNIQUEIDENTIFIER NOT NULL,
    [StaffID]       UNIQUEIDENTIFIER NOT NULL,
    [StaffComments] VARCHAR (200)    NOT NULL,
    [StaffVoteDate] DATETIME         NOT NULL,
    [RecordStatus]  VARCHAR (20)     CONSTRAINT [DF_tblStaffSuggestionVotes_RecordStatus] DEFAULT ('Active') NOT NULL,
    CONSTRAINT [PK_tblSuggestionVotes] PRIMARY KEY CLUSTERED ([SuggestionID] ASC, [StaffID] ASC),
    CONSTRAINT [FK_Votes_Suggestions] FOREIGN KEY ([SuggestionID]) REFERENCES [dbo].[tblStaffSuggestions] ([SuggestionID])
);

