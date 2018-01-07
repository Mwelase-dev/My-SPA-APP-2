CREATE TABLE [dbo].[tblAnnouncements] (
    [AnnouncementID]      UNIQUEIDENTIFIER NOT NULL,
    [AnnouncementSubject] VARCHAR (50)     NOT NULL,
    [Announcement]        VARCHAR (MAX)    NOT NULL,
    [AnnouncementAuthor]  VARCHAR (100)    NOT NULL,
    [AnnouncementDate]    DATETIME         NOT NULL,
    [RecordStatus]        VARCHAR (20)     NOT NULL,
    CONSTRAINT [PK__tblAnnouncements__29572725] PRIMARY KEY CLUSTERED ([AnnouncementID] ASC)
);

