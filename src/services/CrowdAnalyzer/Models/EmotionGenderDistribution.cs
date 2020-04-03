using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class EmotionGenderDistribution
    {
        public EmotionDistribution MaleDistribution { get; set; }
        public EmotionDistribution FemaleDistribution { get; set; }
    }
}
