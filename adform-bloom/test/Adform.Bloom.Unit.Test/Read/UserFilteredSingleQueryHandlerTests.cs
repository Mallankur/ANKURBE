using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Moq;
using Xunit;
using Subject = Adform.Bloom.Contracts.Output.Subject;
using User = Adform.Bloom.Contracts.Output.User;

namespace Adform.Bloom.Unit.Test.Read
{
    public class UserFilteredSingleQueryHandlerTests
    {
        public UserFilteredSingleQueryHandlerTests()
        {
            _handler = new UserSingleQueryHandler(
                _repositoryMock.Object, _accessRepositoryMock.Object, _userReadModelProviderMock.Object);
        }

        private readonly Mock<IAdminGraphRepository> _repositoryMock = new Mock<IAdminGraphRepository>();

        private readonly Mock<IUserReadModelProvider> _userReadModelProviderMock = new Mock<IUserReadModelProvider>();

        private readonly Mock<IVisibilityProvider<QueryParamsTenantIds, Subject>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParamsTenantIds, Subject>>();

        private readonly UserSingleQueryHandler _handler;

        [Fact]
        public async Task Handle_Calls_HasAccess_For_Query()
        {
            // Arrange
            var query = new UserQuery(
                new ClaimsPrincipal(),
                Guid.NewGuid()
            );

            var entity = new User
            {
                Id = query.Id,
                Username = "aaa",
                Name = "demo",
                Email = "a@adform.com"
            };
            _repositoryMock.Setup(r =>
                    r.GetNodeAsync(It.IsAny<Expression<Func<Adform.Bloom.Infrastructure.Models.Subject, bool>>>()))
                .ReturnsAsync(new Adform.Bloom.Infrastructure.Models.Subject {Id = entity.Id});
            _accessRepositoryMock
                .Setup(m => m.HasVisibilityAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<QueryParamsTenantIds>(),
                    It.IsAny<string>()))
                .ReturnsAsync(true);
            _userReadModelProviderMock.Setup(o => o.SearchForResourceAsync(query.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            _accessRepositoryMock.Verify(
                m => m.HasVisibilityAsync(
                    query.Principal,
                    It.Is<IReadOnlyCollection<Guid>>(r => r.Count == 1 && r.Contains(query.Id)),
                    query.TenantIds,
                    null), Times.Once);
            _userReadModelProviderMock.Verify(
                m => m.SearchForResourceAsync(It.Is<Guid>(r => r == (query.Id)), It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Username, result.Username);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Email, result.Email);
        }
    }
}