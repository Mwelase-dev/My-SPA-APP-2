CREATE TABLE [dbo].[tblThoughts] (
    [ThoughtID]     UNIQUEIDENTIFIER CONSTRAINT [DF_tblThoughts_ThoughtID] DEFAULT (newid()) ROWGUIDCOL NOT NULL,
    [Thought]       VARCHAR (MAX)    NOT NULL,
    [ThoughtAuthor] VARCHAR (100)    NOT NULL,
    [RecordStatus]  VARCHAR (20)     NOT NULL,
    CONSTRAINT [PK_tblThoughts_1] PRIMARY KEY CLUSTERED ([ThoughtID] ASC)
);

