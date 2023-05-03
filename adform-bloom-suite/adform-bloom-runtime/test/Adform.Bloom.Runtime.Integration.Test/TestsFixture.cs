using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Infrastructure.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Bloom.Runtime.Integration.Test.Utils;
using Adform.Bloom.Runtime.Read.Entities;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test
{
    public class TestsFixture : IDisposable
    {
        private HttpClient _oAuthClient;
        private HttpClient _client;
        private IGraphQLClient _graphQlClient;
        private IDriver _driver;

        private string _clientId;
        private string _clientSecret;

        private  IDictionary<string, HttpClient> _clients;
        private  PrincipalBuilder _identities;
        public IRuntimeProvider RuntimeProvider { get; private set; }
        public IAdformTenantProvider TenantProvider { get; private set; }


        public TestsFixture()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.json"), false);
#if !DEBUG
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.testenv.json"), true);
#endif

            var config = configurationBuilder.Build();
            ConfigureOAuth(config);
            ConfigureIdentities(config);
            ConfigureClients(config);
            ConfigureDriver(config);
            ConfigureRepository();
            Seed();
        }



        public void Dispose()
        {
            _oAuthClient?.Dispose();
            _graphQlClient?.Dispose();
            _client?.Dispose();
            _driver?.Dispose();
        }

        public async Task<HttpResponseMessage> SendRestRequestAsync(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request);
            return response;
        }

        public async Task<HttpResponseMessage?> SendRestRequestAsync(string actor, HttpRequestMessage request)
        {
            if (!_clients.ContainsKey(actor)) return null;
            return await _clients[actor].SendAsync(request);
        }

        public async Task<IReadOnlyList<RuntimeResult>> SendGraphQlRequestAsync(GraphQLRequest request)
        {
            var response = await _graphQlClient.SendQueryAsync<RuntimeEvaluationResult>(request);
            return response.Data.SubjectEvaluation;
        }

        public async Task<ExistenceResult?> SendRoleExistenceCheckGraphQlQueryAsync(GraphQLRequest request)
        {
            var response = await _graphQlClient.SendQueryAsync<RoleExistenceCheckResult>(request);
            return response.Errors == null ? response.Data.RoleExistsCheck : null;
        }

        public async Task<ExistenceResult?> SendNodesExistenceCheckGraphQlQueryAsync(GraphQLRequest request)
        {
            var response = await _graphQlClient.SendQueryAsync<NodesExistenceCheckResult>(request);
            return response.Errors == null ? response.Data.NodesExistCheck : null;
        }

        public async Task<ExistenceResult?> SendLegacyTenantsExistenceCheckGraphQlQueryAsync(GraphQLRequest request)
        {
            var response = await _graphQlClient.SendQueryAsync<LegacyTenantsExistenceCheckResult>(request);
            return response.Errors == null ? response.Data.LegacyTenantsExistCheck : null;
        }

        public async Task<string> GetToken()
        {
            var tokenResponse = await _oAuthClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Scope = StartupOAuth.Scopes.Readonly
            });

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            return tokenResponse.AccessToken;
        }

        private void ConfigureOAuth(IConfigurationRoot config)
        {
            var oAuthHost = config.GetValue<string>("OAuth2:TokenEndpointUri");
            _oAuthClient = new HttpClient
            {
                BaseAddress = new Uri(oAuthHost)
            };
            _clientId = config.GetValue<string>("OAuth2:ClientId");
            _clientSecret = config.GetValue<string>("OAuth2:ClientSecret");
        }

        private void ConfigureIdentities(IConfigurationRoot config)
        {
            _identities = new PrincipalBuilder(config);
        }

        private void ConfigureClients(IConfigurationRoot config)
        {
            var host = config.GetValue<string>("Host");
            var token = GetToken().GetAwaiter().GetResult();

            _client = CofigureClient(host, token);

            _clients = new Dictionary<string, HttpClient>();
            foreach (var identity in _identities.Token)
            {
                _clients.Add(identity.Key, CofigureClient(host, identity.Value));
            }

            _graphQlClient = ConfigureGQLClient(host, token);
        }

        private HttpClient CofigureClient(string host, string token)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(host)
            };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }

        private IGraphQLClient ConfigureGQLClient(string host, string token)
        {
            var graphQLOptions = new GraphQLHttpClientOptions
            {
                EndPoint = new Uri($"{host}/graphql"),
                HttpMessageHandler = new QueryRequestHandler(token),
            };
            return new GraphQLHttpClient(graphQLOptions, new NewtonsoftJsonSerializer());
        }

        private void ConfigureDriver(IConfigurationRoot config)
        {
            var ongDbHost = config.GetValue<string>("OngDB:Host");
            var ongDbUsername = config.GetValue<string>("OngDB:Username");
            var ongDbPassword = config.GetValue<string>("OngDB:Password");

            _driver = GraphDatabase.Driver(ongDbHost, AuthTokens.Basic(ongDbUsername, ongDbPassword));
        }

        private void ConfigureRepository()
        {
            var cursorConverter = new CursorToResultConverter();
            RuntimeProvider = new RuntimeProvider(_driver, cursorConverter);
            TenantProvider = new AdformTenantProvider(_driver, cursorConverter);
        }

        private void Seed()
        {
            var seedQuery = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "init.cypher"));
            var seed2Query = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "init2.cypher"));

            var session = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Read));
            try
            {
                session.RunAsync("MATCH (n) DETACH DELETE n;").GetAwaiter().GetResult();
                session.RunAsync(seedQuery).GetAwaiter().GetResult();
                session.RunAsync(seed2Query).GetAwaiter().GetResult();
            }
            finally
            {
                session.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}