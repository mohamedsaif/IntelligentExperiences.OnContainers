using System;

namespace CognitiveOrchestrator.Models
{
    public class CognitiveRequest
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string DeviceId { get; set; }
        public DateTime TakenAt { get; set; }
        public DateTime ProcessedAt { get; set;}
        public string TargetAction { get; set; }
        public string Status { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsActive { get; set; }
    }
}