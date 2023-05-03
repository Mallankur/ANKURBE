using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

namespace Adform.Bloom.Common.Test
{
    public class OngBuilder:IDisposable
    {
        private readonly IConfigurationRoot _configuration;
        public readonly string DeleteQuery = "MATCH (n) DETACH DELETE n;";
        public ITransactionalGraphClient GraphClient { get; }
        public AdminGraphRepository GraphRepository { get; }
        public VisibilityRepositoriesContainer VisibilityRepositoriesContainer { get; }
        public AccessRepositoriesContainer AccessRepositoriesContainer { get; }

        public OngBuilder(IConfigurationRoot configuration)
        {
            _configuration = configuration;
            var ongDbHost = _configuration.GetValue<string>("OngDB:Host");
            var ongDbUsername = _configuration.GetValue<string>("OngDB:Username");
            var ongDbPassword = _configuration.GetValue<string>("OngDB:Password");

            var driver = GraphDatabase.Driver(ongDbHost, AuthTokens.Basic(ongDbUsername, ongDbPassword));
            GraphClient = new BoltGraphClient(driver);
            GraphRepository = new AdminGraphRepository(GraphClient);
            VisibilityRepositoriesContainer = new VisibilityRepositoriesContainer(GraphClient);
            AccessRepositoriesContainer = new AccessRepositoriesContainer(GraphClient);
        }

        public async Task Seed()
        {
            var seedQueryString =
                File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Seed", "init.cypher"));
            var seedQuery =
                new CypherQuery(seedQueryString, new Dictionary<string, object>(), CypherResultMode.Set, "");
            await ((IRawGraphClient) await GraphRepository.GraphClient).ExecuteCypherAsync(seedQuery);
        }

        public async Task Clean()
        {
            var deleteQuery = new CypherQuery(DeleteQuery, new Dictionary<string, object>(), CypherResultMode.Set, "");
            await ((IRawGraphClient) await GraphRepository.GraphClient).ExecuteCypherAsync(deleteQuery);
        }

        public void Dispose()
        {
            GraphClient?.Dispose();
        }
    }
}