CREATE TABLE [dbo].[tblBranches] (
    [BranchID]        UNIQUEIDENTIFIER NOT NULL,
    [BranchName]      VARCHAR (50)     NOT NULL,
    [BranchShortName] VARCHAR (50)     NOT NULL,
    [RecordStatus]    VARCHAR (20)     NOT NULL,
    CONSTRAINT [PK_tblBranches] PRIMARY KEY CLUSTERED ([BranchID] ASC)
);

