using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Runtime.Contracts.Services;
using GraphQL;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Adform.Bloom.Acceptance.Test
{
    public class TestsFixture : IDisposable
    {
        private IConfigurationRoot Configuration;
        public readonly HttpClient RestClient;
        public Dictionary<string, ClaimsPrincipal> BloomApiPrincipal;

        public readonly OngBuilder OngDB;
        public readonly PsqlBuilder SQL;
        public readonly PrincipalBuilder Identities;
        public readonly Dictionary<string, IGraphQLClient> GQLClient;
        public HttpClientBuilder RestClientBuilder;
        public GraphQLClientBuilder GraphqlClientBuilder;
        public RuntimeClientBuilder RuntimeClientBuilder;
        public readonly IBloomRuntimeClient RuntimeClient;

        public TestsFixture()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.json"), false);
#if !DEBUG
            configurationBuilder.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "testsettings.testenv.json"), true);
#endif

            Configuration = configurationBuilder.Build();

            //Ongdb
            OngDB = new OngBuilder(Configuration);
            OngDB.Clean().GetAwaiter().GetResult();
            OngDB.Seed();

            //Postgresql
            SQL = new PsqlBuilder(Configuration);
            SQL.Clean();
            SQL.SeedUserData();
            SQL.SeedBusinessAccountData();

            //Identity
            Identities = new PrincipalBuilder(Configuration);
            BloomApiPrincipal = Identities.GeneratePrincipals();

            //Client
            RestClientBuilder = new HttpClientBuilder(Configuration);
            GraphqlClientBuilder = new GraphQLClientBuilder(Configuration, Identities.Token);
            RuntimeClientBuilder = new RuntimeClientBuilder(Configuration);
            RestClient = RestClientBuilder.Client;
            GQLClient = GraphqlClientBuilder.GQLClient;
            RuntimeClient = RuntimeClientBuilder.Client;
        }

        public async Task<dynamic> SendGraphqlRequestAsync(string sub, GraphQLRequest request,
            bool withErrorResponse = false)
        {
            try
            {
                var client = GQLClient[sub];
                var response = await client.SendQueryAsync<dynamic>(request);
                return withErrorResponse ? response : response.Data;
            }
            catch (GraphQLHttpRequestException ex)
            {
                var token = JToken.Parse(ex.Content);
                return token.ToObject<GraphQLResponse<dynamic>>();
            }
        }

        public IDictionary<string, string> ExtractGraphqlErrorsData(GraphQLResponse<dynamic> response)
        {
            var errorData = response.Errors.SelectMany(e => e.Extensions);
            var errData = errorData.First(o => o.Key == "data").Value as Dictionary<string, object>;
            return errData.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }

        public IDictionary<string, string> ExtractGraphqlErrorsExtensions(GraphQLResponse<dynamic> response)
        {
            return response.Errors.SelectMany(e => e.Extensions)
                .ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
        }

        public void Dispose()
        {
            RestClientBuilder.Dispose();
            GraphqlClientBuilder.Dispose();
        }
    }
}