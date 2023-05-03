using Microsoft.AspNetCore.Builder;

namespace Adform.Bloom.Api.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGatewayRequestId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestIdMiddleware>();
        }
    }
}