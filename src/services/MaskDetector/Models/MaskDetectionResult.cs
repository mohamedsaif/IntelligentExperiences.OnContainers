using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Models
{
    /// <summary>
    /// Custom vision model REST result
    /// </summary>
    public class MaskDetectionResult
    {
        public DateTime Created { get; set; }
        public string Id { get; set; }
        public string Iteration { get; set; }
        public string Project { get; set; }
        
        public List<MaskDetectionPredection> Predictions { get; set; }
    }

    public class MaskDetectionPredection
    {
        public double Probability { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
        public BoundingBoxRect BoundingBox { get; set; }
    }

    public class BoundingBoxRect
    {
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
    }
}
