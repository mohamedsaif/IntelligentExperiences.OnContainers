using System;

namespace CognitiveOrchestrator.Models
{
    public class CamFrame
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
        public string DeviceId { get; set; }
        public DateTime TakenAt { get; set; }
        public string TargetAction { get; set; }
        public string Status { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsActive { get; set; }
    }
}