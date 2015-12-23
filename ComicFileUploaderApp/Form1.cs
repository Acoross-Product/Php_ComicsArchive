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
using System.Text.RegularExpressions;

namespace ComicFileUploaderApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            m_EndJobList = new ArrayList();
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

            backgroundWorker1.RunWorkerAsync(di);
        }
        
        ArrayList m_EndJobList;
        
        void processOneFile_end(string filename, string msg)
        {
            Logger.Add("[upload " + msg + "]" + filename);
            listBox1.Invoke((MethodInvoker)delegate
            {
                m_EndJobList.Add("[" + msg + "]:" + filename);
                BindListBox1();
                int visibleItems = listBox1.ClientSize.Height / listBox1.ItemHeight;
                listBox1.TopIndex = Math.Max(listBox1.Items.Count - visibleItems + 1, 0);
            });
        }
        
        private void processOneFile(string libDirName, FileInfo fi)
        {
            try
            {
                byte[] title_img;
                var ret = LocalComicFileUploader.RegisterOneFile(libDirName, fi, out title_img);
                if (title_img != null)
                {
                    pictureBox1.Invoke((MethodInvoker)delegate
                    {
                        using (var ms = new MemoryStream(title_img))
                        {
                            pictureBox1.Image = null;
                            pictureBox1.Image = Image.FromStream(ms);
                        }   
                    });
                }

                if (ret == register_result.success)
                {
                    processOneFile_end(fi.FullName, "success");
                }
                else
                {
                    processOneFile_end(fi.FullName, ret.ToString());
                }
            }
            catch (Exception ex)
            {
                processOneFile_end(fi.FullName, "exception");
                Logger.Add(ex.ToString());
            }
        }
        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var di = (DirectoryInfo)e.Argument;

            var files = di.GetFiles();
            var libDirName = di.FullName;

            if (!libDirName.StartsWith(LocalComicFileUploader.libDirRoot))
                return;
            
            Logger.Add(DateTime.Now.ToLongDateString());
            Logger.Add(DateTime.Now.ToLongTimeString());
            Logger.Add("upload start.");

            int filenum = 0;
            int done = 0;

            DirectoryProcess.TraverseAllSubDir(di,
                (OneDirDelegate)delegate (DirectoryInfo di2)
                {
                    filenum += di2.GetFiles().Length;
                });

            DirectoryProcess.TraverseAllSubDir(di, 
                (OneDirDelegate)delegate(DirectoryInfo di2)
                {
                    foreach (var fi in di2.GetFiles())
                    {
                        int per = 100 * done / filenum;
                        backgroundWorker1.ReportProgress(per, "[" + done.ToString() + "/" + filenum.ToString() + "]" + fi.Name);

                        processOneFile(libDirName, fi);

                        ++done;
                    }
                });

            Logger.Add("upload end.");
            backgroundWorker1.ReportProgress(100, "complete!");
            MessageBox.Show("file upload end.");
        }

        private void btRegisterSelected_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            if (openFileDialog1.FileNames.Length <= 0)
                return;

            //Logger.Add(DateTime.Now.ToLongDateString());
            //Logger.Add(DateTime.Now.ToLongTimeString());
            //Logger.Add("upload start - selected files.");

            //int filenum = openFileDialog1.FileNames.Length;
            //int done = 0;

            //foreach (var fn in openFileDialog1.FileNames)
            //{
            //    var fi = new FileInfo(fn);

            //    int per = 100 * done / filenum;
            //    backgroundWorker1.ReportProgress(per, fi.Name);

            //    processOneFile(fi);

            //    ++done;
            //}

            //Logger.Add("upload end - selected files.");
            //backgroundWorker1.ReportProgress(100, "complate!");
            //MessageBox.Show("upload end - selected files.");
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbProgress.Value = e.ProgressPercentage;
            lbProgress.Text = (string)e.UserState;
        }
        
        void BindListBox1()
        {
            listBox1.DataSource = null;
            listBox1.DataSource = m_EndJobList;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lbProgress.Text = "";
        }
    }
}
