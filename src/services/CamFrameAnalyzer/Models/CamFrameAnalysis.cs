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
        public CognitiveRequest Request { get; set; }
        public byte[] Data { get; set; }
        public IEnumerable<DetectedFace> DetectedFaces { get; set; }
        public IEnumerable<DetectedFace> IdentifiedFaces { get; set; }
        public IEnumerable<DetectedFace> SimilarFaces { get; set; }
        public CamFrameSummary Summary { get; set; }
        public bool IsSuccessfull { get; set; }
        public string Status { get; set; }
    }
}