using CoreLib.Repos;
using CoreLib.Utils;
using Microsoft.Azure.ServiceBus.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.Repos
{
    public class CamFrameAnalyzerServiceBus : AzureServiceBusRepository
    {
        public CamFrameAnalyzerServiceBus() : base(GlobalSettings.GetKeyValue("serviceBusConnection"), AppConstants.SBTopicCamFrameAnalyzer, null)
        {
            
        }
    }
}
