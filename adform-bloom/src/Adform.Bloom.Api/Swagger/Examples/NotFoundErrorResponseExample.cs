using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Swagger.Examples
{
    public class NotFoundErrorResponseExample : IExamplesProvider<NotFoundErrorResponse>
    {
        public NotFoundErrorResponse GetExamples()
        {
            return new NotFoundErrorResponse(
                "notFound",
                "Resource not found."
            );
        }
    }
}