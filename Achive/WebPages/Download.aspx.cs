using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Data.SqlClient;

namespace Achive.WebPages
{
    public partial class Download : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void download_Click(object sender, EventArgs e)
        {
            string filename = null;
            Byte[] filebytes = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "SELECT org_file_name, comic_file FROM dbo.comics_files WHERE comic_id = @comic_id";
                cmd.Parameters.AddWithValue("@comic_id", int.Parse(comic_id.Text));

                try
                {
                    con.Open();
                    var sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        filename = (string)sdr["org_file_name"];
                        filebytes = (Byte[])sdr["comic_file"];
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

            if (filebytes == null)
            {
                throw new Exception("couldn't get comic_file from comics_files");
            }

            WriteFile(filebytes, filename);
        }

        private void WriteFile(Byte[] bytes, string filename)
        {
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = "text/plain";
            Response.AddHeader("content-disposition", "attachment;filename=" + filename);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }

        private const string connectionString = "server = NV-PC\\SQLEXPRESS; database = archive; Integrated Security=SSPI";
    }
}