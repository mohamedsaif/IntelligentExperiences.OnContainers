using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Models
{
    public class CrowdDemographics : BaseModel
    {
        public CrowdDemographics()
        {
            AgeGenderDistribution = new AgeGenderDistribution
            {
                MaleDistribution = new AgeDistribution(),
                FemaleDistribution = new AgeDistribution()
            };

            EmotionGenderDistribution = new EmotionGenderDistribution
            {
                MaleDistribution = new EmotionDistribution(),
                FemaleDistribution = new EmotionDistribution()
            };
        }

        //Analysis Window Details
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public string DeviceId { get; set; }

        //Unique stats
        public int TotalVisitors { get; set; }
        public int TotalMales { get; set; }
        public int TotalFemales { get; set; }

        /// <summary>
        /// Stats about completely new visitors (not returning customers)
        /// </summary>
        public int TotalNewMaleVisitors { get; set; }
        /// <summary>
        /// Stats about completely new visitors (not returning customers)
        /// </summary>
        public int TotalNewFemaleVisitors { get; set; }

        public AgeGenderDistribution AgeGenderDistribution { get; set; }
        public EmotionGenderDistribution EmotionGenderDistribution { get; set; }

        //System telemtry
        public int TotalProcessingTime { get; set; }
    }
}
