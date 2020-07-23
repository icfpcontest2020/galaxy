using System.Linq;
using System.Reflection;

using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace PlanetWars.Server.Helpers
{
    public class RawTextRequestOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var rawTextRequestAttribute = context.MethodInfo.GetCustomAttributes<RawTextRequestAttribute>().FirstOrDefault();
            if (rawTextRequestAttribute != null)
            {
                operation.RequestBody = new OpenApiRequestBody();
                operation.RequestBody.Content.Add(rawTextRequestAttribute.MediaType, new OpenApiMediaType()
                {
                    Schema = new OpenApiSchema()
                    {
                        Type = "string"
                    }
                });
            }
        }
    }
}