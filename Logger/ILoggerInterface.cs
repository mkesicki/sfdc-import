using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SFDCImport.Logger
{
    interface ILoggerInterface
    {
        //public String Path { get; set; }
        void Info(String message);
        public void Warning(String message);
        public void Error(String message);
        public void Save();
        public void Close();
    }
}
