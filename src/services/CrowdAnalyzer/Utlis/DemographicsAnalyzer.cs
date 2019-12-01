using CognitiveServiceHelpers.Models;
using CoreLib.Utils;
using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Utils
{
    public class DemographicsAnalyzer
    {
        private CamFrameAnalysis frameAnalysis;
        private IVisitorsRepository visitorRepo;
        private ICrowdDemographicsRepository crowdDemographicsRepo;
        private string origin;
        private ILogger log;
        public CrowdDemographics Demographics = null;

        public DemographicsAnalyzer(CamFrameAnalysis analysis,
            IVisitorsRepository vRepo,
            ICrowdDemographicsRepository cRepo,
            ILogger logger)
        {
            frameAnalysis = analysis;
            visitorRepo = vRepo;
            crowdDemographicsRepo = cRepo;
            origin = GlobalSettings.GetKeyValue("origin");
            log = logger;
        }
        public async Task<bool> UpdateDemographics()
        {
            var start = DateTime.UtcNow;

            //A flag to know if we would update/insert the demographics record
            bool isNewDemographics = false;

            //A flag to know that and updated demographics are needed
            bool isDemographicsChanged = false;

            var analysisTakenAt = frameAnalysis.Request.TakenAt;

            //As the system designed to handle multiple devices, analysis also should be conducted in 
            //the context of DeviceId
            var deviceId = frameAnalysis.Request.DeviceId;

            //We need to retain our analysis relevant to the frame taken date
            var visitDate = frameAnalysis.Request.TakenAt;

            // aggregate window start from begging of the hour and continue aggregating till 59min:59sec.
            var demographicsWindowMins = double.Parse(GlobalSettings.GetKeyValue("demographicsWindowMins")); //Default to 60mins
            var currentAggregateWindowStart = new DateTime(analysisTakenAt.Year, analysisTakenAt.Month, analysisTakenAt.Day, analysisTakenAt.Hour, 0, 0);
            var currentAggregateWindowEnd = currentAggregateWindowStart.AddMinutes(demographicsWindowMins);
            //var currentTime = DateTime.UtcNow;
            
            var passedTimeOfCurrentWindowMins = (currentAggregateWindowEnd - currentAggregateWindowStart).TotalMinutes;
            string crowdDemographicsId = null;

            if (passedTimeOfCurrentWindowMins > demographicsWindowMins)
            {
                // Move the demographics to the next window
                currentAggregateWindowStart = currentAggregateWindowStart.AddMinutes(demographicsWindowMins);
                currentAggregateWindowEnd = currentAggregateWindowStart.AddMinutes(demographicsWindowMins);
            }
            
            crowdDemographicsId = $"{currentAggregateWindowStart.ToString("yyyy-MM-dd-HH-mm-ss")}|{currentAggregateWindowEnd.ToString("yyyy-MM-dd-HH-mm-ss")}:{deviceId}";
            
            try
            {
                Demographics = await crowdDemographicsRepo.GetByIdAsync(crowdDemographicsId);
            }
            catch (Exception ex)
            {
                if (ex.Message == "Entity not found")
                    Demographics = null;
                else
                    throw;
            }

            if(Demographics == null)
            {
                isNewDemographics = true;
                isDemographicsChanged = true;
                Demographics = new CrowdDemographics
                {
                    Id = crowdDemographicsId,
                    DeviceId = deviceId,
                    WindowStart = currentAggregateWindowStart,
                    WindowEnd = currentAggregateWindowEnd,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow,
                    Origin = origin,
                    IsDeleted = false
                };
            }

            if (frameAnalysis.SimilarFaces != null)
            {
                // Update the Visitor collection (either add new entry or update existing)
                log.LogWarning($"FUNC (CrowdAnalyzer): ({frameAnalysis.SimilarFaces.Count()}) detected faces is being processed.");
                foreach (var item in frameAnalysis.SimilarFaces)
                {
                    //Do we have that visitor in our db
                    Guid persistedFaceId = item.SimilarPersistedFace.PersistedFaceId.GetValueOrDefault();

                    Visitor visitor = null;
                    try
                    {
                        visitor = await visitorRepo.GetByIdAsync($"{persistedFaceId}:{item.Face.FaceAttributes.Gender}");
                    }
                    catch (Exception ex)
                    {

                        if (ex.Message == "Entity not found")
                            visitor = null;
                        else
                            throw;
                    }
                    
                    //Visitor exists:
                    if (visitor != null)
                    {
                        //Check if the visit was already recorded before (new date is within the visit window)
                        var lastVisit = visitor.LastVisits.Where(v => v.DetectedOnDeviceId == frameAnalysis.Request.DeviceId).FirstOrDefault();
                        
                        if (lastVisit != null)
                        {
                            if (!(lastVisit.VisitDate >= currentAggregateWindowStart && lastVisit.VisitDate <= currentAggregateWindowEnd))
                            {
                                //New visit of a returning customer :) update the counts
                                visitor.VisitsCount++;
                                lastVisit.Count++;
                                lastVisit.VisitDate = analysisTakenAt;

                                //This means also that the demographic for this window is changed as well.
                                isDemographicsChanged = true;
                                await visitorRepo.UpdateAsync(visitor);
                            }
                        }
                        else
                        {
                            visitor.VisitsCount++;
                            var newVisit = new Visit
                            {
                                Count = 1,
                                DetectedOnDeviceId = deviceId,
                                VisitDate = analysisTakenAt
                            };
                            visitor.LastVisits.Add(newVisit);
                            isDemographicsChanged = true;
                            await visitorRepo.UpdateAsync(visitor);
                        }
                    }
                    //New Visitor
                    else
                    {
                        isDemographicsChanged = true;

                        visitor = new Visitor
                        {
                            Id = $"{persistedFaceId}:{item.Face.FaceAttributes.Gender}",
                            VisitsCount = 1,
                            Age = (int)item.Face.FaceAttributes.Age,
                            AgeGroup = GetAgeGroupDescription((int)item.Face.FaceAttributes.Age),
                            Gender = item.Face.FaceAttributes.Gender.ToString(),
                            CreatedAt = DateTime.UtcNow,
                            IsDeleted = false,
                            Origin = origin,
                            LastVisits = new List<Visit> 
                            { 
                                new Visit { 
                                    Count = 1,
                                    DetectedOnDeviceId = deviceId, 
                                    VisitDate = analysisTakenAt 
                                } 
                            }
                        };

                        await visitorRepo.AddAsync(visitor);
                        isDemographicsChanged = true;
                        if (item.Face.FaceAttributes.Gender == Gender.Male)
                            Demographics.TotalNewMaleVisitors++;
                        else
                            Demographics.TotalNewFemaleVisitors++;
                    }

                    if (isDemographicsChanged)
                        UpdateVisitorDemographics(item);
                }

                Demographics.TotalVisitors = Demographics.TotalMales + Demographics.TotalFemales;
                Demographics.TotalProcessingTime = (int)(DateTime.UtcNow - start).TotalMilliseconds;

                if (isNewDemographics)
                {
                    await crowdDemographicsRepo.AddAsync(Demographics);
                    return true;
                }
                else if (isDemographicsChanged)
                {
                    await crowdDemographicsRepo.UpdateAsync(Demographics);
                    return true;
                }
            }
            //No faces in the analysis request
            else
            {
                log.LogWarning($"FUNC (CrowdAnalyzer): crowd-analysis found no faces in the analysis request");
            }

            return false;
        }

        public void UpdateVisitorDemographics(SimilarFaceMatch item)
        {
            AgeDistribution genderBasedAgeDistribution = null;
            if (item.Face.FaceAttributes.Gender == Gender.Male)
            {
                Demographics.TotalMales++;
                genderBasedAgeDistribution = Demographics.AgeGenderDistribution.MaleDistribution;
            }
            else
            {
                Demographics.TotalFemales++;
                genderBasedAgeDistribution = Demographics.AgeGenderDistribution.FemaleDistribution;
            }

            if (item.Face.FaceAttributes.Age < 16)
            {
                genderBasedAgeDistribution.Age0To15++;
            }
            else if (item.Face.FaceAttributes.Age < 20)
            {
                genderBasedAgeDistribution.Age16To19++;
            }
            else if (item.Face.FaceAttributes.Age < 30)
            {
                genderBasedAgeDistribution.Age20s++;
            }
            else if (item.Face.FaceAttributes.Age < 40)
            {
                genderBasedAgeDistribution.Age30s++;
            }
            else if (item.Face.FaceAttributes.Age < 50)
            {
                genderBasedAgeDistribution.Age40s++;
            }
            else if (item.Face.FaceAttributes.Age < 60)
            {
                genderBasedAgeDistribution.Age50s++;
            }
            else
            {
                genderBasedAgeDistribution.Age60sAndOlder++;
            }

            //updating emotions as well:
            UpdateVisitorEmotions(item);
        }

        public void UpdateVisitorEmotions(SimilarFaceMatch item)
        {
            EmotionDistribution genderBasedEmotionDistribution = null;

            if (item.Face.FaceAttributes.Gender == Gender.Male)
            {
                genderBasedEmotionDistribution = Demographics.EmotionGenderDistribution.MaleDistribution;
            }
            else
            {
                genderBasedEmotionDistribution = Demographics.EmotionGenderDistribution.FemaleDistribution;
            }

            var emotions = new Dictionary<string, double>
            {
                { "Anger", item.Face.FaceAttributes.Emotion.Anger },
                { "Contempt", item.Face.FaceAttributes.Emotion.Contempt },
                { "Disgust", item.Face.FaceAttributes.Emotion.Disgust },
                { "Fear", item.Face.FaceAttributes.Emotion.Fear },
                { "Happiness", item.Face.FaceAttributes.Emotion.Happiness },
                { "Neutral", item.Face.FaceAttributes.Emotion.Neutral },
                { "Sadness", item.Face.FaceAttributes.Emotion.Sadness },
                { "Surprise", item.Face.FaceAttributes.Emotion.Surprise }
            };

            var topEmotion = emotions.OrderByDescending(e => e.Value).FirstOrDefault();

            switch (topEmotion.Key)
            {
                case "Anger":
                    genderBasedEmotionDistribution.Anger++;
                    break;
                case "Contempt":
                    genderBasedEmotionDistribution.Contempt++;
                    break;
                case "Disgust":
                    genderBasedEmotionDistribution.Disgust++;
                    break;
                case "Fear":
                    genderBasedEmotionDistribution.Fear++;
                    break;
                case "Happiness":
                    genderBasedEmotionDistribution.Happiness++;
                    break;
                case "Neutral":
                    genderBasedEmotionDistribution.Neutral++;
                    break;
                case "Sadness":
                    genderBasedEmotionDistribution.Sadness++;
                    break;
                case "Surprise":
                    genderBasedEmotionDistribution.Surprise++;
                    break;
                default:
                    break;
            }
        }

        public string GetAgeGroupDescription(int age)
        {
            if (age < 16)
            {
                return "Age0To15++";
            }
            else if (age < 20)
            {
                return "Age16To19++";
            }
            else if (age < 30)
            {
                return "Age20s++";
            }
            else if (age < 40)
            {
                return "Age30s++";
            }
            else if (age < 50)
            {
                return "Age40s++";
            }
            else if (age < 60)
            {
                return "Age50s++";
            }
            else
            {
                return "Age60sAndOlder++";
            }
        }
    }
}
