using MaskDetector.Abstractions;
using MaskDetector.Repos;
using CoreLib.Abstractions;
using CoreLib.Repos;
using CoreLib.Utils;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using MaskDetector.Services;
using Refit;
using Newtonsoft.Json.Serialization;

[assembly: FunctionsStartup(typeof(MaskDetector.Startup))]

namespace MaskDetector
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
                    new Dictionary<string,string> { { AppConstants.DbColMaskAnalysis, AppConstants.DbColMaskAnalysisPartitionKey } }, 
                    dbClient);
            });

            //Register our cosmos db repository :)
            builder.Services.AddSingleton<IMaskAnalysisRepository, MaskAnalysisRepository>();

            var camFrameStorageConnection = GlobalSettings.GetKeyValue("camFrameStorageConnection");
            builder.Services.AddSingleton<IStorageRepository>((s) =>
            {
                return new AzureBlobStorageRepository(camFrameStorageConnection, AppConstants.StorageContainerName);
            });

            var serviceBusConnection = GlobalSettings.GetKeyValue("serviceBusConnection");
            builder.Services.AddSingleton<IAzureServiceBusRepository>((s) =>
            {
                return new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
            });

            var maskDetectionApiUrl = GlobalSettings.GetKeyValue("maskDetectionApiUrl");
            AppConstants.MaskDetectionThreshold = double.Parse(GlobalSettings.GetKeyValue("maskDetectionThreshold"));
            builder.Services.AddSingleton<IMaskDetectionAPI>((s) =>
            {
                return RestService.For<IMaskDetectionAPI>(maskDetectionApiUrl, new RefitSettings
                {
                     ContentSerializer = new NewtonsoftJsonContentSerializer(new Newtonsoft.Json.JsonSerializerSettings
                     {
                         ContractResolver = new CamelCasePropertyNamesContractResolver()
                     })
                });
            });
        }
    }
}