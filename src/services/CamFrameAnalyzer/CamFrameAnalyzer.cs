using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using CamFrameAnalyzer.Models;
using Newtonsoft.Json;
using CoreLib.Utils;
using CoreLib.Abstractions;
using CamFrameAnalyzer.Abstractions;
using CoreLib.Repos;

namespace CamFrameAnalyzer.Functions
{
    public class CamFrameAnalyzer
    {
        static string key = string.Empty;
        static string endpoint = string.Empty;
        static IFaceClient faceClient;
        private IStorageRepository filesStorageRepo;
        private ICamFrameAnalysisRepository camFrameAnalysisRepo;
        private IAzureServiceBusRepository serviceBusRepo;

        public CamFrameAnalyzer(IStorageRepository storageRepo, IAzureServiceBusRepository sbRepo, ICamFrameAnalysisRepository camFrameRepo)
        {
            filesStorageRepo = storageRepo;
            camFrameAnalysisRepo = camFrameRepo;
            serviceBusRepo = sbRepo;
        }

        [FunctionName("CamFrameAnalyzer")]
        public async Task Run(
            [ServiceBusTrigger(AppConstants.SBTopic, AppConstants.SBSubscription, Connection = "serviceBusConnection")]string request, 
            ILogger log)
        {
            CognitiveRequest cognitiveRequest = null;
            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered processing message: {JsonConvert.SerializeObject(cognitiveRequest)}");

            try
            {
                cognitiveRequest = JsonConvert.DeserializeObject<CognitiveRequest>(request);
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(request)} with the error: {ex.Message}");
            }

            try
            {
                key = GlobalSettings.GetKeyValue("cognitiveKey");
                endpoint = GlobalSettings.GetKeyValue("cognitiveEndpoint");

                faceClient = new FaceClient(
                        new Microsoft.Azure.CognitiveServices.Vision.Face.ApiKeyServiceClientCredentials(key),
                        new System.Net.Http.DelegatingHandler[] { })
                        { Endpoint = endpoint };

                var data = await filesStorageRepo.GetFileAsync(cognitiveRequest.FileUrl);

                var frameAnalysis = new CamFrameAnalysis
                {
                    Request = cognitiveRequest,
                    CreatedAt = DateTime.UtcNow,
                    Data = data,
                    IsDeleted = false,
                    IsSuccessfull = false,
                    Origin = "CamFrameAnalyzer",
                    Status = ProcessingStatus.Processing.ToString()
                };
             }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }

            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
        }

        public async Task<CamFrameAnalysis> FaceDetection(CamFrameAnalysis input, ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Detection");

            IList<FaceAttributeType> faceAttributes = new FaceAttributeType[]
                                           {
                                                FaceAttributeType.Gender, FaceAttributeType.Age,
                                                FaceAttributeType.Smile, FaceAttributeType.Emotion,
                                                FaceAttributeType.Glasses, FaceAttributeType.Hair
                                           };
            try
            {
                using (Stream imageFileStream = new MemoryStream(input.Data))
                {
                    // The second argument specifies to return the faceId, while
                    // the third argument specifies not to return face landmarks.
                    IList<DetectedFace> faceList =
                        await faceClient.Face.DetectWithStreamAsync(
                            imageFileStream, true, false, faceAttributes);

                    input.DetectedFaces = faceList;
                    input.IsSuccessfull = true;
                    input.Status = $"Detected Faces: {faceList.Count}";

                    log.LogInformation($"FUNC (CamFrameAnalyzer): Finished Face Detection");
                    
                    return input;
                }
            }
            // Catch and display Face API errors.
            catch (APIErrorException e)
            {
                log.LogError($"####### Failed to detect faces: {e.Message}");
                input.IsSuccessfull = false;
                input.Status = e.Message;
                return input;
            }
        }
    }
}
