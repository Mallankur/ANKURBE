using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Integration.Test.Seed;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Adform.Bloom.Runtime.Integration.Test.Utils
{
    public class PrincipalBuilder
    {
        private readonly IConfigurationRoot _configuration;
        public Dictionary<string, string> Token;
        public PrincipalBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            Token = GetTokens().GetAwaiter().GetResult();
        }
        
        public Dictionary<string, ClaimsPrincipal> GeneratePrincipals()
        {
            
            return new Dictionary<string, ClaimsPrincipal>
            {
                {Graph.Subject1, EnhanceIdentity(Graph.Subject1).GetAwaiter().GetResult()},
                {Graph.Subject2, EnhanceIdentity(Graph.Subject2).GetAwaiter().GetResult()},
                {Graph.Subject3, EnhanceIdentity(Graph.Subject3).GetAwaiter().GetResult()},
                {Graph.Subject4, EnhanceIdentity(Graph.Subject4).GetAwaiter().GetResult()},
                {Graph.Subject5, EnhanceIdentity(Graph.Subject5).GetAwaiter().GetResult()},
                {Graph.Subject6, EnhanceIdentity( Graph.Subject6).GetAwaiter().GetResult()}
            };
        }

        private async Task<ClaimsPrincipal> EnhanceIdentity(string actorId)
        {
            var context = Common.BuildUser(actorId);
            var nextMock = new Mock<RequestDelegate>(MockBehavior.Loose);
            nextMock.Setup(o => o(It.IsAny<HttpContext>())).Returns(Task.FromResult(0));
            return context.User;
        }
        
        private async Task<Dictionary<string, string>> GetTokens()
        {
            var configuration = _configuration.GetSection("OAuth2").Get<TestOAuth>();
            var oAuthClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.TokenEndpointUri)
            };
            var dictionary = new Dictionary<string, string>();
            foreach (var client in configuration.Clients)
            {
                var tokenResponse = await oAuthClient.RequestClientCredentialsTokenAsync(
                    new ClientCredentialsTokenRequest
                    {
                        ClientId = client.ClientId,
                        ClientSecret = client.ClientSecret,
                        Scope = StartupOAuth.Scopes.Readonly
                    });

                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.Error);
                }

                dictionary.Add(client.Subject, tokenResponse.AccessToken);
            }

            return dictionary;
        }
    }
}