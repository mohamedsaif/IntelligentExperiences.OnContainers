using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Models
{
    public class MaskAnalysis : BaseModel
    {
        public string TimeKey { get; set; }
        public CognitiveRequest Request { get; set; }

        public MaskDetectionResult DetectionResult { get; set; }

        public int TotalDetected { get; set; }
        public int TotalDetectedWithMasks { get; set; }
        public int TotalDetectedWithoutMasks { get; set; }

        // Processing status
        public bool IsSuccessful { get; set; }
        public string Status { get; set; }
        public int TotalProcessingTime { get; set; }

    }
}
