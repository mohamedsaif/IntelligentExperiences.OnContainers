using System;
using CognitiveOrchestrator.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CognitiveOrchestrator.Functions
{
    public static class CognitiveOrchestrator
    {
        [FunctionName("CognitiveOrchestrator")]
        public static void Run(
            [ServiceBusTrigger("cognitive-request", "cognitive-orchestrator", Connection = "SB_Connection")]string request, 
            ILogger log)
        {
            var cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);

            log.LogInformation($"cognitive-orchestrator topic trigger function processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
            if(cognitiveRequest.TargetAction == CognitiveTargetAction.CamFrame.ToString())
                CamFrameAnalysis(cognitiveRequest);
            else
                return;
            
            return;
        }

        public static void CamFrameAnalysis(CognitiveRequest request)
        {
            
        }
    }
}
