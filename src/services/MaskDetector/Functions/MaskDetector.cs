using CoreLib.Abstractions;
using MaskDetector.Abstractions;
using MaskDetector.Models;
using MaskDetector.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Functions
{
    public class MaskDetector
    {
        private IMaskAnalysisRepository maskAnalysisRepo;
        private IStorageRepository filesStorageRepo;
        private IMaskDetectionAPI maskDetectionApi;

        public MaskDetector(IMaskAnalysisRepository repo, IStorageRepository storage, IMaskDetectionAPI maskApi)
        {
            maskAnalysisRepo = repo;
            filesStorageRepo = storage;
            maskDetectionApi = maskApi;
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
            var imageStream = new MemoryStream(data);

            try
            {
                var detectionResult = await maskDetectionApi.DetectImage(new StreamPart(imageStream, "image.jpg", "multipart/form-data"));
                result.DetectionResult = detectionResult;
                result.DetectionResult.Predictions = detectionResult.Predictions.Where(p => p.Probability >= AppConstants.MaskDetectionThreshold).ToList();
                result.IsSuccessful = true;
                result.Status = ProcessingStatus.Successful.ToString();
                result.TotalDetected = detectionResult.Predictions.Where(p => p.Probability >= AppConstants.MaskDetectionThreshold).Count();
                result.TotalDetectedWithMasks = detectionResult.Predictions.Where(p => p.TagName == "MASK" && p.Probability >= AppConstants.MaskDetectionThreshold).Count();
                result.TotalDetectedWithoutMasks = detectionResult.Predictions.Where(p => p.TagName == "NOMASK" && p.Probability >= AppConstants.MaskDetectionThreshold).Count();
                result.MaskDetectionThreshold = AppConstants.MaskDetectionThreshold;
                result.TotalProcessingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                var dbResult = await maskAnalysisRepo.AddAsync(result);
                return JsonConvert.SerializeObject(dbResult);
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (MaskDetector): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }

            return null;
        }
    }
}
