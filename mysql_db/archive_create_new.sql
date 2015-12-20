DROP TABLE comics_new;
CREATE TABLE comics_new (
    comic_id     INT NOT NULL AUTO_INCREMENT,
    filename      NVARCHAR(255) NOT NULL,
    filepath      NVARCHAR(1000) NOT NULL,
    org_filename  NVARCHAR(255)  NOT NULL,
    title        NVARCHAR(255) NOT NULL,
    title_img     MEDIUMBLOB NOT NULL,
    title_img_ext  NVARCHAR(25)   NOT NULL,
    CONSTRAINT PK_comics PRIMARY KEY NONCLUSTERED (comic_id ASC),
    CONSTRAINT AK_comics UNIQUE NONCLUSTERED (filename ASC)
);