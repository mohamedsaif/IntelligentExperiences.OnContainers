using CoreLib.Repos;
using MaskDetector.Abstractions;
using MaskDetector.Models;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskDetector.Repos
{
    public class MaskAnalysisRepository : CosmosDbRepository<MaskAnalysis>, IMaskAnalysisRepository
    {
        public MaskAnalysisRepository(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) { }

        public override string CollectionName { get; } = AppConstants.DbColMaskAnalysis;

        public override string GenerateId(MaskAnalysis entity) => $"{Guid.NewGuid()}";

        // Initially I opted to use month-year as the partition key. You can partition the data in different way.
        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey($"{entityId.Substring(entityId.LastIndexOf('-') + 1)}");
    }
}
