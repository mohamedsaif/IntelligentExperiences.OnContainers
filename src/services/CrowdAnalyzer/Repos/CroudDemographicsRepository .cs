using CrowdAnalyzer.Abstractions;
using CrowdAnalyzer.Models;
using CoreLib.Repos;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrowdAnalyzer.Repos
{
    public class CroudDemographicsRepository : CosmosDbRepository<CrowdDemographics>, ICrowdDemographicsRepository
    {
        public CroudDemographicsRepository(ICosmosDbClientFactory cosmosDbClientFactory) : base(cosmosDbClientFactory) { }

        public override string CollectionName { get; } = AppConstants.DbColCrowdDemographics;

        public override string GenerateId(CrowdDemographics entity) => $"{Guid.NewGuid()}";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey($"{entityId}");
    }
}
