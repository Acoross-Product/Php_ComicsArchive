CREATE TABLE [dbo].[authors](
	[author_id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_authors] PRIMARY KEY NONCLUSTERED 
(
	[author_id] ASC
),
 CONSTRAINT [AK_authors] UNIQUE NONCLUSTERED 
(
	[author_id] ASC,
	[name] ASC
)
) ON [PRIMARY]
GO


