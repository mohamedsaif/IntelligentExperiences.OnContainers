using CamFrameAnalyzer.Abstractions;
using CamFrameAnalyzer.Models;
using CoreLib.Repos;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamFrameAnalyzer.Repos
{
    public class CamFrameAnalysisRepository : CosmosDbRepository<CamFrameAnalysis>, ICamFrameAnalysisRepository
    {
        public CamFrameAnalysisRepository(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) {}

        public override string CollectionName { get; } = AppConstants.DbColCamFrameAnalysis;

        public override string GenerateId(CamFrameAnalysis entity) => $"{Guid.NewGuid()}";

        // Initially I opted to use month-year as the partition key. You can partition the data in different way.
        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey($"{entityId.Substring(entityId.LastIndexOf('-') + 1)}");
    }
}
