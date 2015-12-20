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

        private void btRegister_Click(object sender, EventArgs e)
        {
            string rootPath = textBox1.Text;
            if (Directory.Exists(rootPath))
            {
                folderBrowserDialog1.SelectedPath = rootPath;
            }

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
                return;

            string Dir = folderBrowserDialog1.SelectedPath;
            var di = new System.IO.DirectoryInfo(Dir);
            if (!di.Exists)
                return;

            foreach(var d in di.GetDirectories())
            {
                Console.WriteLine(d.FullName);
            }

            backgroundWorker1.RunWorkerAsync(di);
        }

        // import 버튼 클릭
        private void btImport_Click(object sender, EventArgs e)
        {
            //if (openFileDialog1.ShowDialog() != DialogResult.OK)
            //    return;

            //if (openFileDialog1.FileNames.Length <= 0)
            //    return;

            //string Dir = openFileDialog1.FileName.TrimEnd(openFileDialog1.SafeFileName.ToCharArray());

            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK)
                return;

            string Dir = folderBrowserDialog1.SelectedPath;
            var di = new System.IO.DirectoryInfo(Dir);
            if (!di.Exists)
                return;

            backgroundWorker1.RunWorkerAsync(di);
        }
        
        static bool RegisterOneFile(FileInfo postedfile)
        {
            string ext = Path.GetExtension(postedfile.Name);
            if (ext != ".zip")
            {
                return false;
            }

            byte[] title_img_bytes;
            string title_img_ext;
            using (var fs = postedfile.OpenRead())
            {
                OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, fs);
            }

            if (title_img_bytes == null)
            {
                return false;
            }

            string sTitle = Path.GetFileNameWithoutExtension(postedfile.Name);
            string filename = postedfile.Name;
            int i = 0;
            while (OneFileUploader.odbc_CheckSameComicsNew(filename) == true)
            {
                filename = sTitle + "_" + i.ToString() + ext;
            }

            File.Move(postedfile.FullName, postedfile.DirectoryName + "\\" + filename);

            int ID = -1;
            OneFileUploader.odbc_InsertNew_ComicsNew(out ID, filename, postedfile.DirectoryName, postedfile.Name, sTitle, title_img_bytes, title_img_ext);

            return true;
        }

        static bool ImportOneFile(FileInfo postedfile)
        {
            string ext = Path.GetExtension(postedfile.Name);
            if (ext != ".zip")
            {
                return false;
            }

            byte[] filebytes = new byte[(int)postedfile.Length];
            byte[] title_img_bytes;
            string title_img_ext;
            using (var fs = postedfile.OpenRead())
            {
                fs.Read(filebytes, 0, (int)postedfile.Length);
            }

            OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, filebytes);

            if (title_img_bytes == null)
            {
                return false;
            }

            string sTitle = Path.GetFileNameWithoutExtension(postedfile.Name);
            int retId = OneFileUploader.odbc_InsertNewComics(sTitle);
            if (retId < 0)
            {
                return false;
            }

            OneFileUploader.odbc_SaveTitleImg(retId, title_img_bytes, title_img_ext);

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

        void ProcessFileUploadEnd(string filename, string msg)
        {
            Logger.Add("[upload " + msg + "]" + filename);
            listBox1.Invoke((MethodInvoker)delegate
            {
                m_EndJobList.Add("[" + msg + " ]:" + filename);
                BindListBox1();
            });
        }

        delegate void OneDirDelegate(DirectoryInfo di);

        static void TraverseAllSubDir(DirectoryInfo di, OneDirDelegate dg)
        {
            dg(di);

            foreach (var dii in di.GetDirectories())
            {
                TraverseAllSubDir(dii, dg);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var di = (DirectoryInfo)e.Argument;

            var files = di.GetFiles();

            Logger.Add(DateTime.Now.ToLongDateString());
            Logger.Add(DateTime.Now.ToLongTimeString());
            Logger.Add("upload start.");

            int filenum = 0;
            int done = 0;

            TraverseAllSubDir(di,
                (OneDirDelegate)delegate (DirectoryInfo di2)
                {
                    filenum += di2.GetFiles().Length;
                });

            TraverseAllSubDir(di, 
                (OneDirDelegate)delegate(DirectoryInfo di2)
                {
                    foreach (var fi in di2.GetFiles())
                    {
                        int per = 100 * done / filenum;
                        backgroundWorker1.ReportProgress(per, fi.Name);

                        try
                        {
                            //if (ImportOneFile(fi))
                            if (RegisterOneFile(fi))
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
                });

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
