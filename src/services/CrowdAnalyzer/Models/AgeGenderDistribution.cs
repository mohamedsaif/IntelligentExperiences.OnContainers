using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class AgeGenderDistribution
    {
        public AgeDistribution MaleDistribution { get; set; }
        public AgeDistribution FemaleDistribution { get; set; }
    }
}
