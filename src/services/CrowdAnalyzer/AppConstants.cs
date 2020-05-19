using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer
{
    /// <summary>
    /// Hold a central list of all static configurations that can remain the same across all deployments
    /// </summary>
    public static class AppConstants
    {
        //Cosmos Db settings
        public const string DbName = "cognitive-analysis-db";
        public const string DbColVisitors = "visitors";
        public const string DbColVisitorsPartitionKey = "Gender";
        public const string DbColCrowdDemographics = "crowd-demographics";
        public const string DbColCrowdDemographicsPartitionKey = "DeviceId";

        public const string DbColIdentifiedVisitor = "identified-visitors";
        public const string DbColIdentifiedVisitorPartitionKey = "PartitionKey";

        //Main Service Bus settings
        public const string SBTopic = "crowd-analysis";
        public const string SBSubscription = "crowd-analyzer";

        //Demographics Analysis Service Bus settings
        public const string SBTopicDemographics = "demographics-analysis";

        public static double IdentificationConfidence = 0.55;
    }
}
