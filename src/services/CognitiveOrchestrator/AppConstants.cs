using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator
{
    public class AppConstants
    {
        //Remember always that blob storage containers allows only lower case letters
        public const string StorageContainerName = "camframefiles";

        //Cosmos Db settings
        public const string DbName = "CognitiveAnalysisDb";
        public const string DbCognitiveFilesContainer = "CamFrameAnalysis";

        //Primary Service Bus settings
        public const string SBTopic = "cognitive-request";
        public const string SBSubscription = "cognitive-orchestrator";

        //Cognitive Actions Service Bus settings
        public const string SBTopicCamFrameAnalyzer = "camframe-analysis";
    }
}
