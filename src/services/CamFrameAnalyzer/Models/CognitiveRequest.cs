using CoreLib.Models;
using System;

namespace CamFrameAnalyzer.Models
{
    public class CognitiveRequest : BaseModel
    {
        public string FileUrl { get; set; }
        public string DeviceId { get; set; }
        public string TargetAction { get; set; }
        public string Status { get; set; }
        public bool IsProcessed { get; set; }
    }
}