using System;
using System.Text;
using CognitiveOrchestrator.Models;
using CognitiveOrchestrator.Repos;
using CoreLib.Repos;
using CoreLib.Utils;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CognitiveOrchestrator.Functions
{
    public class CognitiveOrchestrator
    {
        private IAzureServiceBusRepository serviceBusRepo;
        private CamFrameAnalyzerServiceBus camFrameAnalyzerServiceBus;

        public CognitiveOrchestrator(CamFrameAnalyzerServiceBus camFrameSB)
        {
            camFrameAnalyzerServiceBus = camFrameSB;
        }

        [FunctionName("CognitiveOrchestrator")]
        public void Run(
            [ServiceBusTrigger(AppConstants.SBTopic, AppConstants.SBSubscription, Connection = "serviceBusConnection")]string request, 
            ILogger log)
        {
            var cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);

            log.LogInformation($"FUNC (CognitiveOrchestrator): cognitive-orchestrator topic triggered and processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
            
            //Based on the cognitive action requested, the relevant message will be pushed to the designated topic.
            if(cognitiveRequest.TargetAction == CognitiveTargetAction.CamFrame.ToString())
                CamFrameAnalysis(cognitiveRequest);
            else
                log.LogWarning($"FUNC (CognitiveOrchestrator): cognitive-orchestrator topic executed NO actions for message: {JsonConvert.SerializeObject(cognitiveRequest)}");

            log.LogInformation($"FUNC (CognitiveOrchestrator): cognitive-orchestrator topic completed: {JsonConvert.SerializeObject(cognitiveRequest)}");

            return;
        }

        public void CamFrameAnalysis(CognitiveRequest request)
        {
            var messageBody = JsonConvert.SerializeObject(request);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            camFrameAnalyzerServiceBus.PublishMessage(message);
        }
    }
}
