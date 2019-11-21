using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class Visit
    {
        public int Count { get; set; }
        public DateTime VisitDate { get; set; }
        public string DetectedOnDeviceId { get; set; }
    }
}
