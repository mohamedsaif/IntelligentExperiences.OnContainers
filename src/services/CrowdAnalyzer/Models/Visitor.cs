using CognitiveServiceHelpers.Models;
using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class Visitor : BaseModel
    {
        public int VisitsCount { get; set; }
        public int Age { get; set; }
        public string AgeGroup { get; set; }
        public string Gender { get; set; }
        public List<Visit> LastVisits { get; set; }
    }
}
