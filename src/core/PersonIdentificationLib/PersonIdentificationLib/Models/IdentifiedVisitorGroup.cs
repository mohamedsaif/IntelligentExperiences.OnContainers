using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib.Models
{
    public class IdentifiedVisitorGroup : BaseModel
    {
        public string Name { get; set; }
        public string GroupId { get; set; }
        public string Filter { get; set; }
        public bool IsActive { get; set; }
        public string PartitionKey { get; set; }
    }
}
