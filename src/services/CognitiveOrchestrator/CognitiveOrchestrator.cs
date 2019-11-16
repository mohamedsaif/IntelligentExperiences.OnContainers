using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CognitiveOrchestrator.Functions
{
    public static class CognitiveOrchestrator
    {
        [FunctionName("CognitiveOrchestrator")]
        public static void Run([ServiceBusTrigger("camframe", "cognitive-orchestrator", Connection = "")]string camFrameEvent, ILogger log)
        {
            log.LogInformation($"C# ServiceBus topic trigger function processed message: {camFrameEvent}");
        }
    }
}
