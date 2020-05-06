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
        //private IVisitorsRepository visitorRepo;
        private IAzureServiceBusRepository serviceBusRepo;
        private CamFrameAnalysis frameAnalysis;
        private CognitiveFacesAnalyzer cognitiveFacesAnalyzer;

        public CamFrameAnalyzer(
            IStorageRepository storageRepo, 
            IAzureServiceBusRepository sbRepo, 
            ICamFrameAnalysisRepository camFrameRepo)
        {
            filesStorageRepo = storageRepo;
            camFrameAnalysisRepo = camFrameRepo;
            serviceBusRepo = sbRepo;
        }

        [FunctionName("CamFrameAnalyzer")]
        [return: ServiceBus(queueOrTopicName: AppConstants.SBTopicCrowdAnalysis, 
            entityType: Microsoft.Azure.WebJobs.ServiceBus.EntityType.Topic, 
            Connection = "serviceBusConnection")]
        public async Task<string> Run(
            [ServiceBusTrigger(AppConstants.SBTopic, AppConstants.SBSubscription, Connection = "serviceBusConnection")]string request,
            ILogger log)
        {
            DateTime startTime = DateTime.UtcNow;
            log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered processing message: {JsonConvert.SerializeObject(request)}");
            
            CognitiveRequest cognitiveRequest = null;

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
                faceWorkspaceDataFilter = GlobalSettings.GetKeyValue("faceWorkspaceDataFilter");

                FaceServiceHelper.ApiKey = key;
                FaceServiceHelper.ApiEndpoint = endpoint;
                FaceListManager.FaceListsUserDataFilter = faceWorkspaceDataFilter;

                frameAnalysis = new CamFrameAnalysis
                {
                    Id = $"{Guid.NewGuid()}-{cognitiveRequest.TakenAt.Month}{cognitiveRequest.TakenAt.Year}",
                    Request = cognitiveRequest,
                    CreatedAt = startTime,
                    TimeKey = $"{cognitiveRequest.TakenAt.Month}{cognitiveRequest.TakenAt.Year}",
                    IsDeleted = false,
                    IsSuccessful = false,
                    Origin = "CamFrameAnalyzer",
                    Status = ProcessingStatus.Processing.ToString()
                };

                // Get image data. We need only the filename not the FQDN in FileUrl
                var fileName = cognitiveRequest.FileUrl.Substring(cognitiveRequest.FileUrl.LastIndexOf("/") + 1);
                var data = await filesStorageRepo.GetFileAsync(fileName);

                // Load the analyzer with data
                CognitiveFacesAnalyzer.PeopleGroupsUserDataFilter = faceWorkspaceDataFilter;
                cognitiveFacesAnalyzer = new CognitiveFacesAnalyzer(data);
                
                await AnalyzeCameFrame(log);
                
                UpdateAnalysisSummary();
                
                frameAnalysis.TotalProcessingTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                await SaveAnalysisAsync();

                log.LogInformation($"FUNC (CamFrameAnalyzer): camframe-analysis COMPLETED: {JsonConvert.SerializeObject(frameAnalysis)}");

                //Only publish a new analysis when face detection was successful with faces
                if (frameAnalysis.IsSuccessful)
                    return JsonConvert.SerializeObject(frameAnalysis);
                else
                    return null;
            }
            catch (Exception ex)
            {
                log.LogError($"FUNC (CamFrameAnalyzer): camframe-analysis topic triggered and failed to parse message: {JsonConvert.SerializeObject(cognitiveRequest)} with the error: {ex.Message}");
            }

            return null;
        }

        private async Task AnalyzeCameFrame(ILogger log)
        {
            //First we detect the faces in the image
            await DetectFacesAsync(log);
            if (frameAnalysis.IsDetectionSuccessful)
            {
                //Second, we take the detected list and compare it to similar and identified persons lists
                await Task.WhenAll(IdentifyFacesAsync(log), this.DetectSimilarFacesAsync(log));

                //validate that everything went well :)
                if (frameAnalysis.IsSimilaritiesSuccessful && frameAnalysis.IsIdentificationSuccessful)
                {
                    frameAnalysis.IsSuccessful = true;
                    frameAnalysis.Status = ProcessingStatus.Successful.ToString();
                }
                else
                {
                    frameAnalysis.IsSuccessful = false;
                    frameAnalysis.Status = ProcessingStatus.PartiallySuccessful.ToString();
                }
            }
            else
            {
                frameAnalysis.IsSuccessful = false;
                frameAnalysis.Status += "|" + ProcessingStatus.Failed.ToString();

                log.LogError($"FUNC (CamFrameAnalyzer): Detection failed: {JsonConvert.SerializeObject(frameAnalysis)}");
            }
        }

        public async Task DetectFacesAsync(ILogger log)
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
                frameAnalysis.Status = $"CamFrameAnalyzer ERROR: Failed to detect faces ({e.Message})";
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

                log.LogInformation($"FUNC (CamFrameAnalyzer): Finished Similarities Detection");
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
                    frameAnalysis.IdentifiedPersons = cognitiveFacesAnalyzer.DetectedFaces.Select(f => new Tuple<DetectedFace, IdentifiedPerson>(f, cognitiveFacesAnalyzer.IdentifiedPersons.FirstOrDefault(p => p.FaceId == f.FaceId)));
                    frameAnalysis.IsIdentificationSuccessful = true;
                    frameAnalysis.Status = "Identified Persons";
                }

                log.LogInformation($"FUNC (CamFrameAnalyzer): Finished Identification Detection");
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

        private void UpdateAnalysisSummary()
        {
            if(frameAnalysis.DetectedFaces != null)
            {
                foreach(var face in frameAnalysis.DetectedFaces)
                {
                    frameAnalysis.Summary.TotalDetectedFaces++;
                    AgeDistribution genderBasedAgeDistribution = null;
                    if (face.FaceAttributes.Gender == Gender.Male)
                    {
                        frameAnalysis.Summary.TotalMales++;
                        genderBasedAgeDistribution = frameAnalysis.Summary.AgeGenderDistribution.MaleDistribution;
                    }
                    else
                    {
                        frameAnalysis.Summary.TotalFemales++;
                        genderBasedAgeDistribution = frameAnalysis.Summary.AgeGenderDistribution.FemaleDistribution;
                    }

                    if (face.FaceAttributes.Age < 16)
                    {
                        genderBasedAgeDistribution.Age0To15++;
                    }
                    else if (face.FaceAttributes.Age < 20)
                    {
                        genderBasedAgeDistribution.Age16To19++;
                    }
                    else if (face.FaceAttributes.Age < 30)
                    {
                        genderBasedAgeDistribution.Age20s++;
                    }
                    else if (face.FaceAttributes.Age < 40)
                    {
                        genderBasedAgeDistribution.Age30s++;
                    }
                    else if (face.FaceAttributes.Age < 50)
                    {
                        genderBasedAgeDistribution.Age40s++;
                    }
                    else if (face.FaceAttributes.Age < 60)
                    {
                        genderBasedAgeDistribution.Age50s++;
                    }
                    else
                    {
                        genderBasedAgeDistribution.Age60sAndOlder++;
                    }
                }
            }
        }

        private async Task SaveAnalysisAsync()
        {
            await camFrameAnalysisRepo.AddAsync(frameAnalysis);
        }
    }
}
