using System;
using System.Text;
using System.IO;

namespace SFDCImport.Logger
{
    class FileLogerr : ILoggerInterface
    {
        public FileStream LogSuccess { get; set; }
        public FileStream LogError { get; set; }

        private readonly object LogSuccessLock = new object();
        private readonly object LogErrorLock = new object();

        public FileLogerr(String Dir)
        {
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            String PathSuccess = Dir + System.IO.Path.DirectorySeparatorChar + "success.md";
            String PathError = Dir + System.IO.Path.DirectorySeparatorChar + "error.md";

            if (File.Exists(PathSuccess))
            {
                File.Delete(PathSuccess);
            }

            if (File.Exists(PathError))
            {
                File.Delete(PathError);
            }

            LogSuccess = File.Create(PathSuccess);
            LogError = File.Create(PathError);

            Console.WriteLine("Log file: {0} created", PathSuccess);
            Console.WriteLine("Log file: {0} created", PathError);
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
            AddError(s);
        }

        public void Save()
        {
            LogSuccess.Close();
            LogError.Close();
        }

        public void Close()
        {
            Save();
        }

        private void AddMessage(String Message) {

            lock (LogSuccessLock)
            {
                byte[] info = new UTF8Encoding(true).GetBytes(Message);
                LogSuccess.Write(info, 0, info.Length);
            }
        }

        private void AddError(String Message)
        {
            lock (LogErrorLock)
            {
                byte[] info = new UTF8Encoding(true).GetBytes(Message);
                LogError.Write(info, 0, info.Length);
            }
        }
    }
}
