using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Infrastructure.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using AutoFixture;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Infrastructure
{
    public class ExistenceProviderTests
    {
        private readonly ExistenceProvider _provider;
        private readonly Mock<IDriver> _driver;
        private readonly Mock<ICursorToResultConverter> _converter;
        private readonly Fixture _fixture;

        public ExistenceProviderTests()
        {
            _driver = new Mock<IDriver>();
            _driver.SetupAllProperties();
            _converter = new Mock<ICursorToResultConverter>();
            _provider = new ExistenceProvider(_driver.Object, _converter.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CheckExistence_WithUnsupportedQuery_ShouldFail()
        {
            var query = new FakeQuery();

            var result = await _provider.CheckExistence(query);
            var expectedResult = Result.Fail(new ExceptionalError(new NotSupportedException($"ExistenceProvider does not support query:{query.GetType().FullName}")));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public async Task CheckExistence_RoleQuery_HappyPath_ReturnsSuccessfulResult(int count)
        {
            var roleQuery = _fixture.Create<RoleExistenceQuery>();
            roleQuery.TenantId = Guid.NewGuid();
            const string cypherQuery = "MATCH (x:Tenant {Id: $tenantId})-[:OWNS]->(r:Role {Name: $roleName}) RETURN count(*)";
            var parameters = new Dictionary<string, object>
            {
                {"tenantId", roleQuery.TenantId.ToString()},
                {"roleName", roleQuery.RoleName}
            };
            
            var cursor = new Mock<IResultCursor>();
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            session.Setup(o => o.RunAsync(cypherQuery, Common.Matches(parameters)))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);
            _converter.Setup(c => c.ConvertToCountResult(cursor.Object)).ReturnsAsync(count);

            var result = await _provider.CheckExistence(roleQuery);

            result.Should().BeEquivalentTo(Result.Ok<bool>(count == 1));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckExistence_LegacyTenantQuery_HappyPath_ReturnsSuccessfulResult(bool matchLegacyListCount)
        {
            var legacyTenantQuery = _fixture.Create<LegacyTenantExistenceQuery>();
            var cypherQuery = $"MATCH (x:Tenant:{legacyTenantQuery.TenantType}) WHERE x.LegacyId in $legacyIds RETURN count(*)";
            var parameters = new Dictionary<string, object>
            {
                {"legacyIds", legacyTenantQuery.TenantLegacyIds},
            };

            var cursor = new Mock<IResultCursor>();
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            session.Setup(o => o.RunAsync(cypherQuery, Common.Matches(parameters)))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);
            var count = matchLegacyListCount
                ? legacyTenantQuery.TenantLegacyIds.Count
                : legacyTenantQuery.TenantLegacyIds.Count + 1;
            _converter.Setup(c => c.ConvertToCountResult(cursor.Object)).ReturnsAsync(count);

            var result = await _provider.CheckExistence(legacyTenantQuery);

            result.Should().BeEquivalentTo(Result.Ok<bool>(matchLegacyListCount));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CheckExistence_NodeExistenceQuery_HappyPath_ReturnsSuccessfulResult(bool matchNodeListCount)
        {
            var nodeExistenceQuery = new NodeExistenceQuery
            {
                NodeDescriptors = new List<NodeDescriptor>
                {
                    new NodeDescriptor {Label = _fixture.Create<string>(), Id = _fixture.Create<Guid>()},
                    new NodeDescriptor {Label = _fixture.Create<string>(), UniqueName = _fixture.Create<string>()},
                    new NodeDescriptor {Label = _fixture.Create<string>(), Id = _fixture.Create<Guid>(), UniqueName = _fixture.Create<string>()},
                }
            };
            var aux = nodeExistenceQuery.NodeDescriptors;
            var cypherQuery =
                $"MATCH (x) WHERE (x:{aux[0].Label} AND x.Id = \"{aux[0].Id}\") OR (x:{aux[1].Label} AND x.Name = \"{aux[1].UniqueName}\") OR (x:{aux[2].Label} AND x.Id = \"{aux[2].Id}\" AND x.Name = \"{aux[2].UniqueName}\") RETURN count(*)";

            var cursor = new Mock<IResultCursor>();
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            session.Setup(o => o.RunAsync(cypherQuery))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);
            var count = matchNodeListCount
                ? aux.Count
                : aux.Count + 1;
            _converter.Setup(c => c.ConvertToCountResult(cursor.Object)).ReturnsAsync(count);

            var result = await _provider.CheckExistence(nodeExistenceQuery);

            result.Should().BeEquivalentTo(Result.Ok<bool>(matchNodeListCount));
        }

        [Theory]
        [MemberData(nameof(ExceptionScenarios), MemberType = typeof(ExistenceProviderTests))]
        public async Task CheckExistence_SessionThrows_ReturnsFailureResult(IRequest<Result<bool>> query, Dictionary<string, object> parameters)
        {
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var exception = _fixture.Create<Exception>();
            if (parameters != null)
                session.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                    .ThrowsAsync(exception);
            else
                session.Setup(o => o.RunAsync(It.IsAny<string>()))
                    .ThrowsAsync(exception);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            var result = await _provider.CheckExistence(query);

            var expectedResult = Result.Fail(new ExceptionalError(exception));
            result.IsFailed.Should().Be(true);
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [MemberData(nameof(ExceptionScenarios), MemberType = typeof(ExistenceProviderTests))]
        public async Task CheckExistence_ConverterThrows_ReturnsFailureResult(IRequest<Result<bool>> query, Dictionary<string, object> parameters)
        {
            var cursor = new Mock<IResultCursor>();
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            if (parameters != null)
                session.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                    .ReturnsAsync(cursor.Object);
            else
                session.Setup(o => o.RunAsync(It.IsAny<string>()))
                    .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);
            var exception = _fixture.Create<Exception>();
            _converter.Setup(c => c.ConvertToCountResult(cursor.Object)).Throws(exception);

            var result = await _provider.CheckExistence(query);

            var expectedResult = Result.Fail(new ExceptionalError(exception));
            result.IsFailed.Should().Be(true);
            result.Should().BeEquivalentTo(expectedResult);
        }

        public static TheoryData<IRequest<Result<bool>>, Dictionary<string, object>> ExceptionScenarios()
        {
            var data = new TheoryData<IRequest<Result<bool>>, Dictionary<string, object>>
            {
                {new LegacyTenantExistenceQuery{ TenantLegacyIds = new List<int>{1}}, new Dictionary<string, object>()},
                {new RoleExistenceQuery(), new Dictionary<string, object>()},
                {new NodeExistenceQuery {NodeDescriptors = new List<NodeDescriptor> {new NodeDescriptor()}}, null}
            };

            return data;
        }
    }

    public class FakeQuery : IRequest<Result<bool>>
    {
    }
}
