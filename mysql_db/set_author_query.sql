select *
from comics_new where title like '%��׿��ϫ�%';

select c.comic_id, title, a.name, c.filepath
from comics_new c
LEFT JOIN comics_authors ca
  ON c.comic_id = ca.comic_id
LEFT JOIN authors a
  ON ca.author_id = a.author_id
where ca.author_id is null;
  c.filepath like '%�۰���%'
  and;

INSERT INTO authors
(name, dscrpt)
values('��׿��ϫ�', null);

select * from authors;

update authors
SET dscrpt = '���� ���� ����'
where author_id = 14;

/*set authors*/
insert into comics_authors
SELECT c.comic_id, 17
FROM comics_new c
where c.title like '%��׿��ϫ�%';

insert into comics_authors
values (557, 8);

/*join author and comic*/
select cn.comic_id, a.author_id, cn.title, cn.title_img, cn.filepath, cn.filename, cn.filesize
from comics_authors ca
JOIN comics_new cn
ON ca.comic_id = cn.comic_id
JOIN authors a
on a.author_id = ca.author_id;

call sp_AuthorComicAll_get();

