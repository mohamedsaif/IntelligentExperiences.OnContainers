using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CrowdAnalyzer.Functions
{
    public static class CrowdAnalyzer
    {
        [FunctionName("CrowdAnalyzer")]
        public static void Run(
            [ServiceBusTrigger("crowd-analysis", "crowd-analyzer", Connection = "serviceBusConnection")]string request, 
            ILogger log)
        {
            DateTime startTime = DateTime.UtcNow;
            log.LogInformation($"FUNC (CrowdAnalyzer): crowd-analysis topic triggered processing message: {JsonConvert.SerializeObject(request)}");

            
        }
    }
}
