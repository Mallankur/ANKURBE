using Adform.Bloom.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Services
{
    public class ForbiddenErrorResponseExample : IExamplesProvider<ForbiddenErrorResponse>
    {
        public ForbiddenErrorResponse GetExamples()
        {
            return new ForbiddenErrorResponse(
            "insufficient_scope", 
            "The access token does not contain scopes required to access the resource."
            );
        }
    }
}