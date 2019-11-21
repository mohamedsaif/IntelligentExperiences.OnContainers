using System;
using System.Threading.Tasks;
using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Models;
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
            log.LogInformation($"FUNC (CrowdAnalyzer): crowd-analysis topic triggered processing message: {JsonConvert.SerializeObject(request)}");

            CamFrameAnalysis analysis = null;

            try
            {
                analysis = JsonConvert.DeserializeObject<CamFrameAnalysis>(request);
            }
            catch(Exception e)
            {

            }

            return null;
        }
    }
}
