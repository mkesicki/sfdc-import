using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SFDCImport.Parser
{
    interface IParserInterface
    {
        Dictionary<String, String> Row { get; set; }

        Dictionary<String, String> Header { get; set; }

        List<String> Columns { get; set; }

        int Success { get; set; }

        int Error { get; set; }

        int Parsed { get { return Success + Error; } }

        int RowsParsed { get; set; }

        String Path { get; set; }
        public void OpenFile(String Path) {

            if (!File.Exists(Path)) {
                throw new FileNotFoundException("File to parse: {0} not found!", Path);
            }
            this.Path = Path;
        }
        public void GetHeader() { }
        public Dictionary<String, String> ReadRow() {

            return Row;
        }
    }
}
