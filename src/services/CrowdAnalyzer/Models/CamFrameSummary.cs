using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class CamFrameSummary
    {
        public CamFrameSummary()
        {
            AgeGenderDistribution = new AgeGenderDistribution
            {
                MaleDistribution = new AgeDistribution(),
                FemaleDistribution = new AgeDistribution()
            };
        }
        public int TotalDetectedFaces { get; set; }
        public int TotalMales { get; set; }
        public int TotalFemales { get; set; }
        public AgeGenderDistribution AgeGenderDistribution { get; set; }
    }
}
