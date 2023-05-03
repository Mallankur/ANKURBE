using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Infrastructure;
using Adform.Bloom.Runtime.Infrastructure.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Ciam.OngDb.Core.Extensions;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Infrastructure
{
    public class AdformTenantProviderTests
    {
        private readonly Mock<IDriver> _driver;

        public AdformTenantProviderTests()
        {
            _driver = new Mock<IDriver>();
            _driver.SetupAllProperties();
        }

        [Theory]
        [MemberData(nameof(Common.Data), MemberType = typeof(Common))]
        public async Task GetAdformTenant_Returns_Expected_Result(Common.TestData data)
        {
            // Arrange
            var output = data.Input.TenantIds.First();
            var mapService = new Mock<ICursorToResultConverter>();
            mapService
                .Setup(o => o.ConvertToTenantIdAsync(It.IsAny<IResultCursor>()))
                .ReturnsAsync(output);
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var cursor = new Mock<IResultCursor>();
            session.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            var parameters = new Dictionary<string, object>();
            parameters.Add("subjectId", data.Input.SubjectId.ToString());
            var query = new StringBuilder();
            query.Append(Queries.BaseQuery());
            query.Append(Queries.Result);
            var provider = new AdformTenantProvider(_driver.Object, mapService.Object);
            // Act
            var cypherResult = await provider.GetAdformTenant(data.Input.SubjectId);

            // Assert
            var actualTenant = cypherResult;
            Assert.Equal(output, actualTenant);
            mapService.Verify(o => o.ConvertToTenantIdAsync(It.IsAny<IResultCursor>()), Times.Once);
            session.Verify(
                o => o.RunAsync(It.Is<string>(m => m == query.ToString()),
                    It.Is<Dictionary<string, object>>(d => d.Keys.SequenceEqual(parameters.Keys))), Times.Once);
        }

        public static class Queries
        {
            public static Func<string> BaseQuery = () =>
                $"MATCH (s:Subject {{Id: $subjectId}}){Constants.MemberOfLink.ToCypher()}(g:Group){Constants.BelongsLink.ToCypher()}(t:Tenant:{Constants.AdformLabel})";

            public static string Result =
                " RETURN t.Id as TenantId LIMIT 1";
        }
    }
}