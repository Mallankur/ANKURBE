using System.Collections.Generic;
using System.Net.Mime;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Adform.Bloom.Api.Swagger.Filters
{
    public class AddRequestHeaderAcceptFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter()
            {
                Name = HeaderNames.Accept.ToLower(),
                In = ParameterLocation.Header,
                Description = "Accept header",
                Required = false,
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                    Default = new OpenApiString(MediaTypeNames.Application.Json)
                }
            });
       }
    }
}