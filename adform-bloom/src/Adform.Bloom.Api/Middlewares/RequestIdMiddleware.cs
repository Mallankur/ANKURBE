using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace Adform.Bloom.Api.Middlewares
{
    public class RequestIdMiddleware
    {
        private const string GatewayRequestIdHeaderName = "RequestId";
        private const string GatewayRequestIdPropertyName = "GatewayRequestId";

        private readonly RequestDelegate _next;

        public RequestIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(GatewayRequestIdHeaderName, out var value))
                using (LogContext.PushProperty(GatewayRequestIdPropertyName, value.ToString()))
                {
                    await _next.Invoke(context);
                    return;
                }

            await _next(context);
        }
    }
}