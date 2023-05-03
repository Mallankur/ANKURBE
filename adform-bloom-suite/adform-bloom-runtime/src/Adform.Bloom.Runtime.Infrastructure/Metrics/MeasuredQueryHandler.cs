using System.Diagnostics;
using Adform.Ciam.Monitoring.Abstractions.CustomStructures;
using MediatR;

namespace Adform.Bloom.Runtime.Infrastructure.Metrics
{
    public class MeasuredQueryHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;
        private readonly string _queryName;
        private readonly ICustomHistogram _histogram;

        public MeasuredQueryHandler(IRequestHandler<TRequest, TResponse> inner, ICustomHistogram histogram, string queryName)
        {
            _inner = inner;
            _queryName = queryName;
            _histogram = histogram;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return await _inner.Handle(request, cancellationToken);
            }
            finally
            {
                sw.Stop();
                _histogram.Observe(sw.ElapsedMilliseconds, _queryName);
            }
        }
    }
}