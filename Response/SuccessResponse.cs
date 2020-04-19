using System;
using System.Collections.Generic;
using System.Text;

namespace SFDCImport.Response
{
    public class ResultSuccess
    {
        public string referenceId { get; set; }
        public string id { get; set; }
    }

    public class SuccessResponse
    {
        public bool hasErrors { get; set; }
        public List<ResultSuccess> results { get; set; }
    }
}
