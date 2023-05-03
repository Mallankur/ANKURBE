using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Handlers;
using Adform.Bloom.Application.Queries;
using FluentAssertions;
using FluentResults;
using MediatR;
using Moq;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Handlers
{
    public class ExistenceQueryHandlerTests
    {
        private readonly Mock<IExistenceProvider> _existenceProvider;

        public ExistenceQueryHandlerTests()
        {
            _existenceProvider = new Mock<IExistenceProvider>();
        }

        [Theory]
        [MemberData(nameof(TestCases), MemberType = typeof(ExistenceQueryHandlerTests))]
        public async Task ExistenceQueryHandler_ReturnsExistenceProviderResult(IRequest<Result<bool>> query, bool expectedResult)
        {
            _existenceProvider
                .Setup(e => e.CheckExistence(query,It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await ExecuteQuery(query);

            result.Value.Should().Be(expectedResult);
        }

        private async Task<Result<bool>> ExecuteQuery(IRequest<Result<bool>> query)
        {
            return query switch
            {
                NodeExistenceQuery nodeExistenceQuery => await new NodeExistenceQueryHandler(_existenceProvider.Object)
                    .Handle(nodeExistenceQuery, CancellationToken.None),
                RoleExistenceQuery roleExistenceQuery => await new RoleExistenceQueryHandler(_existenceProvider.Object)
                    .Handle(roleExistenceQuery, CancellationToken.None),
                LegacyTenantExistenceQuery legacyTenantExistenceQuery => await
                    new LegacyTenantExistenceQueryHandler(_existenceProvider.Object).Handle(legacyTenantExistenceQuery,
                        CancellationToken.None),
                _ => new Result<bool>()
            };
        }

        public static TheoryData<IRequest<Result<bool>>, bool> TestCases()
        {
            var result = new TheoryData<IRequest<Result<bool>>, bool>
            {
                { new RoleExistenceQuery(), false },
                {new RoleExistenceQuery(), true},
                {new NodeExistenceQuery(), false},
                {new NodeExistenceQuery(), true},
                {new LegacyTenantExistenceQuery(), false},
                {new LegacyTenantExistenceQuery(), true}
            };

            return result;
        }
    }
}
