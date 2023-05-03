using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Transactions
{
    [Collection(nameof(DatabaseCollection))]
    public class ConstrainsTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;

        public ConstrainsTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData("Permission", "test")]
        [InlineData("Tenant", "test")]
        [InlineData("Policy", "test")]
        [Order(0)]
        public async Task OnDuplicatePropertyThrowException(string entity, string name)
        {
            // Arrange
            var constrain = $"CREATE CONSTRAINT ON (n:{entity}) ASSERT n.Name IS UNIQUE";
            var query = $"CREATE (p:{entity} {{Name: '{name}'}})";

            var constrainQuery = new CypherQuery(constrain, new Dictionary<string, object>(), CypherResultMode.Set, "");
            await ((IRawGraphClient) _fixture.GraphClient).ExecuteCypherAsync(constrainQuery);

            var creatQuery = new CypherQuery(query, new Dictionary<string, object>(), CypherResultMode.Set, "");
            await ((IRawGraphClient) _fixture.GraphClient).ExecuteCypherAsync(creatQuery);
            // Act & Assert
            Assert.ThrowsAsync<ClientException>(async () =>
                await ((IRawGraphClient) _fixture.GraphClient).ExecuteCypherAsync(creatQuery));

            //CLEAN
            var drop = $"DROP CONSTRAINT ON(n: {entity}) ASSERT n.Name IS UNIQUE";
            await ((IRawGraphClient) _fixture.GraphClient).ExecuteCypherAsync(new CypherQuery(drop,
                new Dictionary<string, object>(), CypherResultMode.Set, ""));
        }
    }
}