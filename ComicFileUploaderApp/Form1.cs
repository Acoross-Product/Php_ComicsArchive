using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Threading;

using ComicFileUploader;

namespace ComicFileUploaderApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 파일 열기 버튼 클릭
        private void btOpenFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            if (openFileDialog1.FileNames.Length <= 0)
                return;
            
            string Dir = openFileDialog1.FileName.TrimEnd(openFileDialog1.SafeFileName.ToCharArray());
            var di = new System.IO.DirectoryInfo(Dir);
            if (!di.Exists)
                return;

            backgroundWorker1.RunWorkerAsync(di);
        }
        
        static bool processOneFile(FileInfo postedfile)
        {
            byte[] title_img_bytes;
            string title_img_ext;

            string ext = Path.GetExtension(postedfile.Name);
            if (ext != ".zip")
            {
                return false;
            }

            byte[] filebytes = new byte[(int)postedfile.Length];
            using (var fs = postedfile.OpenRead())
            {
                fs.Read(filebytes, 0, (int)postedfile.Length);
            }

            OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, filebytes);
            if (title_img_bytes == null)
            {
                return false;
            }
            //output_title_img.ImageUrl = "data:image/jpeg;base64," + Convert.ToBase64String(title_img_bytes);

            string sTitle = Path.GetFileNameWithoutExtension(postedfile.Name);
            int retId = OneFileUploader.odbc_InsertNewComics(sTitle);
            if (retId < 0)
            {
                return false;
            }

            OneFileUploader.odbc_SaveTitleImg(retId, title_img_bytes, title_img_ext);

            //ODBC_comics_files(retId);

            string randFilename = Path.GetRandomFileName();
            string fullpath = filepath + randFilename;            
            while (File.Exists(fullpath))
            {
                randFilename = Path.GetRandomFileName();
                fullpath = filepath + randFilename;
            }
            
            OneFileUploader.SaveComicFileTo(filebytes, fullpath);

            OneFileUploader.odbc_Savefilepath(retId, postedfile.Name, randFilename);

            return true;
        }

        const string filepath = "\\\\RASPBERRYPI\\RaspberryPI\\Extern\\personal\\Acoross\\Codes\\DnD_4e_Assist\\DnD_4e_Assist\\dat\\comic_archive_data\\";

        // 모두 업로드 버튼
        private void btUploadAll_Click(object sender, EventArgs e)
        {
            foreach (string filename in (ArrayList)listBox1.DataSource)
            {
                string sTitle = Path.GetFileNameWithoutExtension(filename);
                OneFileUploader.odbc_InsertNewComics(filename);
            }
        }

        void ProcessFileUploadEnd(string filename, string msg)
        {
            Logger.Add("[upload " + msg + "]" + filename);
            listBox1.Invoke((MethodInvoker)delegate
            {
                m_EndJobList.Add("[" + msg + " ]:" + filename);
                BindListBox1();
            });
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var di = (DirectoryInfo)e.Argument;

            var files = di.GetFiles();

            Logger.Add(DateTime.Now.ToShortTimeString());
            Logger.Add("upload start.");
            
            int filenum = files.Length;
            int done = 0;
            foreach (FileInfo fi in di.GetFiles())
            {
                int per = 100 * done / filenum;
                backgroundWorker1.ReportProgress(per, fi.Name);

                try
                {
                    if (processOneFile(fi))
                    {
                        ProcessFileUploadEnd(fi.Name, "success");
                    }
                    else
                    {
                        ProcessFileUploadEnd(fi.Name, "fail");
                    }
                }
                catch (Exception ex)
                {
                    ProcessFileUploadEnd(fi.Name, "exception");
                    Logger.Add(ex.ToString());
                }
                
                ++done;
            }

            backgroundWorker1.ReportProgress(100, "complete!");
            MessageBox.Show("file upload end.");
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
            lbProgress.Text = (string)e.UserState;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            m_EndJobList = new ArrayList();
        }

        void BindListBox1()
        {
            listBox1.DataSource = null;
            listBox1.DataSource = m_EndJobList;
        }

        ArrayList m_EndJobList;
    }
}
