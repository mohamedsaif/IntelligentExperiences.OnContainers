using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class AgeDistribution
    {
        public int Age0To15 { get; set; }
        public int Age16To19 { get; set; }
        public int Age20s { get; set; }
        public int Age30s { get; set; }
        public int Age40s { get; set; }
        public int Age50s { get; set; }
        public int Age60sAndOlder { get; set; }
    }
}
