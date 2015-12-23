using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;
using System.IO;
using System.IO.Compression;
using SharpCompress;
using SharpCompress.Archive.Rar;

namespace ComicFileUploader
{
    public class odbcExecuter
    {
        public static int ExecuteNonQuery(string odbc_conn_string, OdbcCommand cmd)
        {
            using (var conn = new OdbcConnection(odbc_conn_string))
            {
                cmd.Connection = conn;

                try
                {
                    conn.Open();
                    cmd.Prepare();
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        public static object ExecuteScalar(string odbc_conn_string, OdbcCommand cmd)
        {
            using (var conn = new OdbcConnection(odbc_conn_string))
            {
                cmd.Connection = conn;

                try
                {
                    conn.Open();
                    cmd.Prepare();
                    return cmd.ExecuteScalar();
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    conn.Close();
                }
            }
        }
    }

    public class OneFileUploader
    {
        /////////////////////////////////////////////
        public static void SaveComicFileTo(byte[] comic_file_bytes, string filepath)
        {
            if (comic_file_bytes == null)
                return;

            using (FileStream fs = File.Create(filepath))
            {
                fs.Write(comic_file_bytes, 0, comic_file_bytes.Length);

                fs.Close();
            }
        }

        public static void ExtractTitleImgFromRar(out byte[] title_img_bytes, out string title_img_ext, Stream zipStream)
        {
            // title image
            title_img_bytes = null;
            title_img_ext = null;

            using (RarArchive rarc = RarArchive.Open(zipStream))
            {
                foreach (var ent in rarc.Entries)
                {
                    string zipext = Path.GetExtension(ent.FilePath).ToLower();
                    if (zipext == ".jpg" || zipext == ".jpeg")
                    {
                        using (var rstrm = ent.OpenEntryStream())
                        {
                            title_img_bytes = new byte[ent.Size];
                            rstrm.Read(title_img_bytes, 0, (int)ent.Size);
                            title_img_ext = zipext;
                        }

                        break;
                    }
                }
            }
        }

        public static void ExtractTitleImgFromZip(out byte[] title_img_bytes, out string title_img_ext, Stream zipStream)
        {
            // title image
            title_img_bytes = null;
            title_img_ext = null;

            using (ZipArchive zarc = new ZipArchive(zipStream))
            {
                foreach (ZipArchiveEntry ent in zarc.Entries)
                {   
                    string zipext = Path.GetExtension(ent.Name).ToLower();
                    if (zipext == ".jpg" || zipext == ".jpeg")
                    {
                        using (var zstrm = ent.Open())
                        {
                            title_img_bytes = new byte[ent.Length];
                            zstrm.Read(title_img_bytes, 0, (int)ent.Length);
                            title_img_ext = zipext;
                        }

                        break;
                    }
                }
            }
        }
        //////////////////////////////////////////////


        private const string odbc_conn_string = "FIL=MySQLDatabase; DSN=raspi;";

        public static bool odbc_CheckSameComicsNew(string filename)
        {
            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "SELECT comic_id FROM comics_new WHERE filename = ?;";
                cmd.Parameters.AddWithValue("@filename", filename);

                object ret = odbcExecuter.ExecuteScalar(odbc_conn_string, cmd);

                int ID = Convert.ToInt32(ret);
                if (ID > 0)
                    return true;
                else
                    return false;
            }
        }

        public static bool odbc_CheckSameFullpath(string filename, string filepath)
        {
            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "SELECT comic_id FROM comics_new WHERE filename = ? and filepath = ?;";
                cmd.Parameters.AddWithValue("@filename", filename);
                cmd.Parameters.AddWithValue("@filepath", filepath);

                object ret = odbcExecuter.ExecuteScalar(odbc_conn_string, cmd);

                int ID = Convert.ToInt32(ret);
                if (ID > 0)
                    return true;
                else
                    return false;
            }
        }

        public static void odbc_SaveComicsNew(string filename, string filepath, string org_filename, int filesize, string title, byte[] title_img, string title_img_ext, string local_libDir, string relative_filedir)
        {
            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "UPDATE comics_new " +
                                "SET filepath = ?, org_filename = ?, filesize = ?, title = ?, title_img = ?, title_img_ext = ?, local_libDir = ?, relative_filedir = ?" +
                                "WHERE filename = ?";
                cmd.Parameters.AddWithValue("@filepath", filepath);
                cmd.Parameters.AddWithValue("@org_filename", org_filename);
                cmd.Parameters.AddWithValue("@filesize", filesize);
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.Add("@title_img", OdbcType.Binary);
                cmd.Parameters["@title_img"].Value = title_img;
                cmd.Parameters.AddWithValue("@ext", title_img_ext);
                cmd.Parameters.AddWithValue("@local_libDir", local_libDir);
                cmd.Parameters.AddWithValue("@relative_filedir", relative_filedir);
                cmd.Parameters.AddWithValue("@filename", filename);

                if (odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd) < 1)
                {
                    int ID;
                    odbc_InsertNew_ComicsNew(out ID, filename, filepath, org_filename, filesize, title, title_img, title_img_ext, local_libDir, relative_filedir);
                    if (ID < 1)
                    {
                        throw new Exception("comics_new fail");
                    }
                }
            }
        }

