using CoreLib.Abstractions;
using MaskDetector.Abstractions;
using MaskDetector.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Functions
{
    public class MaskDetector
    {
        private IMaskAnalysisRepository maskAnalysisRepo;
        private IStorageRepository filesStorageRepo;

        public MaskDetector(IMaskAnalysisRepository repo, IStorageRepository storage)
        {
            maskAnalysisRepo = repo;
            filesStorageRepo = storage;
        }

        [FunctionName("MaskDetector")]
        [return: ServiceBus(queueOrTopicName: AppConstants.SBPublishingTopic,
            entityType: Microsoft.Azure.WebJobs.ServiceBus.EntityType.Topic,
            Connection = "serviceBusConnection")]
        public async Task<string> Run(
            [ServiceBusTrigger(AppConstants.SBTopic, AppConstants.SBSubscription, Connection = "serviceBusConnection")] string request,
            ILogger log)
        {
            DateTime startTime = DateTime.UtcNow;
            log.LogInformation($"FUNC (MaskDetector): camframe-analysis topic triggered processing message: {JsonConvert.SerializeObject(request)}");

            CognitiveRequest cognitiveRequest = null;

            try
            {
                cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (MaskDetector): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(request)} with the error: {ex.Message}");
            }

            //Starting Mask Detection
            MaskAnalysis result = new MaskAnalysis
            {
                Id = $"{Guid.NewGuid()}-{cognitiveRequest.TakenAt.Month}{cognitiveRequest.TakenAt.Year}",
                Request = cognitiveRequest,
                CreatedAt = startTime,
                TimeKey = $"{cognitiveRequest.TakenAt.Month}{cognitiveRequest.TakenAt.Year}",
                IsDeleted = false,
                IsSuccessful = false,
                Origin = "MaskDetector",
                Status = ProcessingStatus.Processing.ToString()
            };

            var fileName = cognitiveRequest.FileUrl.Substring(cognitiveRequest.FileUrl.LastIndexOf("/") + 1);
            var data = await filesStorageRepo.GetFileAsync(fileName);

            return null;
        }
    }
}
