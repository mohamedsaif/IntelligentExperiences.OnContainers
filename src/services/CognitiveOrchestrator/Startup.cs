using CognitiveOrchestrator.Repos;
using CoreLib.Repos;
using CoreLib.Utils;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

[assembly: FunctionsStartup(typeof(CognitiveOrchestrator.Startup))]

namespace CognitiveOrchestrator
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<CamFrameAnalyzerServiceBus>();
        }
    }
}