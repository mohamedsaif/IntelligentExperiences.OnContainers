using System;

namespace PersonIdentificationLib.Models
{
    public class Visit
    {
        public int Count { get; set; }
        public DateTime VisitDate { get; set; }
        public string DetectedOnDeviceId { get; set; }
    }
}