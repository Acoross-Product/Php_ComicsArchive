using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ComicFileUploaderApp
{
    class Logger
    {
        public static void Add(string log)
        {
            using (var file = File.Open(".\\comic_upload_error.txt", FileMode.Append))
            {
                using (TextWriter writeFile = new StreamWriter(file))
                {
                    writeFile.WriteLine(log);
                }
            }
        }
    }
}
