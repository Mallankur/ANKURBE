using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class UpdateRoleCommandHandlerTests : BaseTests
    {
        public UpdateRoleCommandHandlerTests()
        {
            _handler = new UpdateRoleCommandHandler(
                _adminGraphRepositoryMock.Object,
                _mediatorMock.Object,
                _accessValidatorMock.Object,
                new NamedNodeMapper<UpdateRoleCommand, Role>(),
                _bloomCacheManagerMock.Object);
        }

        private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();
        private readonly Mock<IBloomCacheManager> _bloomCacheManagerMock = new Mock<IBloomCacheManager>();

        private readonly UpdateRoleCommandHandler _handler;

        private readonly IReadOnlyCollection<Guid> _assignments = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

       

        [Fact]
        public async Task Handle_Update_Item()
        {
            var (roleToBeUpdated, cmd) = SetupUpdateRoleCommand();

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertUpdateItem(res, roleToBeUpdated);

            _mediatorMock.AssertAuditEventWasPublished<Role>(roleToBeUpdated.Id, _claimsPrincipal,
                AuditOperation.Update);

            _bloomCacheManagerMock.Verify(p => p.ForgetByRoleAsync(It.Is<string>(p => p == res.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Throws_Forbidden_Exception_If_Principal_Has_No_Access_To_Role()
        {
            var (_, cmd) = SetupUpdateRoleCommand(hasAccessToRole: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

        [Fact]
        public async Task Handle_Throws_Bad_Request_Exception_If_Role_Version_Does_Not_Exist()
        {
            var (_, cmd) = SetupUpdateRoleCommand(concurrency: true);

            await Assert.ThrowsAsync<ConflictException>(
                async () => await _handler.Handle(cmd, CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

        [Fact]
        public async Task Handle_Throws_Not_Found_Exception_If_Role_Does_Not_Exist()
        {
            var (_, cmd) = SetupUpdateRoleCommand(roleExists: false);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(cmd, CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

        [Theory]
        [InlineData("")]
        [InlineData("<script")]
        public async Task Handle_Throws_Bad_Request_Exception_For_Invalid_Name(string name)
        {
            var (_, cmd) = SetupUpdateRoleCommand(role: new Role
            {
                Name = name,
            });

            var exception =
                await Assert.ThrowsAsync<BadRequestException>(async () =>
                    await _handler.Handle(cmd, CancellationToken.None));
            Assert.True(exception.Params.ContainsKey("name"));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

        [Theory]
        [InlineData("<script")]
        public async Task Handle_Throws_Bad_Request_Exception_For_Invalid_Description(string description)
        {
            var (_, cmd) = SetupUpdateRoleCommand(role: new Role
            {
                Name = Guid.NewGuid().ToString(),
                Description = description
            });

            var exception =
                await Assert.ThrowsAsync<BadRequestException>(async () =>
                    await _handler.Handle(cmd, CancellationToken.None));
            Assert.True(exception.Params.ContainsKey("description"));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

      

        private (Role roleToBeCreated, UpdateRoleCommand command) SetupUpdateRoleCommand(
           bool concurrency = false,
           bool updateRoleThrowsException = false,
           bool roleExists = true,
           bool hasAccessToRole = true,
           Role role = null)
        {
            var roleToBeUpdated = role ?? new Role("aaa");
            var tenantId = Guid.NewGuid();

            var cmd = new UpdateRoleCommand(
                Common.BuildPrincipal(tenantId.ToString()),
                roleToBeUpdated.Id,
                roleToBeUpdated.Name,
                roleToBeUpdated.Description,
                roleToBeUpdated.IsEnabled,
                roleToBeUpdated.UpdatedAt);

            var res = new ValidationResult();

            if (!roleExists)
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (!hasAccessToRole)
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            _accessValidatorMock.Setup(r => r.CanUpdateRole(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
                .ReturnsAsync(res);

            _adminGraphRepositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(roleToBeUpdated);

            if (updateRoleThrowsException)
                _adminGraphRepositoryMock
                    .Setup(r => r.UpdateNodeAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Role>()))
                    .ThrowsAsync(new Exception());
            else
            {
                if (concurrency)
                    _adminGraphRepositoryMock
                        .Setup(r => r.UpdateNodeAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Role>()))
                        .ReturnsAsync((Role?)null);
                else
                    _adminGraphRepositoryMock
                        .Setup(r => r.UpdateNodeAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<Role>()))
                        .ReturnsAsync(roleToBeUpdated);
            }

            return (roleToBeUpdated, cmd);
        }

        private void AssertNoCacheBeingInvalidated()
        {
            _bloomCacheManagerMock.Verify(
                p => p.ForgetByRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}