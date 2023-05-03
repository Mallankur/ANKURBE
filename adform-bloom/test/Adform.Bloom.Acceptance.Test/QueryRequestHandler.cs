using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Acceptance.Test
{
    public class QueryRequestHandler : DelegatingHandler
    {
        private readonly string _token;

        public QueryRequestHandler(string token, HttpClientHandler? inner = null) : base(inner ?? new HttpClientHandler())
        {
            _token = token;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return await base.SendAsync(request, cancellationToken);
        }
    }
}