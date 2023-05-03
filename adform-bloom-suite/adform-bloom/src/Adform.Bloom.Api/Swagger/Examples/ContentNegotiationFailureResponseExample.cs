using Adform.Bloom.Api.Swagger.Examples;
using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Services
{
    public class ContentNegotiationFailureResponseExample : IExamplesProvider<ContentNegotiationFailureResponse>
    {
        public ContentNegotiationFailureResponse GetExamples()
        {
            return new ContentNegotiationFailureResponse(
                "notAcceptable",
                "The requested representation is not supported."
            );
        }
    }
}