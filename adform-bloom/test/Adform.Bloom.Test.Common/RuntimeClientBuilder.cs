using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Adform.Bloom.Client.Contracts.Services;
using Adform.Bloom.Common.Test.Commons;
using Adform.Bloom.Runtime.Contracts.Services;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;

namespace Adform.Bloom.Common.Test
{
    public class RuntimeClientBuilder
    {
        private readonly IConfigurationRoot _configuration;
        public IBloomRuntimeClient Client { get; }

        public RuntimeClientBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            var host = _configuration.GetSection("BloomRuntimeApi")["Host"];
            var token = GetToken("BloomRuntimeApi:Scopes:0", "ReadModel:OAuth2").GetAwaiter().GetResult();
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(host)
            };
            httpClient.SetBearerToken(token);
            Client = new BloomRuntimeClient(httpClient);
        }

        private async Task<string> GetToken(string scopePath, string oAuthConfigPath)
        {
            var scope = _configuration.GetValue<string>(scopePath);
            var configuration = _configuration.GetSection(oAuthConfigPath).Get<TestOAuth>();
            var oAuthClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.TokenEndpointUri)
            };
            var client = configuration.Clients.First();
            var tokenResponse = await oAuthClient.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    ClientId = client.ClientId,
                    ClientSecret = client.ClientSecret,
                    Scope = scope
                });

            if (tokenResponse.IsError) throw new Exception(tokenResponse.Error);

            return tokenResponse.AccessToken;
        }
    }
}