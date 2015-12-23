use archive2;


DROP TABLE comics_authors;
DROP TABLE authors;
DROP TABLE comics_new;
DROP PROCEDURE sp_AuthorComicAll_get;


CREATE TABLE comics_new (
    comic_id     INT NOT NULL AUTO_INCREMENT,
    filename      NVARCHAR(255) NOT NULL,
    filepath      NVARCHAR(1000) NOT NULL,
    org_filename  NVARCHAR(255)  NOT NULL,
    filesize      INT NOT NULL,
    title        NVARCHAR(255) NOT NULL,
    title_img     MEDIUMBLOB NOT NULL,
    title_img_ext  NVARCHAR(25)   NOT NULL,
    local_libDir      NVARCHAR(1000) NOT NULL,
    relative_filedir      NVARCHAR(1000) NOT NULL,
    CONSTRAINT PK_comics PRIMARY KEY NONCLUSTERED (comic_id ASC),
    CONSTRAINT AK_comics UNIQUE NONCLUSTERED (filename ASC)
);

CREATE TABLE authors (
    author_id INT NOT NULL AUTO_INCREMENT,
    name      NVARCHAR (255) NOT NULL,
    dscrpt    NVARCHAR(1000) NULL,
    CONSTRAINT PK_authors PRIMARY KEY NONCLUSTERED (author_id ASC),
    CONSTRAINT AK_authors UNIQUE NONCLUSTERED (name ASC)
);

CREATE TABLE comics_authors (
  comic_id INT NOT NULL,
  author_id INT NOT NULL,
  CONSTRAINT PK_comics_authors PRIMARY KEY NONCLUSTERED (comic_id, author_id)
);

ALTER TABLE comics_authors
  ADD CONSTRAINT FK_comics_authors_comics_new FOREIGN KEY (comic_id)
    REFERENCES comics_new (comic_id) ON DELETE CASCADE ON UPDATE CASCADE;

ALTER TABLE comics_authors
  ADD CONSTRAINT FK_comics_authors_authors FOREIGN KEY (author_id)
    REFERENCES authors (author_id) ON DELETE CASCADE ON UPDATE CASCADE;


DELIMITER //
CREATE PROCEDURE sp_AuthorComicAll_get()
BEGIN
  /*join author and comic*/
select cn.comic_id, a.author_id, cn.title, cn.title_img, cn.filepath, cn.filename, cn.filesize
from comics_authors ca
JOIN comics_new cn
ON ca.comic_id = cn.comic_id
JOIN authors a
on a.author_id = ca.author_id;
END //
DELIMITER ;