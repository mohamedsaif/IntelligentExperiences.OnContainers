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
        public const string DbColIdentifiedVisitorPartitionKey = "ParitionKey";
    }
}
