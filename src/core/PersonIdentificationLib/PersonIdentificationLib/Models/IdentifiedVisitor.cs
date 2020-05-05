using CognitiveServiceHelpers.Models;
using CoreLib.Models;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Models
{
    public class IdentifiedVisitor : BaseModel
    {
        public Person PersonDetails { get; set; }
        public string GroupId { get; set; }

        public string PartitionKey { get; set; }

        public string Name { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string ContactPhone { get; set; }
        public bool IsConsentGranted { get; set; }
        public List<VisitorPhoto> Photos { get; set; }
        public double Age { get; set; }
        public string Gender { get; set; }
        public List<string> LastVisits { get; set; }
        public int VisitsCount { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }

    public class VisitorPhoto
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsSaved { get; set; }
        [JsonIgnore]
        public byte[] PhotoData { get; set; }
        public string Status { get; set; }
    }
}
