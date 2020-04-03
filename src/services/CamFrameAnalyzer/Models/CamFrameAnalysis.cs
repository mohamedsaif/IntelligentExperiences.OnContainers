using CognitiveServiceHelpers.Models;
using CoreLib.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CamFrameAnalyzer.Models
{
    public class CamFrameAnalysis : BaseModel
    {
        public CamFrameAnalysis()
        {
            Summary = new CamFrameSummary();
        }
        public string TimeKey { get; set; }
        public CognitiveRequest Request { get; set; }
        [JsonIgnore]
        public IEnumerable<DetectedFace> DetectedFaces { get; set; }
        public IEnumerable<Tuple<DetectedFace, IdentifiedPerson>> IdentifiedPersons { get; set; }
        public IEnumerable<SimilarFaceMatch> SimilarFaces { get; set; }
        public CamFrameSummary Summary { get; set; }
        public bool IsSuccessful { get; set; }
        public bool IsDetectionSuccessful { get; set; }
        public bool IsIdentificationSuccessful { get; set; }
        public bool IsSimilaritiesSuccessful { get; set; }
        public string Status { get; set; }

        //Milliseconds
        public int TotalProcessingTime { get; set; }
    }
}