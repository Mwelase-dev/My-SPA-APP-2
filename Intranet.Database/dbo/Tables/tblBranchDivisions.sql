CREATE TABLE [dbo].[tblBranchDivisions] (
    [DivisionID]       UNIQUEIDENTIFIER NOT NULL,
    [DivisionBranchID] UNIQUEIDENTIFIER NOT NULL,
    [DivisionName]     VARCHAR (100)    NOT NULL,
    [RecordStatus]     VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_tblBranchDivisions] PRIMARY KEY CLUSTERED ([DivisionID] ASC),
    CONSTRAINT [FK_Division_Branch] FOREIGN KEY ([DivisionBranchID]) REFERENCES [dbo].[tblBranches] ([BranchID])
);

