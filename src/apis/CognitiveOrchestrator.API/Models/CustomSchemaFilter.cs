using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CognitiveOrchestrator.API.Models
{
    public class CustomSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            ////This is to allow easy import to Azure API Management service by resting interface like IFormFile.Headers object type to string.
            //var typeInfo = context.SystemType.GetTypeInfo();
            //if (typeInfo.IsInterface)
            //    schema.Type = "string";
        }
    }
}
