
using System;

namespace CamFrameAnalyzer.Models
{
    public abstract class BaseModel
    {
        public string Id { get; set; }
        public DateTime TakenAt { get; set; }
        public DateTime ProcessedAt { get; set;}
        public bool IsActive { get; set; }
    }
}