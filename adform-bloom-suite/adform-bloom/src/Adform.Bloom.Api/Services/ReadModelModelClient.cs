using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Adform.Bloom.Api.Services
{
    public class ReadModelModelClient : IReadModelClient
    {
        private readonly HttpClient _client;
        public ReadModelModelClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<bool> IsHealthy()
        {
            var response = await _client.GetAsync("/healthy");
            var res = await response.Content.ReadAsStringAsync();
            return string.Equals(res, HealthStatus.Healthy.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}