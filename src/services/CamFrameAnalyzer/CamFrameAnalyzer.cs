using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using CamFrameAnalyzer.Models;
using Newtonsoft.Json;
using CoreLib.Utils;
using CoreLib.Abstractions;
using CamFrameAnalyzer.Abstractions;
using CoreLib.Repos;
using CognitiveServiceHelpers;
using System.Linq;
using CognitiveServiceHelpers.Models;

namespace CamFrameAnalyzer.Functions
{
    public class CamFrameAnalyzer
    {
        static string key = string.Empty;
        static string endpoint = string.Empty;
        private string faceWorkspaceDataFilter;

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
            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered processing message: {JsonConvert.SerializeObject(request)}");

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
                DateTime startTime = DateTime.UtcNow;

                key = GlobalSettings.GetKeyValue("cognitiveKey");
                endpoint = GlobalSettings.GetKeyValue("cognitiveEndpoint");
                faceWorkspaceDataFilter = GlobalSettings.GetKeyValue("faceWorkspaceDataFilter");

                FaceServiceHelper.ApiKey = key;
                FaceServiceHelper.ApiEndpoint = endpoint;
                FaceListManager.FaceListsUserDataFilter = faceWorkspaceDataFilter;

                //We need only the filename not the FQDN in FileUrl
                var fileName = cognitiveRequest.FileUrl.Substring(cognitiveRequest.FileUrl.LastIndexOf("/") + 1);

                var data = await filesStorageRepo.GetFileAsync(fileName);

                frameAnalysis = new CamFrameAnalysis
                {
                    Request = cognitiveRequest,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false,
                    IsSuccessful = false,
                    Origin = "CamFrameAnalyzer",
                    Status = ProcessingStatus.Processing.ToString()
                };

                cognitiveFacesAnalyzer = new CognitiveFacesAnalyzer(data);

                //First we detect the faces in the image
                await DetectFaces(log);
                if (frameAnalysis.IsDetectionSuccessful)
                {
                    //Second, we take the detected list and compare it to similar and identified persons lists
                    await Task.WhenAll(IdentifyFacesAsync(log), this.DetectSimilarFacesAsync(log));

                    //validate that everything went well :)
                    if(frameAnalysis.IsSimilaritiesSuccessful && frameAnalysis.IsIdentificationSuccessful)
                    {
                        frameAnalysis.IsSuccessful = true;
                        frameAnalysis.Status = ProcessingStatus.Successful.ToString();
                        frameAnalysis.TotalProcessingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    }
                    else
                    {
                        frameAnalysis.IsSuccessful = false;
                        frameAnalysis.Status = ProcessingStatus.PartiallySuccessful.ToString();
                        frameAnalysis.TotalProcessingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    }
                }
                else
                {
                    frameAnalysis.IsSuccessful = false;
                    frameAnalysis.Status = ProcessingStatus.Failed.ToString();
                    frameAnalysis.TotalProcessingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    log.LogError($"FUNC (CamFrameAnalyzer): Detection failed: {JsonConvert.SerializeObject(frameAnalysis)}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }



            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and processed message: {JsonConvert.SerializeObject(frameAnalysis)}");
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
                    frameAnalysis.IsDetectionSuccessful = false;
                    frameAnalysis.Status = "No Detected Faces";
                }
                else
                {
                    log.LogInformation($"FUNC (CamFrameAnalyzer): Finished Face Detection");
                    frameAnalysis.DetectedFaces = cognitiveFacesAnalyzer.DetectedFaces;
                    frameAnalysis.IsDetectionSuccessful = true;
                    frameAnalysis.Status = $"Detected Faces";
                }
            }
            // Catch and display Face API errors.
            catch (Exception e)
            {
                log.LogError($"####### Failed to detect faces: {e.Message}");
                frameAnalysis.DetectedFaces = null;
                frameAnalysis.IsDetectionSuccessful = false;
                frameAnalysis.IsSuccessful = false;
                frameAnalysis.Status = "Failed to detect faces";
            }
        }

        private async Task DetectSimilarFacesAsync(ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Similarities Detection");
            
            try
            {
                await cognitiveFacesAnalyzer.FindSimilarPersistedFacesAsync();

                if (!cognitiveFacesAnalyzer.SimilarFaceMatches.Any())
                {
                    log.LogWarning($"FUNC (CamFrameAnalyzer): No similarities detected");
                    frameAnalysis.SimilarFaces = null;
                    frameAnalysis.IsSimilaritiesSuccessful = true;
                    frameAnalysis.Status = "No similarities detected";
                }
                else
                {
                    frameAnalysis.SimilarFaces = cognitiveFacesAnalyzer.SimilarFaceMatches;
                    frameAnalysis.IsSimilaritiesSuccessful = true;
                    frameAnalysis.Status = "Detected Similarities";
                }
            }
            catch (Exception e)
            {

                log.LogError($"####### Failed to find similar faces: {e.Message}");
                frameAnalysis.SimilarFaces = null;
                frameAnalysis.IsSimilaritiesSuccessful = false;
                frameAnalysis.IsSuccessful = false;
                frameAnalysis.Status = "Failed to find similar faces";
            }
        }

        private async Task IdentifyFacesAsync(ILogger log)
        {
            log.LogInformation($"FUNC (CamFrameAnalyzer): Starting Face Identification");

            try
            {
                await cognitiveFacesAnalyzer.IdentifyFacesAsync();

                if (!cognitiveFacesAnalyzer.IdentifiedPersons.Any())
                {
                    log.LogWarning($"FUNC (CamFrameAnalyzer): No identified persons detected");
                    frameAnalysis.IdentifiedPersons = null;
                    frameAnalysis.IsIdentificationSuccessful = true;
                    frameAnalysis.Status = "No identified persons detected";
                }
                else
                {
                    log.LogWarning($"FUNC (CamFrameAnalyzer): Identified persons detected");
                    frameAnalysis.IdentifiedPersons = cognitiveFacesAnalyzer.DetectedFaces.Select(f => new Tuple<DetectedFace, IdentifiedPerson>(f, cognitiveFacesAnalyzer.IdentifiedPersons.FirstOrDefault(p => p.FaceId == f.FaceId)));
                    frameAnalysis.IsIdentificationSuccessful = true;
                    frameAnalysis.Status = "Identified Persons";
                }
            }
            catch (Exception e)
            {
                log.LogError($"####### Failed to identify faces: {e.Message}");
                frameAnalysis.IdentifiedPersons = null;
                frameAnalysis.IsIdentificationSuccessful = false;
                frameAnalysis.IsSuccessful = false;
                frameAnalysis.Status = "Failed to identify faces";
            }
        }
    }
}
