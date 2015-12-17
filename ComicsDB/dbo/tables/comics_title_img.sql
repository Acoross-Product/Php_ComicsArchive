CREATE TABLE [dbo].[comics_title_img](
	[comic_id] [int] NOT NULL,
	[ext] [nvarchar](25) NOT NULL,
	[title_img] [varbinary](max) NOT NULL,
	CONSTRAINT [PK_comics_title_img] PRIMARY KEY CLUSTERED (
		[comic_id] ASC
	)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[comics_title_img]  ADD  CONSTRAINT [FK_comics_title_img_comics] FOREIGN KEY([comic_id])
	REFERENCES [dbo].[comics] ([comic_id])
	ON UPDATE CASCADE
	ON DELETE CASCADE
GO

ALTER TABLE [dbo].[comics_title_img] CHECK CONSTRAINT [FK_comics_title_img_comics]
GO