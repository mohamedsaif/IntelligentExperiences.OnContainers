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
using CognitiveServiceHelpers;
using System.Linq;

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
        private CamFrameAnalysis frameAnalysis;
        private CognitiveFacesAnalyzer cognitiveFacesAnalyzer;

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

                //We need only the filename not the FQDN in FileUrl
                var fileName = cognitiveRequest.FileUrl.Substring(cognitiveRequest.FileUrl.LastIndexOf("/") + 1);

                var data = await filesStorageRepo.GetFileAsync(fileName);

                frameAnalysis = new CamFrameAnalysis
                {
                    Request = cognitiveRequest,
                    CreatedAt = DateTime.UtcNow,
                    Data = data,
                    IsDeleted = false,
                    IsSuccessfull = false,
                    Origin = "CamFrameAnalyzer",
                    Status = ProcessingStatus.Processing.ToString()
                };

                cognitiveFacesAnalyzer = new CognitiveFacesAnalyzer(frameAnalysis.Data);

                //First we detect the faces in the image
                await DetectFaces(log);
                if (frameAnalysis.IsSuccessfull)
                {
                    //Second, we take the detected list and compare it to similar and identified persons lists
                }
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }



            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and processed message: {JsonConvert.SerializeObject(cognitiveRequest)}");
        }

        public async Task DetectFaces(ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Detection");

            try
            {
                await cognitiveFacesAnalyzer.DetectFacesAsync(detectFaceAttributes: true);
                if (cognitiveFacesAnalyzer.DetectedFaces == null || !cognitiveFacesAnalyzer.DetectedFaces.Any())
                {
                    log.LogWarning($"FUNC (CamFrameAnalyzer): No faces detected");
                    frameAnalysis.DetectedFaces = null;
                    frameAnalysis.IsSuccessfull = false;
                    frameAnalysis.Status = "No Detected Faces";
                }
                else
                {
                    log.LogInformation($"FUNC (CamFrameAnalyzer): Finished Face Detection");
                    frameAnalysis.DetectedFaces = cognitiveFacesAnalyzer.DetectedFaces;
                    frameAnalysis.IsSuccessfull = false;
                    frameAnalysis.Status = $"Detected Faces";
                }
            }
            // Catch and display Face API errors.
            catch (Exception e)
            {
                log.LogError($"####### Failed to detect faces: {e.Message}");
                frameAnalysis.DetectedFaces = null;
                frameAnalysis.IsSuccessfull = false;
                frameAnalysis.Status = "Failed to detect faces";
            }
        }

        private async Task DetectSimilarFacesAsync(ILogger log)
        {
            await e.FindSimilarPersistedFacesAsync();

            if (!e.SimilarFaceMatches.Any())
            {
                this.lastSimilarPersistedFaceSample = null;
            }
            else
            {
                this.lastSimilarPersistedFaceSample = e.SimilarFaceMatches;
            }
        }
    }
}
