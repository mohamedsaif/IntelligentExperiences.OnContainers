namespace CognitiveOrchestrator.API.Models
{
    public class AppSettings
    {
        public string ServiceBusConnection { get; set; }
        public string ServiceBusSubscription { get; set; }
        public string ServiceBusTopic { get; set; }
        public string StorageConnection { get; set; }
        public string StorageContainer { get; set; }
    }
}