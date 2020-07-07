using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Models
{
    public class SwaggerFilter : IDocumentFilter
    {
        private readonly string title;
        private readonly string filter;

        public SwaggerFilter(string title, string filter)
        {
            this.title = title;
            this.filter = filter;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Even though the filter is applied to a specific document, this get called for every document,
            // so to ensure we only apply the filter to the right document we check it is for the expected document
            if (!swaggerDoc.Info.Title.Contains(this.title)) return;
            swaggerDoc.Info = new OpenApiInfo
            {
                Title = title,
                Description = "Set of APIs to manage various aspects of the crowd analytics platform",
                Version = "1.0.0",
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://blog.mohamedsaif.com")
                },
                Contact = new OpenApiContact
                {
                    Name = "Mohamed Saif",
                    Url = new Uri ("https://github.com/mohamedsaif")
                }
            };
            var filteredPaths = swaggerDoc.Paths.Where(x => x.Key.Contains(filter)).ToDictionary(x => x.Key, x => x.Value);
            swaggerDoc.Paths.Clear();
            foreach (var path in filteredPaths)
                swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}
