using System;

namespace SFDCImport.Logger
{
    interface ILoggerInterface
    {
        void Info(String message);
        public void Warning(String message);
        public void Error(String message);
        public void Save();
        public void Close();
    }
}
