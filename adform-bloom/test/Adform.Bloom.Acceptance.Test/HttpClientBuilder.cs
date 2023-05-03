using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace Adform.Bloom.Acceptance.Test
{
    public class HttpClientBuilder : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        public HttpClient Client { get; }

        public HttpClientBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            var host = _configuration.GetValue<string>("Host");
            Client = new HttpClient
            {
                BaseAddress = new Uri(host)
            };
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}