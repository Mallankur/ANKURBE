using System;
using System.Collections.Generic;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;

namespace Adform.Bloom.Acceptance.Test
{
    public class GraphQLClientBuilder : IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        public Dictionary<string, IGraphQLClient> GQLClient;

        public GraphQLClientBuilder(IConfigurationRoot configuration, Dictionary<string, string> identitiesToken)
        {
            _configuration = configuration;
            var host = _configuration.GetValue<string>("Host");
            GQLClient = new Dictionary<string, IGraphQLClient>();
            foreach (var identity in identitiesToken)
            {
                var graphQLOptions = new GraphQLHttpClientOptions
                {
                    EndPoint = new Uri($"{host}/graphql"),
                    HttpMessageHandler = new QueryRequestHandler(identity.Value)
                };
                GQLClient.Add(identity.Key, new GraphQLHttpClient(graphQLOptions, new NewtonsoftJsonSerializer()));
            }
        }

        public void Dispose()
        {
            foreach (var client in GQLClient)
            {
                client.Value?.Dispose();
            }
        }
    }
}