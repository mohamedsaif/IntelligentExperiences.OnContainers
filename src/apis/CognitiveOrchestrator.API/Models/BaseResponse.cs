using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Models
{
    /// <summary>
    /// Represent default response to APIs that don't return a specific type
    /// </summary>
    public class BaseResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorDetails { get; set; }
    }
}
