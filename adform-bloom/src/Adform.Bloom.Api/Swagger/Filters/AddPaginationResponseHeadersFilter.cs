using System.Collections.Generic;
using System.Linq;
using Adform.Ciam.Swagger.Attributes;
using Adform.Ciam.Swagger.Enums;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Adform.Bloom.Api.Swagger.Filters
{
    public class AddPaginationResponseHeadersFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            context.ApiDescription.TryGetMethodInfo(out var methodInfo);
            var headerAttribute = methodInfo?
                .GetCustomAttributes(typeof(ProducesResponseHeadersAttribute), false)
                .FirstOrDefault();

            if (headerAttribute is null)
            {
                return;
            }

            var responseHeadersAttribute = (ProducesResponseHeadersAttribute) headerAttribute;
            switch (responseHeadersAttribute.Headers)
            {
                case Headers.Pagination:
                    AddPaginationResponseHeaders(operation, responseHeadersAttribute.StatusCodes);
                    break;
            }
        }

        private static void AddPaginationResponseHeaders(OpenApiOperation operation, IEnumerable<int> statusCodes)
        {
            statusCodes.ToList().ForEach(i =>
            {
                operation.Responses.TryGetValue(i.ToString(), out var response);
                response?.Headers.Add(CreateHeader("limit"));
                response?.Headers.Add(CreateHeader("offset"));
                response?.Headers.Add(CreateHeader("total-count"));
            });
        }

        private static KeyValuePair<string, OpenApiHeader> CreateHeader(string headerName)
        {
            return new(
                headerName,
                new OpenApiHeader
                {
                    Description = $"Pagination {headerName}",
                    Schema = new OpenApiSchema()
                    {
                        Type = "integer",
                    }
                });
        }
    }
}