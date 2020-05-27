using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Models
{
    public class BaseResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorDetails { get; set; }
    }
}
