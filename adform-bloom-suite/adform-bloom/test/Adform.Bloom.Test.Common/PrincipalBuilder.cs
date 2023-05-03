using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Client.Contracts.Services;
using Adform.Bloom.Common.Test.Commons;
using Adform.Bloom.Middleware.Configuration;
using Adform.Bloom.Middleware.Middlewares;
using Adform.Bloom.Runtime.Contracts.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace Adform.Bloom.Common.Test
{
    public class PrincipalBuilder
    {
        private readonly IConfigurationRoot _configuration;
        public Dictionary<string, string> Token;

        public PrincipalBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, ClaimsPrincipal> GeneratePrincipals()
        {
            Token = GetTokens().GetAwaiter().GetResult();
            return new Dictionary<string, ClaimsPrincipal>
            {
                {Graph.Subject0, EnhanceIdentity(Guid.NewGuid().ToString(), Graph.Subject0).GetAwaiter().GetResult()},
                {Graph.Subject1, EnhanceIdentity(Graph.Subject1, null).GetAwaiter().GetResult()},
                {Graph.Subject2, EnhanceIdentity(Guid.NewGuid().ToString(), Graph.Subject2).GetAwaiter().GetResult()},
                {Graph.Subject3, EnhanceIdentity(Graph.Subject3, null).GetAwaiter().GetResult()},
                {Graph.Subject4, EnhanceIdentity(Guid.NewGuid().ToString(), Graph.Subject4).GetAwaiter().GetResult()},
                {Graph.Subject5, EnhanceIdentity(Graph.Subject5, null).GetAwaiter().GetResult()},
                {Graph.Subject6, EnhanceIdentity(Guid.NewGuid().ToString(), Graph.Subject6).GetAwaiter().GetResult()},
                {Graph.Subject8, EnhanceIdentity(Graph.Subject8, null).GetAwaiter().GetResult()},
                {Graph.Subject9, EnhanceIdentity(Graph.Subject9, null).GetAwaiter().GetResult()},
                {Graph.Subject10, EnhanceIdentity(Graph.Subject10, null).GetAwaiter().GetResult()}
            };
        }

        private async Task<ClaimsPrincipal> EnhanceIdentity(string sub, string actorId)
        {
            var context = Common.BuildUser(sub, actorId);
            var nextMock = new Mock<RequestDelegate>(MockBehavior.Loose);
            nextMock.Setup(o => o(It.IsAny<HttpContext>())).Returns(Task.FromResult(0));
            var config = _configuration.GetSection("BloomRuntimeApi").Get<BloomRuntimeOptions>();
            var options = Options.Create(config);

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetSection("BloomRuntimeApi")["Host"])
            };
            httpClient.SetBearerToken(Token[actorId ?? sub]);
            var client = new BloomRuntimeClient(httpClient);
            var middleware = new BloomRuntimeClaimTransformation(client, options);
            return await middleware.TransformAsync(context.User);
        }

        private async Task<Dictionary<string, string>> GetTokens()
        {
            var runtimeScope = _configuration.GetValue<string>("BloomRuntimeApi:Scopes:0");
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
                        Scope = StartupOAuth.Scopes.Readonly + " " + StartupOAuth.Scopes.Full + " " +
                                StartupOAuth.Scopes.FullSubject + " " + runtimeScope
                    });

                if (tokenResponse.IsError)
                {
                    throw new Exception(tokenResponse.Error);
                }

                dictionary.Add(client.Subject, tokenResponse.AccessToken);
            }

            return dictionary;
        }
        
        public IBloomRuntimeClient GetBloomRuntimeClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_configuration.GetSection("BloomRuntimeApi")["Host"])
            };
            httpClient.SetBearerToken(Token[Graph.SubjectUsedByBloomApi]);
            return new BloomRuntimeClient(httpClient);
        }
    }
}