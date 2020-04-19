using System;
using System.Collections.Generic;
using System.Text;

namespace SFDCImport.Response
{
    public class Error
    {
        public string statusCode { get; set; }
        public string message { get; set; }
        public List<string> fields { get; set; }
    }

    public class ResultError
    {
        public string referenceId { get; set; }
        public List<Error> errors { get; set; }
    }

    public class ErrorResponse
    {
        public bool hasErrors { get; set; }
        public List<ResultError> results { get; set; }
    }
}
