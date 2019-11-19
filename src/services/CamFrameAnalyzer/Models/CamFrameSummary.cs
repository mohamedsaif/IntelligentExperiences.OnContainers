using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamFrameAnalyzer.Models
{
    public class CamFrameSummary
    {
        public int TotalDetectedFaces { get; set; }
        public int TotalIdentifiedFaces { get; set; }
        public int TotalSimilarFaces { get; set; }
        public int TotalUniqueFaces { get; set; }
        public int TotalUniqueMales { get; set; }
        public int TotalUniqueFemales { get; set; }
        public Dictionary<string, int> AgeGroupsMales { get; set; }
        public Dictionary<string, int> AgeGroupFemales { get; set; }
        public Dictionary<string, float> GenericStats { get; set; }
    }
}
