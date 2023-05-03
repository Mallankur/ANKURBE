using System.Threading;
using System.Threading.Tasks;
using Adform.Ciam.TokenProvider.Configuration;
using Adform.Ciam.TokenProvider.Services;
using Grpc.Core;
using Microsoft.Extensions.Options;
using ProtoBuf.Grpc;

namespace Adform.Bloom.DataAccess.Extensions
{
    public class CallContextEnhancer : ICallContextEnhancer
    {
        private readonly OAuth2Configuration _options;
        private readonly ITokenProvider _provider;
        private readonly BloomReadClientSettings _client;

        public CallContextEnhancer(ITokenProvider provider, IOptions<OAuth2Configuration> options, IOptions<BloomReadClientSettings> client)
        {
            _options = options.Value;
            _provider = provider;
            _client = client.Value;
        }
        public async Task<CallContext> Build(CancellationToken cancellation = default)
        {
            var metadata = new Metadata
            {
                { "Authorization", $"Bearer {await GetTokenAsync()}" }
            };
            var options = new CallOptions(metadata, cancellationToken: cancellation);
            return new CallContext(options);
        }

        private async Task<string> GetTokenAsync()
        {
            return await _provider.RequestTokenAsync(_options.ClientId, _client.Scopes);
        }
    }
}