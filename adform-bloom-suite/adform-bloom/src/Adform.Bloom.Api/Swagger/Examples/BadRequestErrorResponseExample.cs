using Swashbuckle.AspNetCore.Filters;

namespace Adform.Bloom.Api.Swagger.Examples
{
    public class BadRequestErrorResponseExample : IExamplesProvider<BadRequestErrorResponse>
    {
        public BadRequestErrorResponse GetExamples()
        {
            return new BadRequestErrorResponse(
                new object(),
                "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                "One or more validation errors occurred.",
                400,
                "00-9bc98f51ec487244bf7846d2784c24f3-7b78b80e3249ee46-00"
            );
        }
    }
}