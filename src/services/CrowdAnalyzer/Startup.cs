using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Repos;
using CoreLib.Abstractions;
using CoreLib.Repos;
using CoreLib.Utils;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using CrowdAnalyzer;

[assembly: FunctionsStartup(typeof(CamFrameAnalyzer.Startup))]

namespace CamFrameAnalyzer
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string identificationConfidence = GlobalSettings.GetKeyValue("IdentificationConfidence");
            if (string.IsNullOrEmpty(identificationConfidence))
                AppConstants.IdentificationConfidence = double.Parse(identificationConfidence);
            var checkForDbConsistency = bool.Parse(GlobalSettings.GetKeyValue("checkForDbConsistency"));
            //First register the db client (which is needed for the strongly typed repos)
            var cosmosDbEndpoint = GlobalSettings.GetKeyValue("cosmosDbEndpoint");
            var cosmosDbKey = GlobalSettings.GetKeyValue("cosmosDbKey");
            var dbClient = new DocumentClient(
                new Uri(cosmosDbEndpoint), 
                cosmosDbKey, 
                new ConnectionPolicy { EnableEndpointDiscovery = false });
            builder.Services.AddSingleton<ICosmosDbClientFactory>((s) =>
            {
                var factory = new CosmosDbClientFactory(
                    AppConstants.DbName, 
                    new Dictionary<string,string> { 
                        { AppConstants.DbColCrowdDemographics, AppConstants.DbColCrowdDemographicsPartitionKey },
                        { AppConstants.DbColVisitors, AppConstants.DbColVisitorsPartitionKey },
                    }, 
                    dbClient);
                if (checkForDbConsistency)
                    factory.EnsureDbSetupAsync().Wait();
                return factory;
            });

            //Register our cosmos db repository :)
            builder.Services.AddSingleton<ICrowdDemographicsRepository, CrowdDemographicsRepository>();
            builder.Services.AddSingleton<IVisitorsRepository, VisitorsRepository>();

            //If you need further control over Service Bus, you can also inject the repo for it.
            //var serviceBusConnection = GlobalSettings.GetKeyValue("serviceBusConnection");
            //builder.Services.AddSingleton<IAzureServiceBusRepository>((s) =>
            //{
            //    return new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
            //});
        }
    }
}