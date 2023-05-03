using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using MediatR;

namespace Adform.Bloom.Mediatr.Extensions
{
    public class MeasuredPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private static readonly string Type = typeof(TRequest).Name;
        private readonly ICustomHistogram _histogram;

        public MeasuredPipelineBehavior(ICustomHistogram histogram)
        {
            _histogram = histogram;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var response = await next();
                return response;
            }
            finally
            {
                watch.Stop();
                _histogram.Observe(watch.ElapsedMilliseconds, Type);
            }
        }
    }
}