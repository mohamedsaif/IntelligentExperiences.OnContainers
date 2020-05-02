using CognitiveServiceHelpers;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.Azure.Documents.Client;
using PersonIdentificationLib.Models;
using PersonIdentificationLib.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Services
{
    public class VisitorIdentificationManager
    {
        private string key = string.Empty;
        private string endpoint = string.Empty;
        private string faceWorkspaceDataFilter;

        private IStorageRepository filesStorageRepo;
        
        //private ICamFrameAnalysisRepository camFrameAnalysisRepo;
        
        private IAzureServiceBusRepository serviceBusRepo;
        private CognitiveFacesAnalyzer cognitiveFacesAnalyzer;
        private IdentifiedVisitorRepo identifiedVisitorRepo;
        private AzureBlobStorageRepository storageRepo;

        public VisitorIdentificationManager(string cognitiveKey, 
            string cognitiveEndpoint, 
            string faceFilter,
            string cosmosDbEndpoint,
            string cosmosDbKey,
            string cosmosDbName,
            string storageConnection,
            string storageContainerName)
        {
            key = cognitiveKey;
            endpoint = cognitiveEndpoint;
            faceWorkspaceDataFilter = faceFilter;

            var dbClient = new DocumentClient(
                new Uri(cosmosDbEndpoint),
                cosmosDbKey,
                new ConnectionPolicy { EnableEndpointDiscovery = false });

            var dbFactory = new CosmosDbClientFactory(
                    cosmosDbName,
                    new Dictionary<string, string> { { AppConstants.DbColIdentifiedVisitor, AppConstants.DbColIdentifiedVisitorPartitionKey } },
                    dbClient);
            identifiedVisitorRepo = new IdentifiedVisitorRepo(dbFactory, AppConstants.DbColIdentifiedVisitor);
            
            storageRepo = new AzureBlobStorageRepository(storageConnection, storageContainerName);

            //serviceBusRepo = new AzureServiceBusRepository(serviceBusConnection, AppConstants.SBTopic, AppConstants.SBSubscription);
        }

        public async Task CreateVisitorGroupAsync(string groupId, string filter)
        {

        }

        public async Task CreateVisitorAsync(IdentifiedVisitor visitor)
        {

        }

        public async Task AddVisitorPhotoAsync(string visitorId, string photoUrl)
        {

        }

        public async Task TrainVisitorGroup(string groupId)
        {

        }

        public async Task<List<IdentifiedVisitor>> GetIdentifiedVisitorsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
