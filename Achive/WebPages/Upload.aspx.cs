using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;

using System.Data;
using System.Data.SqlClient;

using ComicFileUploader;

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
        
        static void processOneFile(HttpPostedFile postedfile)
        {
            byte[] title_img_bytes;
            string title_img_ext;

            string ext = Path.GetExtension(postedfile.FileName);
            if (ext != ".zip")
            {
                return;
            }

            byte[] filebytes = null;
            using (var br = new BinaryReader(postedfile.InputStream))
            {
                filebytes = br.ReadBytes(postedfile.ContentLength);
            }
            
            OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, filebytes);
            if (title_img_bytes == null)
            {
                return;
            }
            //output_title_img.ImageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(title_img_bytes);

            string sTitle = Path.GetFileNameWithoutExtension(postedfile.FileName);
            int retId = OneFileUploader.odbc_InsertNewComics(sTitle);

            if (retId < 0)
            {
                return;
            }

            //ODBC_comics_files(retId);
            string filepath = "\\\\RASPBERRYPI\\RaspberryPI\\Extern\\personal\\Acoross\\Codes\\DnD_4e_Assist\\DnD_4e_Assist\\dat\\comic_archive_data\\";
            string randFilename = Path.GetRandomFileName();
            string fullpath = filepath + randFilename;
            while (File.Exists(fullpath))
            {
                randFilename = Path.GetRandomFileName();
                fullpath = filepath + randFilename;
            }

            //postedfile.SaveAs(filepathToSave);
            OneFileUploader.SaveComicFileTo(filebytes, fullpath);

            OneFileUploader.odbc_Savefilepath(retId, postedfile.FileName, randFilename);

            OneFileUploader.odbc_SaveTitleImg(retId, title_img_bytes, title_img_ext);
        }

        void ODBCVersion()
        {
            if (!fileupload1.HasFile)
                return;

            foreach (var postedfile in fileupload1.PostedFiles)
            {
                processOneFile(postedfile);
            }
        }
                
        #region SQLversions
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


        void sql_SaveTitleImg(int comic_id, string title_img_ext, byte[] title_img_bytes)
        {
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
        #endregion

        private const string connectionString = "server = NV-PC\\SQLEXPRESS; database = archive; Integrated Security=SSPI";
        //private const string odbc_conn_string = "FIL=MySQLDatabase; DSN=raspi;";
    }
}