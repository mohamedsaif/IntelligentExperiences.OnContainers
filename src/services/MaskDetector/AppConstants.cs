using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector
{
    /// <summary>
    /// Hold a central list of all static configurations that can remain the same across all deployments
    /// </summary>
    public static class AppConstants
    {
        //Remember always that blob storage containers allows only lower case letters
        public const string StorageContainerName = "camframefiles";

        //Cosmos Db settings
        public const string DbName = "cognitive-analysis-db";
        public const string DbColMaskAnalysis = "mask-analysis";
        public const string DbColMaskAnalysisPartitionKey = "TimeKey";

        //Main Service Bus settings
        public const string SBTopic = "camframe-analysis";
        public const string SBSubscription = "mask-detector";
    }
}
