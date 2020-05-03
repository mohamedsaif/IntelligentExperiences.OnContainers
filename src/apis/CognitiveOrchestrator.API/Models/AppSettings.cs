namespace CognitiveOrchestrator.API.Models
{
    public class AppSettings
    {
        public string ServiceBusConnection { get; set; }
        public string ServiceBusSubscription { get; set; }
        public string ServiceBusTopic { get; set; }
        public string StorageConnection { get; set; }
        public string StorageContainer { get; set; }
        public string PersonsStorageContainer { get; set; }
        public string CognitiveKey { get; set; }
        public string CognitiveEndpoint { get; set; }
        public string CosmosDbEndpoint { get; set; }
        public string CosmosDbKey { get; set; }
        public string CosmosDBName { get; set; }
        public string FaceWorkspaceDataFilter { get; set; }
    }
}