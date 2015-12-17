CREATE TABLE [dbo].[comics](
	[comic_id] [int] IDENTITY(1,1) NOT NULL,
	[title] [nvarchar](255) NOT NULL,
	[author] [nvarchar](255) NULL,
	CONSTRAINT [PK_comics] PRIMARY KEY NONCLUSTERED ([comic_id] ASC),
	CONSTRAINT [AK_comics] UNIQUE ([comic_id] ASC,	[title] ASC)
) ON [PRIMARY]
GO