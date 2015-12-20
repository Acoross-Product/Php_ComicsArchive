using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ComicFileUploaderApp
{
    public delegate void OneDirDelegate(DirectoryInfo di);

    public class DirectoryProcess
    {
        public static void TraverseAllSubDir(DirectoryInfo di, OneDirDelegate dg)
        {
            dg(di);

            foreach (var dii in di.GetDirectories())
            {
                TraverseAllSubDir(dii, dg);
            }
        }
    }
}
