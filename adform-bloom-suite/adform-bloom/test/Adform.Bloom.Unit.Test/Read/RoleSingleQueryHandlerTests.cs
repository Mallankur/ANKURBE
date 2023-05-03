using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Moq;
using Xunit;
using Role = Adform.Bloom.Contracts.Output.Role;

namespace Adform.Bloom.Unit.Test.Read
{
    public class RoleSingleQueryHandlerTests
    {
        public RoleSingleQueryHandlerTests()
        {
            _handler = new RoleSingleQueryHandler(
                _repositoryMock.Object, _accessRepositoryMock.Object);
        }

        private readonly Mock<IAdminGraphRepository> _repositoryMock = new Mock<IAdminGraphRepository>();

        private readonly Mock<IVisibilityProvider<QueryParamsRoles, Role>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParamsRoles, Role>>();

        private readonly RoleSingleQueryHandler _handler;

        [Fact]
        public async Task Handle_Calls_HasAccess_For_Query()
        {
            // Arrange
            var query = new RoleQuery(
                new ClaimsPrincipal(),
                Guid.NewGuid()
            );

            var entity = new Role
            {
                Id = query.Id,
                Name = "demo",
                Type = RoleCategory.Custom
            };
            _repositoryMock.Setup(r =>
                    r.GetNodeAsync(It.IsAny<Expression<Func<Adform.Bloom.Domain.Entities.Role, bool>>>()))
                .ReturnsAsync(new Adform.Bloom.Domain.Entities.Role {Id = entity.Id, Name = entity.Name});
            _repositoryMock.Setup(r =>
                    r.GetLabelsAsync(It.IsAny<Expression<Func<Adform.Bloom.Domain.Entities.Role, bool>>>()))
                .ReturnsAsync(new string[] {entity.Type.ToString()});
            _accessRepositoryMock
                .Setup(m => m.HasItemVisibilityAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<Guid>(),
                    null))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            _accessRepositoryMock.Verify(
                m => m.HasItemVisibilityAsync(
                    query.Principal,
                    It.Is<Guid>(r => r == query.Id),
                    null), Times.Once);

            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
        }
    }
}