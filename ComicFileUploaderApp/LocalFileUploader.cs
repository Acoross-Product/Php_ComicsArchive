using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ComicFileUploader;
using System.Drawing;

namespace ComicFileUploaderApp
{
    public enum register_result
    {
        success = 0,
        err_not_zip,
        err_same_full_path,
        err_no_title_img,
    }

    class LocalComicFileUploader
    {
        public static register_result RegisterOneFile(string libDirName, FileInfo postedfile)
        {
            byte[] title_img_bytes;
            return RegisterOneFile(libDirName, postedfile, out title_img_bytes);
        }

        public const string libDirRoot = "\\\\RASPBERRYPI\\RaspberryPi\\";
        public const string libDirLocal = "/Extern/";

        public static register_result RegisterOneFile(string libDirName, FileInfo postedfile, out byte[] title_img_bytes)
        {
            Uri rootUri = new Uri(libDirRoot);
            Uri libDirUri = new Uri(libDirName + "\\");
            Uri fileDirUri = new Uri(postedfile.DirectoryName + "\\");

            Uri relativeLibUri = rootUri.MakeRelativeUri(libDirUri);

            Uri relativeFileDirUriFromLib = libDirUri.MakeRelativeUri(fileDirUri);
                        
            if (relativeFileDirUriFromLib.ToString().Length>0)
            {
                Console.WriteLine("dd");
            }

            string local_file_dir = libDirLocal + relativeLibUri.ToString() + relativeFileDirUriFromLib.ToString();
            //string local_file_path = local_file_dir  + "/" + postedfile.Name;

            title_img_bytes = null;

            string ext = Path.GetExtension(postedfile.Name).ToLower();
            if (ext != ".zip" && ext != ".rar")
            {
                return register_result.err_not_zip;
            }

            if (OneFileUploader.odbc_CheckSameFullpath(postedfile.Name, local_file_dir))
            {
                return register_result.err_same_full_path;
            }

            string title_img_ext = "";
            using (var fs = postedfile.OpenRead())
            {
                if (ext == ".zip")
                    OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, fs);
                else if (ext == ".rar")
                    OneFileUploader.ExtractTitleImgFromRar(out title_img_bytes, out title_img_ext, fs);
            }
            
            if (title_img_bytes == null)
            {
                return register_result.err_no_title_img;
            }

            string sTitle = Path.GetFileNameWithoutExtension(postedfile.Name);
            string filename = postedfile.Name;
            int i = 0;
            bool bChangeName = false;
            while (OneFileUploader.odbc_CheckSameComicsNew(filename) == true)
            {
                bChangeName = true;
                filename = sTitle + "_" + i.ToString() + ext;
                ++i;
            }

            if (bChangeName)
            {
                Logger.Add("file name changed " + postedfile.Name + " to " + filename);
                File.Move(postedfile.FullName, postedfile.DirectoryName + "\\" + filename);
            }

            int ID = -1;
            OneFileUploader.odbc_InsertNew_ComicsNew(out ID, filename, local_file_dir, postedfile.Name, (int)postedfile.Length, sTitle, title_img_bytes, title_img_ext, relativeLibUri.ToString(), relativeFileDirUriFromLib.ToString());

            return register_result.success;
        }

        static bool ImportOneFile(FileInfo postedfile)
        {
            string ext = Path.GetExtension(postedfile.Name).ToLower();
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
                OneFileUploader.ExtractTitleImgFromZip(out title_img_bytes, out title_img_ext, fs);
            }

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
    }
}
