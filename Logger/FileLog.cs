using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SFDCImport.Logger
{
    class FileLog : ILoggerInterface
    {
        public string Path { get; set; }
        public FileStream Log {get; set;}

        public FileLog(String Dir, String Filename)
        {
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            Path = Dir + System.IO.Path.DirectorySeparatorChar + Filename;

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }

            Log = File.Create(Path);
        }

        public void Info(string message)
        {
            String s = String.Format("![#000000] {0}: {1} \n", DateTime.Now, message);
            AddMessage(s);
        }
        public void Warning(string message)
        {
            String s = String.Format("![#c5f015] {0}: {1} \n", DateTime.Now, message);
            AddMessage(s);
        }

        public void Error(string message)
        {
            String s = String.Format("![#f03c15] {0}: {1} \n", DateTime.Now, message);
            AddMessage(s);
        }

        public void Save()
        {
            Log.Close();
        }

        public void Close()
        {
            Save();
        }

        private void AddMessage(String Message) {

            using (FileStream fs = Log)
            {
                byte[] info = new UTF8Encoding(true).GetBytes(Message);
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }
        }
    }
}
