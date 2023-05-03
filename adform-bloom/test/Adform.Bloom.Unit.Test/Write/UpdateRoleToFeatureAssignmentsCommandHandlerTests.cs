using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Moq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Unit.Test.Write
{
    public class UpdateRoleToFeatureAssignmentsCommandHandlerTests : BaseTests
    {

        public UpdateRoleToFeatureAssignmentsCommandHandlerTests()
        {
            _handler = new UpdateRoleToFeatureAssignmentsCommandHandler(
                _adminGraphRepositoryMock.Object,
                _mediatorMock.Object,
                _accessValidatorMock.Object,
                _bloomCacheManagerMock.Object);
        }

        private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();
        private readonly Mock<IBloomCacheManager> _bloomCacheManagerMock = new Mock<IBloomCacheManager>();

        private readonly UpdateRoleToFeatureAssignmentsCommandHandler _handler;

        private readonly IReadOnlyCollection<Guid> _assignments = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        [Fact]
        public async Task Handle_Throws_Forbidden_Exception_If_Principal_Has_No_Access_To_Features()
        {
            var (_, cmd) = SetupUpdateRoleAssignmentsCommand(hasAccessToFeature: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
            AssertNoCacheBeingInvalidated();
        }

        [Fact]
        public async Task Handle_Updates_Item_And_Assigns_Permissions_When_FeatureAssignments_Are_set()
        {
            var (roleToBeUpdated, cmd) = SetupUpdateRoleAssignmentsCommand(hasFeatureAssignments: true);

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.Verify(x => x.AssignPermissionsToRoleThroughFeaturesAsync(roleToBeUpdated.Id,
                It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(_assignments))));

            _bloomCacheManagerMock.Verify(p => p.ForgetByRoleAsync(It.Is<string>(p => p == roleToBeUpdated.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact, Order(100)]
        public async Task Handle_Updates_Item_And_Unassigns_Permissions_When_FeatureUnassignments_Are_set()
        {
            var (roleToBeUpdated, cmd) = SetupUpdateRoleAssignmentsCommand(hasFeatureUnassignments: true);

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.Verify(x => x.UnassignPermissionsFromRoleThroughFeaturesAsync(roleToBeUpdated.Id,
                It.Is<IReadOnlyCollection<Guid>>(y => y.SequenceEqual(_assignments))));

            _bloomCacheManagerMock.Verify(p => p.ForgetByRoleAsync(It.Is<string>(p => p == roleToBeUpdated.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        private (Role roleToBeCreated, UpdateRoleToFeatureAssignmentsCommand command) SetupUpdateRoleAssignmentsCommand(
            bool roleExists = true,
            bool hasAccessToRole = true,
            bool hasAccessToFeature = true,
            bool hasFeatureAssignments = false,
            bool hasFeatureUnassignments = false,
            Role role = null)
        {
            var roleToBeUpdated = role ?? new Role("aaa");
            var tenantId = Guid.NewGuid();

            var assignments = hasFeatureAssignments ? _assignments : null;
            var unassignments = hasFeatureUnassignments ? _assignments : null;

            var cmd = new UpdateRoleToFeatureAssignmentsCommand(
                Common.BuildPrincipal(tenantId.ToString()),
                roleToBeUpdated.Id,
                assignments,
                unassignments);

            var res = new ValidationResult();

            if (!roleExists)
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (!hasAccessToRole)
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            if (!hasAccessToFeature)
                res.SetError(ErrorCodes.SubjectCannotAccessFeatures);

            _accessValidatorMock.Setup(r => r.CanUpdateRole(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
                .ReturnsAsync(res);

            _accessValidatorMock.Setup(x => x.CanAssignRoleToFeaturesAsync(It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<Guid>(),
                    It.IsAny<IReadOnlyCollection<Guid>>()))
                .ReturnsAsync(res);

            _adminGraphRepositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<Role, bool>>>()))
                .ReturnsAsync(roleToBeUpdated);

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