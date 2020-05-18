using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Models
{
    public class ResultStatus
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorDetails { get; set; }
    }
}
