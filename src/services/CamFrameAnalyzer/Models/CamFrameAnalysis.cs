namespace CamFrameAnalyzer.Models
{
    public class CamFrameAnalysis : BaseModel
    {
        public byte[] Data { get; set; }
        public IList<DetectedFace> DetectedFaces { get; set; }
        public bool IsSuccessfull { get; set; }
        public string Status { get; set; }
    }
}