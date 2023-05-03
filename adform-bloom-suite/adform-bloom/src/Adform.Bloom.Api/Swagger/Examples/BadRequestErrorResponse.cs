namespace Adform.Bloom.Api.Swagger.Examples
{
    public record BadRequestErrorResponse(object Errors, string Type, string Title, int Status, string TraceId);
}