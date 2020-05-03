using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CognitiveOrchestrator.API.Health;
using CognitiveOrchestrator.API.Models;
using CoreLib.Abstractions;
using CoreLib.Repos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Services;

namespace ConitiveOrchestrator.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            var settings = Configuration.Get<AppSettings>();
            
            services.AddSingleton<IStorageRepository>((s) =>
            {
                return new AzureBlobStorageRepository(settings.StorageConnection, settings.StorageContainer);
            });

            services.AddSingleton<IAzureServiceBusRepository>((s) =>
            {
                return new AzureServiceBusRepository(settings.ServiceBusConnection, settings.ServiceBusTopic, settings.ServiceBusSubscription);
            });

            services.AddTransient<IVisitorIdentificationManager>((s) =>
            {
                return new VisitorIdentificationManager(settings.CognitiveKey, 
                    settings.CognitiveEndpoint,
                    settings.FaceWorkspaceDataFilter, 
                    settings.CosmosDbEndpoint, 
                    settings.CosmosDbKey, 
                    settings.CosmosDBName, 
                    settings.StorageConnection, 
                    settings.PersonsStorageContainer);
            });

            services.AddHealthChecks()
                .AddCheck<ServiceHealthCheck>("Service_Health_Check");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
