using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class TenantCommandHandlersTests : IClassFixture<TestsFixture>
    {
        private readonly CreateWithParentIdCommandHandler<CreateTenantCommand, Tenant> _createHandler;
        private readonly TestsFixture _fixture;

        public TenantCommandHandlersTests(TestsFixture fixture)
        {
            _fixture = fixture;
            var mediator = new Mock<IMediator>().Object;
            _createHandler = new CreateWithParentIdCommandHandler<CreateTenantCommand, Tenant>(
                new NamedNodeMapper<CreateTenantCommand, Tenant>(), 
                fixture.GraphRepository, 
                mediator);
        }

        [Theory]
        [InlineData(Graph.Tenant1)]
        [InlineData(Graph.Tenant2)]
        [InlineData(Graph.Tenant3)]
        public async Task CreateTenantWithParentIdSucceedsWhenParentExist(Guid parentId)
        {
            // Arrange
            const string tenantName = "TenantX";

            // Act
            var created = await _createHandler.Handle(
                new CreateTenantCommand(
                    _fixture.BloomApiPrincipal[Graph.Subject0],
                    parentId,
                    tenantName), CancellationToken.None);
            var tenantId = created.Id;

            // Assert
            var tenant = await _fixture.GraphRepository.GetNodeAsync<Tenant>(r => r.Id == tenantId);
            Assert.StartsWith(tenantName, tenant.Name);

            var tenants =
                await _fixture.GraphRepository.GetConnectedAsync<Tenant, Tenant>(r => r.Id == tenantId,
                    Constants.ChildOfLink);
            Assert.Equal(parentId, tenants.First().Id);
        }

        [Fact]
        public async Task CreateTenantWithParentIdThrowsExceptionWhenParentDoesntExist()
        {
            // Arrange
            const string tenantName = "TenantY";
            var parentId = Guid.Empty;

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _createHandler.Handle(
                new CreateTenantCommand(_fixture.BloomApiPrincipal[Graph.Subject0], parentId,
                    tenantName), CancellationToken.None));

            var tenant = await _fixture.GraphRepository.GetNodeAsync<Tenant>(r => r.Name == tenantName);
            Assert.Null(tenant);
        }

    }
}