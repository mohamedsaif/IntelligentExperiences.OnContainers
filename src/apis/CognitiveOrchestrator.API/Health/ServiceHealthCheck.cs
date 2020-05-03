using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Health
{
    /// <summary>
    /// Perform dependencies health checks
    /// Docs: https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-3.1
    /// </summary>
    public class ServiceHealthCheck : IHealthCheck
    {
        public ServiceHealthCheck()
        {

        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            //TODO: Implement health checks
            return Task.FromResult(HealthCheckResult.Healthy("Default healthy check"));
        }
    }
}
