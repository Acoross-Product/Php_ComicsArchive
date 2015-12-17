CREATE TABLE [dbo].[comics_files](
	[comic_id] [int] NOT NULL,
	[org_file_name] [nvarchar](255) NOT NULL,
	[comic_file] [varbinary](max) NOT NULL,
	CONSTRAINT [PK_comics_files] PRIMARY KEY CLUSTERED (
		[comic_id] ASC
	)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[comics_files]  ADD  CONSTRAINT [FK_comics_files_comics] FOREIGN KEY([comic_id])
	REFERENCES [dbo].[comics] ([comic_id])
	ON UPDATE CASCADE
	ON DELETE CASCADE
GO

ALTER TABLE [dbo].[comics_files] CHECK CONSTRAINT [FK_comics_files_comics]
GO