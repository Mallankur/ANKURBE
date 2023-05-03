using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Adform.Bloom.Api.Swagger.Filters
{
    public class AddResponseHeaderWwwAuthenticateFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses.TryGetValue(StatusCodes.Status401Unauthorized.ToString(), out var responseUnauthorized);
            responseUnauthorized?.Headers.Add(WwwAuthenticateHeader);

            operation.Responses.TryGetValue(StatusCodes.Status403Forbidden.ToString(), out var responseForbidden);
            responseForbidden?.Headers.Add(WwwAuthenticateHeader);
        }

        private static KeyValuePair<string, OpenApiHeader> WwwAuthenticateHeader =>
            new(
                "WWW-Authenticate",
                new OpenApiHeader()
                {
                    Description = "Indicates an authentication scheme that can be used to access the resource",
                    Example = new OpenApiString("Basic realm=<realm>"),
                    Schema = new OpenApiSchema()
                    {
                        Type = "string",
                        Example = new OpenApiString("Basic realm=<realm>")
                    }
                });
    }
}