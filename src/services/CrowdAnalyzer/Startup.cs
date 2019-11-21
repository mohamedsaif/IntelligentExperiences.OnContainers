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
            //First register the db client (which is needed for the strongly typed repos)
            var cosmosDbEndpoint = GlobalSettings.GetKeyValue("cosmosDbEndpoint");
            var cosmosDbKey = GlobalSettings.GetKeyValue("cosmosDbKey");
            var dbClient = new DocumentClient(
                new Uri(cosmosDbEndpoint), 
                cosmosDbKey, 
                new ConnectionPolicy { EnableEndpointDiscovery = false });
            builder.Services.AddSingleton<ICosmosDbClientFactory>((s) =>
            {
                return new CosmosDbClientFactory(
                    AppConstants.DbName, 
                    new Dictionary<string,string> { 
                        { AppConstants.DbColCrowdDemographics, AppConstants.DbColCrowdDemographicsPartitionKey },
                        { AppConstants.DbColVisitors, AppConstants.DbColVisitorsPartitionKey },
                    }, 
                    dbClient);
            });

            //Register our cosmos db repository :)
            builder.Services.AddSingleton<ICrowdDemographicsRepository, CrowdDemographicsRepository>();
            builder.Services.AddSingleton<IVisitorsRepository, VisitorsRepository>();

            //var serviceBusConnection = GlobalSettings.GetKeyValue("serviceBusConnection");
            //builder.Services.AddSingleton<IAzureServiceBusRepository>((s) =>
            //{
            //    return new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
            //});
        }
    }
}