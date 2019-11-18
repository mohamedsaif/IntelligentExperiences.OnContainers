using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CamFrameAnalyzer.Functions
{
    public static class CamFrameAnalyzer
    {
        static string key = string.Empty;
        static string endpoint = string.Empty;
        static ComputerVisionClient computerVision

        [FunctionName("CamFrameAnalyzer")]
        public static async Run(
            [ServiceBusTrigger("camframe-analysis", "camframe-analyzer", Connection = "SB_Connection")]string request, 
            ILogger log)
        {
            try
            {
                var cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }

            try
            {
                string key = GlobalSettings.GetKeyValue("computerVisionKey");
                string endpoint = GlobalSettings.GetKeyValue("computerVisionEndpoint");

                ComputerVisionClient computerVision = new ComputerVisionClient(
                    new Microsoft.Azure.CognitiveServices.Vision.ComputerVision.ApiKeyServiceClientCredentials(key),
                    new System.Net.Http.DelegatingHandler[] { })
                    { Endpoint = endpoint };

            var data = await filesStorageRepo.GetFileAsync(input.FileUrl);
            }
            catch (Exception ex)
            {

            }

            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
        }
        public static async Task<CognitiveStep> FaceDetectionBasic(byte[] input, ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Detection");

            var detectionResult = await computerVision.AnalyzeImageInStreamAsync(new MemoryStream(data), new List<VisualFeatureTypes> { VisualFeatureTypes.Faces });

            input.IsSuccessful = true;
            input.Confidence = detectionResult.Faces.Count > 0 ? 1 : 0;
            input.LastUpdatedAt = DateTime.UtcNow;
            input.RawOutput = JsonConvert.SerializeObject(detectionResult);

            return input;
        }
    }
}
