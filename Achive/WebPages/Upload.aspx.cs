using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using System.IO.Compression;

using System.Data;
using System.Data.SqlClient;
using System.Data.Odbc;

namespace Achive.WebPages
{
    public partial class Upload : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void register_Click(object sender, EventArgs e)
        {
            ODBCVersion();
        }

        void SqlVersion()
        {
            if (!fileupload1.HasFile)
                return;

            string sTitle = Path.GetFileNameWithoutExtension(fileupload1.FileName);

            int retId = -1;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "INSERT INTO dbo.comics (title, author) OUTPUT INSERTED.comic_id values(@title, @author);";
                cmd.Parameters.AddWithValue("@title", sTitle);
                cmd.Parameters.AddWithValue("@author", DBNull.Value);

                try
                {
                    con.Open();
                    retId = (int)cmd.ExecuteScalar();
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    con.Close();
                }
            }

            if (retId < 0)
            {
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "UPDATE dbo.comics_files " +
                    "SET org_file_name = @org_file_name, comic_file = @comic_file " +
                    "WHERE comic_id = @comic_id;" +
                    "IF @@rowcount = 0 " +
                    "INSERT INTO dbo.comics_files (comic_id, org_file_name, comic_file) values(@comic_id, @org_file_name, @comic_file);";

                cmd.Parameters.AddWithValue("@comic_id", retId);
                cmd.Parameters.AddWithValue("@org_file_name", fileupload1.FileName);
                cmd.Parameters.Add("@comic_file", SqlDbType.VarBinary);
                cmd.Parameters["@comic_file"].Value = fileupload1.FileBytes;

                try
                {
                    con.Open();
                    int ret = cmd.ExecuteNonQuery();
                    if (ret < 1)
                    {
                        throw new Exception("comics_files insert fail");
                    }
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    con.Close();
                }
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "UPDATE dbo.comics_files " +
                    "SET org_file_name = @org_file_name, comic_file = @comic_file " +
                    "WHERE comic_id = @comic_id;" +
                    "IF @@rowcount = 0 " +
                    "INSERT INTO dbo.comics_files (comic_id, org_file_name, comic_file) values(@comic_id, @org_file_name, @comic_file);";

                cmd.Parameters.AddWithValue("@comic_id", retId);
                cmd.Parameters.AddWithValue("@org_file_name", fileupload1.FileName);
                cmd.Parameters.Add("@comic_file", SqlDbType.VarBinary, fileupload1.FileBytes.Length);   // 90MB 짜리 파일에서 out of memory exception 뜬다. 이유??
                cmd.Parameters["@comic_file"].Value = fileupload1.FileBytes;

                try
                {
                    con.Open();
                    int ret = cmd.ExecuteNonQuery();
                    if (ret < 1)
                    {
                        throw new Exception("comics_files insert fail");
                    }
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    con.Close();
                }
            }

            //ParseAndSaveTitleImgUsingFileuploadControl(retId);
        }

        void ODBCVersion()
        {
            if (!fileupload1.HasFile)
                return;

            string sTitle = Path.GetFileNameWithoutExtension(fileupload1.FileName);

            Int32 retId = -1;
            
            using (OdbcConnection conn = new OdbcConnection(odbc_conn_string))
            {
                OdbcCommand cmd = new OdbcCommand();
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

            if (retId < 0)
            {
                return;
            }
        }

        void ParseAndSaveTitleImgUsingFileuploadControl(int comic_id)
        {
            // title image
            byte[] title_img_bytes = null;
            string title_img_ext = null;

            {
                string ext = Path.GetExtension(fileupload1.FileName);
                if (ext != ".zip")
                {
                    return;
                }

                MemoryStream ms = new MemoryStream(fileupload1.FileBytes);
                ZipArchive zarc = new ZipArchive(ms);
                foreach (ZipArchiveEntry ent in zarc.Entries)
                {
                    string zipext = Path.GetExtension(ent.Name);
                    if (zipext == ".jpg" || zipext == ".bmp" || zipext == ".png")
                    {   
                        using (var zstrm = ent.Open())
                        {
                            title_img_bytes = new byte[ent.Length];
                            zstrm.Read(title_img_bytes, 0, (int)ent.Length);

                            output_title_img.ImageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(title_img_bytes);
                            title_img_ext = Path.GetExtension(ent.Name);
                            //using (var fs = File.Create("c:/test/img/test.jpg"))
                            //{
                            //    fs.Write(title_img_bytes, 0, (int)ent.Length);
                            //    fs.Close();
                            //}

                            //using (var osw = new StreamWriter(Context.Response.OutputStream))
                            //{
                            //    osw.Write(sr.ReadToEnd());
                            //    Context.Response.ContentType = "image/JPEG";
                            //}
                        }

                        break;
                    }
                }
            }

            if (title_img_bytes == null)
            {
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "UPDATE dbo.comics_title_img " +
                    "SET ext = @ext, title_img = @title_img " +
                    "WHERE comic_id = @comic_id;" +
                    "IF @@rowcount = 0 " +
                    "INSERT INTO dbo.comics_title_img (comic_id, ext, title_img) values(@comic_id, @ext, @title_img);";

                cmd.Parameters.AddWithValue("@comic_id", comic_id);
                cmd.Parameters.AddWithValue("@ext", title_img_ext);
                cmd.Parameters.Add("@title_img", SqlDbType.VarBinary);
                cmd.Parameters["@title_img"].Value = title_img_bytes;

                try
                {
                    con.Open();
                    int ret = cmd.ExecuteNonQuery();
                    if (ret < 1)
                    {
                        throw new Exception("comics_title_img insert fail");
                    }
                }
                catch (Exception e1)
                {
                    throw e1;
                }
                finally
                {
                    con.Close();
                }
            }
        }


        private const string connectionString = "server = NV-PC\\SQLEXPRESS; database = archive; Integrated Security=SSPI";
        private const string odbc_conn_string = "FIL=MySQLDatabase; DSN=raspi32;";
    }
}