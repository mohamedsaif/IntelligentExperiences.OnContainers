using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Microsoft.OpenApi.Models;
using PersonIdentificationLib.Abstractions;
using PersonIdentificationLib.Services;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;

namespace ConitiveOrchestrator.API
{
    public class Startup
    {
        List<string> apis = new List<string>
        {
            "Orchestrator",
            "Identification"
        };
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

            foreach (var api in apis)
            {
                services.AddSwaggerGen(this.SwaggerGen(api, $"api/{api.ToLower()}"));
            }


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

            //Configuring Swagger API documentation and its UI
            app.UseSwagger(c =>
            {
                //Perform any additional customizations here
            });

            //Separate swagger page/json for each controller
            app.UseSwaggerUI(c =>
            {
                foreach (var api in apis)
                {
                    c.SwaggerEndpoint($"/swagger/{api}/swagger.json", api);
                }

                c.RoutePrefix = string.Empty; // Makes Swagger UI the root page
            });


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }

        private Action<SwaggerGenOptions> SwaggerGen(string name, string filter)
        {
            return c =>
            {
                c.SwaggerDoc(name, new OpenApiInfo { Title = $"Crowd Analytics API - {name}", Version = "1.0.0" });

                // The swagger filter takes the filter and removes any path that doesn't contain 
                // the value of the filter in its route. Allowing for us to separate out the generated
                // swagger documents
                c.DocumentFilter<SwaggerFilter>(name, filter);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            };
        }

    }
}
