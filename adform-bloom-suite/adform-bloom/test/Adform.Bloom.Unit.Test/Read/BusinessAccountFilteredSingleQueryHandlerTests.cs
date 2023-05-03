using System;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using MapsterMapper;
using Moq;
using Xunit;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.Unit.Test.Read
{
    public class BusinessAccountFilteredSingleQueryHandlerTests
    {
        private readonly Mock<IAdminGraphRepository> _repositoryMock = new Mock<IAdminGraphRepository>();

        private readonly Mock<IBusinessAccountReadModelProvider> _businessAccountReadModelProviderMock =
            new Mock<IBusinessAccountReadModelProvider>();

        private readonly Mock<IVisibilityProvider<QueryParamsBusinessAccount, Tenant>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParamsBusinessAccount, Tenant>>();

        private readonly BusinessAccountSingleQueryHandler _handler;

        public BusinessAccountFilteredSingleQueryHandlerTests()
        {
            var mapper = new Mapper();
            _handler = new BusinessAccountSingleQueryHandler(
                _repositoryMock.Object, _accessRepositoryMock.Object, _businessAccountReadModelProviderMock.Object,
                mapper);
        }

        [Fact]
        public async Task Handle_Calls_HasAccess_For_Query()
        {
            // Arrange
            var query = new BusinessAccountQuery(
                new ClaimsPrincipal(),
                Guid.NewGuid()
            );

            var entity = new BusinessAccount
            {
                Id = query.Id,
                Name = "demo",
                LegacyId = 0
            };
            _repositoryMock.Setup(r =>
                    r.GetNodeAsync(It.IsAny<Expression<Func<Adform.Bloom.Domain.Entities.Tenant, bool>>>()))
                .ReturnsAsync(new Adform.Bloom.Domain.Entities.Tenant {Id = entity.Id});
            _accessRepositoryMock
                .Setup(m => m.HasVisibilityAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<QueryParamsBusinessAccount>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);
            _businessAccountReadModelProviderMock
                .Setup(o => o.SearchForResourceAsync(query.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            _accessRepositoryMock.Verify(
                m => m.HasVisibilityAsync(
                    query.Principal, 
                    It.Is<QueryParamsBusinessAccount>(q =>
                        q.ResourceIds.Count == 1 && q.ResourceIds.Contains(query.Id) 
                        ),
                    null), Times.Once);
            _businessAccountReadModelProviderMock.Verify(
                m => m.SearchForResourceAsync(It.Is<Guid>(r => r == query.Id), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.LegacyId, result.LegacyId);
            Assert.Equal(entity.Name, result.Name);
        }
    }
}