using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonIdentificationLib
{
    public static class AppConstants
    {
        public const string DbColIdentifiedVisitor = "identified-visitors";
        public const string DbColIdentifiedVisitorGroup = "identified-visitors-groups";
        public const string DbColIdentifiedVisitorPartitionKey = "PartitionKey";
        public const string DbColIdentifiedVisitorPartitionKeyValue = "default";
        public const string Origin = "PersonIdentificationLib.v1.0.0";
    }
}
