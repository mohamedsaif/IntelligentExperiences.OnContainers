using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamFrameAnalyzer
{
    /// <summary>
    /// Hold a central list of all static configurations that can remain the same across all deployments
    /// </summary>
    public static class AppConstants
    {
        //Remember always that blob storage containers allows only lower case letters
        public const string StorageContainerName = "camframefiles";

        //Cosmos Db settings
        public const string DbName = "CognitiveAnalysisDb";
        public const string DbCognitiveFilesContainer = "CamFrameAnalysis";

        //Service Bus settings
        public const string SBTopic = "camframe-analysis";
        public const string SBSubscription = "camframe-analyzer";
    }
}
