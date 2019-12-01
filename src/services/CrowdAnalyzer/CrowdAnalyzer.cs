using System;
using System.Linq;
using System.Threading.Tasks;
using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Models;
using CrowdAnalyzer.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CrowdAnalyzer.Functions
{
    public class CrowdAnalyzer
    {
        private IVisitorsRepository visitorRepo;
        private ICrowdDemographicsRepository crowdDemographicsRepo;

        public CrowdAnalyzer(
            IVisitorsRepository vRepo,
            ICrowdDemographicsRepository cRepo)
        {
            visitorRepo = vRepo;
            crowdDemographicsRepo = cRepo;
        }

        [FunctionName("CrowdAnalyzer")]
        [return: ServiceBus(queueOrTopicName: AppConstants.SBTopicDemographics,
            entityType: Microsoft.Azure.WebJobs.ServiceBus.EntityType.Topic,
            Connection = "serviceBusConnection")]
        public async Task<string> Run(
            [ServiceBusTrigger("crowd-analysis", "crowd-analyzer", Connection = "serviceBusConnection")]string request, 
            ILogger log)
        {
            DateTime startTime = DateTime.UtcNow;
            log.LogInformation($"FUNC (CrowdAnalyzer): crowd-analysis topic triggered processing message: {request.Substring(0, 20)}");

            CamFrameAnalysis analysis = null;

            try
            {
                analysis = JsonConvert.DeserializeObject<CamFrameAnalysis>(request);

                // Only process if there are detected faces
                if (analysis.SimilarFaces == null)
                {
                    log.LogWarning($"FUNC (CrowdAnalyzer): starting crowd-analysis: found no faces in the analysis request");
                    return null;
                }

                DemographicsAnalyzer demographics = new DemographicsAnalyzer(
                    analysis,
                    visitorRepo, 
                    crowdDemographicsRepo,
                    log);
                var isDemographicsUpdated = await demographics.UpdateDemographics();

                log.LogInformation($"FUNC (CrowdAnalyzer): finished processing with result: {JsonConvert.SerializeObject(demographics.Demographics)}");

                // If changes where made, publish changes to demographics-analysis topic
                if (isDemographicsUpdated)
                    return JsonConvert.SerializeObject(demographics.Demographics);

            }
            catch(Exception e)
            {
                log.LogError($"FUNC (CrowdAnalyzer): Failed with error: {JsonConvert.SerializeObject(e.Message)}");
            }

            
            return null;
        }
    }
}
