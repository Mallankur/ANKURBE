using Adform.Bloom.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Services
{
    public class UnauthorizedErrorResponseExample : IExamplesProvider<UnauthorizedErrorResponse>
    {
        public UnauthorizedErrorResponse GetExamples()
        {
            return new UnauthorizedErrorResponse(
                "invalid_token", 
                "The access token is missing."
            );
        }
    }
}