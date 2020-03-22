using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SFDCImport.Parser
{
    interface IParserInterface
    {
        Dictionary<String, String> Row { get; set; }
        public Dictionary<string, List<string> > Header { get; set; }
        public Dictionary<string, List<string> > Relations { get; set; }

        int Success { get; set; }
        int Error { get; set; }

        public Dictionary<string, List<string> >  GetHeader();
        public Dictionary<String, String> ReadRow(); 
    }
}
