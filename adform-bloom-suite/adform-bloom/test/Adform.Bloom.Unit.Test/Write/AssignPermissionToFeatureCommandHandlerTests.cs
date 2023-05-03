using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class AssignPermissionToFeatureCommandHandlerTests : BaseTests
    {
        public AssignPermissionToFeatureCommandHandlerTests()
        {
            _handler = new AssignPermissionToFeatureCommandHandler(_adminGraphRepositoryMock.Object,
                _featureAccessRepositoryMock.Object,
                _mediatorMock.Object);
            _adminGraphRepositoryMock
                .Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Permission, bool>>>(), null, null))
                .Returns(Task.FromResult((long) 1));
            _adminGraphRepositoryMock
                .Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Feature, bool>>>(), null, null))
                .Returns(Task.FromResult((long) 1));
            _featureAccessRepositoryMock
                .Setup(x => x.HasItemVisibilityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), null))
                .ReturnsAsync(true);
            _adminGraphRepositoryMock
                .Setup(x => x.GetConnectedAsync<Permission, Feature>(It.IsAny<Expression<Func<Permission, bool>>>(),
                    Constants.ContainsIncomingLink)).ReturnsAsync(new List<Feature>());
        }

        private readonly AssignPermissionToFeatureCommandHandler _handler;

        [Theory]
        [InlineData(LinkOperation.Assign)]
        [InlineData(LinkOperation.Unassign)]
        public async Task Handle_Sends_Audit_Event(LinkOperation operation)
        {
            var request = new AssignPermissionToFeatureCommand(_claimsPrincipal, Guid.NewGuid(), Guid.NewGuid(),
                operation);
            await _handler.Handle(request, CancellationToken.None);
            _mediatorMock.Verify(m => m.Publish(It.IsAny<AuditChange>(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Send_Audit_Event_In_Case_Of_Error()
        {
            _adminGraphRepositoryMock
                .Setup(m => m.GetCountAsync(It.IsAny<Expression<Func<Permission, bool>>>(), null, null))
                .ThrowsAsync(new Exception());
            var request = new AssignPermissionToFeatureCommand(_claimsPrincipal, Guid.NewGuid(), Guid.NewGuid(),
                LinkOperation.Assign);
            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(request, CancellationToken.None));
            _mediatorMock.Verify(m => m.Publish(It.IsAny<AuditChange>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task Tenant_And_Feature_Presence_Is_Validated()
        {
            var request = new AssignPermissionToFeatureCommand(_claimsPrincipal, Guid.NewGuid(), Guid.NewGuid(),
                LinkOperation.Assign);
            await _handler.Handle(request, CancellationToken.None);
            _adminGraphRepositoryMock.Verify(
                m => m.GetCountAsync(It.IsAny<Expression<Func<Permission, bool>>>(), null, null), Times.Once);
            _adminGraphRepositoryMock.Verify(
                m => m.GetCountAsync(It.IsAny<Expression<Func<Feature, bool>>>(), null, null),
                Times.Once);
        }

        [Fact]
        public async Task When_Assigned_Link_Is_Created()
        {
            var request = new AssignPermissionToFeatureCommand(_claimsPrincipal, Guid.NewGuid(), Guid.NewGuid(),
                LinkOperation.Assign);
            await _handler.Handle(request, CancellationToken.None);
            _adminGraphRepositoryMock.Verify(m => m.AssignPermissionsToRolesThroughFeatureAssignmentAsync(It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<Guid>>()));
            _adminGraphRepositoryMock.Verify(m => m.CreateRelationshipAsync(It.IsAny<Expression<Func<Feature, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                Constants.ContainsLink));
        }

        [Fact]
        public async Task When_Unassigned_Feature_With_Permissions_Are_Deleted()
        {
            var permissionId = Guid.NewGuid();
            var request = new AssignPermissionToFeatureCommand(_claimsPrincipal, Guid.NewGuid(), permissionId,
                LinkOperation.Unassign);
            await _handler.Handle(request, CancellationToken.None);
            _adminGraphRepositoryMock.Verify(m => m.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<Guid>>()));
            _adminGraphRepositoryMock.Verify(m =>
                m.DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(permissionId));
            _adminGraphRepositoryMock.Verify(m => m.DeleteRelationshipAsync(It.IsAny<Expression<Func<Feature, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                Constants.ContainsLink));
        }
    }
}