        public static void odbc_InsertNew_ComicsNew(out int ID, string filename, string filepath, string org_filename, int filesize, string title, byte[] title_img, string title_img_ext, string local_libDir, string relative_filedir)
        {
            ID = -1;

            using (OdbcConnection conn = new OdbcConnection(odbc_conn_string))
            {
                using (var cmd = new OdbcCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "INSERT INTO comics_new (filename, filepath, org_filename, filesize, title, title_img, title_img_ext, local_libDir, relative_filedir) " +
                                      "values(?, ?, ?, ?, ?, ?, ?, ?, ?);";
                    cmd.Parameters.AddWithValue("@filename", filename);
                    cmd.Parameters.AddWithValue("@filepath", filepath);
                    cmd.Parameters.AddWithValue("@org_filename", org_filename);
                    cmd.Parameters.AddWithValue("@filesize", filesize);
                    cmd.Parameters.AddWithValue("@title", title);
                    cmd.Parameters.Add("@title_img", OdbcType.Binary);
                    cmd.Parameters["@title_img"].Value = title_img;
                    cmd.Parameters.AddWithValue("@ext", title_img_ext);
                    cmd.Parameters.AddWithValue("@local_libDir", local_libDir);
                    cmd.Parameters.AddWithValue("@relative_filedir", relative_filedir);

                    try
                    {
                        conn.Open();
                        cmd.Prepare();
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            cmd.CommandText = "SELECT CAST(LAST_INSERT_ID() as unsigned integer);";
                            object obj = cmd.ExecuteScalar();
                            ID = Convert.ToInt32(obj);
                        }
                    }
                    catch (Exception e1)
                    {
                        throw e1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public static int odbc_InsertNewComics(string sTitle)
        {
            Int32 retId = -1;

            using (OdbcConnection conn = new OdbcConnection(odbc_conn_string))
            {
                using (var cmd = new OdbcCommand())
                {
                    cmd.Connection = conn;

                    cmd.CommandText = "INSERT INTO comics (title, author) values(?, ?);";
                    cmd.Parameters.AddWithValue("@title", sTitle);
                    cmd.Parameters.AddWithValue("author", DBNull.Value);

                    try
                    {
                        conn.Open();
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            cmd.CommandText = "SELECT CAST(LAST_INSERT_ID() as unsigned integer);";
                            object obj = cmd.ExecuteScalar();
                            retId = Convert.ToInt32(obj);
                        }
                    }
                    catch (Exception e1)
                    {
                        throw e1;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }   
            }

            return retId;
        }

        public static void odbc_Savefilepath(int comic_id, string org_filename, string filepath)
        {
            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "UPDATE comics_filepaths " +
                    "SET org_filename = ?, filepath = ? " +
                    "WHERE comic_id = ?;";

                cmd.Parameters.AddWithValue("@org_filename", org_filename);
                cmd.Parameters.AddWithValue("@filepath", filepath);
                cmd.Parameters.AddWithValue("@comic_id", comic_id);

                if (odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd) > 0)
                    return;
            }

            using (var cmd2 = new OdbcCommand())
            {
                cmd2.CommandText = "INSERT INTO comics_filepaths (comic_id, org_filename, filepath) values(?, ?, ?);";
                cmd2.Parameters.AddWithValue("@comic_id", comic_id);
                cmd2.Parameters.AddWithValue("@org_filename", org_filename);
                cmd2.Parameters.AddWithValue("@filepath", filepath);

                if (odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd2) < 1)
                {
                    throw new Exception("comics_filepaths fail");
                }
            }
        }

        public static void odbc_SaveTitleImg(int comic_id, byte[] title_img_bytes, string title_img_ext)
        {
            if (title_img_bytes == null)
                return;

            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "UPDATE comics_title_img " +
                "SET ext = ?, title_img = ? " +
                "WHERE comic_id = ?;";

                cmd.Parameters.AddWithValue("@ext", title_img_ext);
                cmd.Parameters.Add("@title_img", OdbcType.Binary);
                cmd.Parameters["@title_img"].Value = title_img_bytes;
                cmd.Parameters.AddWithValue("@comic_id", comic_id);

                if (odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd) > 0)
                    return;
            }

            using (var cmd2 = new OdbcCommand())
            {
                cmd2.CommandText = "INSERT INTO comics_title_img (comic_id, ext, title_img) values(?, ?, ?);";

                cmd2.Parameters.AddWithValue("@comic_id", comic_id);
                cmd2.Parameters.AddWithValue("@ext", title_img_ext);
                cmd2.Parameters.Add("@title_img", OdbcType.Binary);
                cmd2.Parameters["@title_img"].Value = title_img_bytes;

                if (odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd2) < 1)
                {
                    throw new Exception("comics_title_img fail");
                }
            }
        }

        #region blob
        public static void odbc_comics_files(int retId, string filename, byte[] filebytes)
        {
            int ret = 0;
            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "UPDATE comics_files " +
                    "SET org_file_name = ?, comic_file = ? " +
                    "WHERE comic_id = ?;";

                cmd.Parameters.AddWithValue("@org_file_name", filename);
                cmd.Parameters.Add("@comic_file", OdbcType.Binary, filebytes.Length);
                cmd.Parameters["@comic_file"].Value = filebytes;
                cmd.Parameters.AddWithValue("@comic_id", retId);

                ret = odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd);
            }

            if (ret > 0)
                return;

            using (var cmd = new OdbcCommand())
            {
                cmd.CommandText = "INSERT INTO comics_files (comic_id, org_file_name, comic_file) values(?, ?, ?);";

                cmd.Parameters.AddWithValue("@comic_id", retId);
                cmd.Parameters.AddWithValue("@org_file_name", filename);
                cmd.Parameters.Add("@comic_file", OdbcType.Binary, filebytes.Length);
                cmd.Parameters["@comic_file"].Value = filebytes;

                ret = odbcExecuter.ExecuteNonQuery(odbc_conn_string, cmd);
            }

            if (ret < 1)
                throw new Exception("comics_files fail.");
        }
        #endregion
    }
}
