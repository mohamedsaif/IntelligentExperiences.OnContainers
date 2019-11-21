using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class EmotionDistribution
    {
        public int Anger { get; set; }
        public int Contempt { get; set; }
        public int Disgust { get; set; }
        public int Fear { get; set; }
        public int Happiness { get; set; }
        public int Neutral { get; set; }
        public int Sadness { get; set; }
        public int Surprise { get; set; }
    }
}
