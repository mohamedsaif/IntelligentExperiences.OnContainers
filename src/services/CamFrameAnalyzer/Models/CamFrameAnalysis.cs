using CoreLib.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.Collections.Generic;

namespace CamFrameAnalyzer.Models
{
    public class CamFrameAnalysis : BaseModel
    {
        public CamFrameAnalysis()
        {
            Summary = new CamFrameSummary();
        }

        public byte[] Data { get; set; }
        public IList<DetectedFace> DetectedFaces { get; set; }
        public IList<DetectedFace> IdentifiedFaces { get; set; }
        public IList<DetectedFace> SimilarFaces { get; set; }
        public CamFrameSummary Summary { get; set; }
        public bool IsSuccessfull { get; set; }
        public string Status { get; set; }
    }
}