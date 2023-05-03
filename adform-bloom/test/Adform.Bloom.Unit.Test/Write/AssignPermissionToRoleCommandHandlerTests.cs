using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class AssignPermissionToRoleCommandHandlerTests : BaseTests
    {
        public AssignPermissionToRoleCommandHandlerTests()
        {
            _handler = new AssignPermissionToRoleCommandHandler(
                _adminGraphRepositoryMock.Object,
                _accessValidator.Object,
                _mediatorMock.Object);
        }

        private readonly AssignPermissionToRoleCommandHandler _handler;
        private readonly Mock<IAccessValidator> _accessValidator = new Mock<IAccessValidator>();

        [Theory]
        [InlineData(LinkOperation.Assign)]
        [InlineData(LinkOperation.Unassign)]
        public async Task Handle_Send_Audit_Event(LinkOperation operation)
        {
            var (cmd, oldEntity, newEntity) = SetupAssignPermissionToRoleCommand(operation);

            await _handler.Handle(cmd, CancellationToken.None);

            _mediatorMock.AssertAuditChangeWasPublished(_claimsPrincipal, oldEntity, newEntity, AuditOperation.Update);
        }

        [Theory]
        [InlineData(LinkOperation.Assign, true, true, true, true, true)]
        [InlineData(LinkOperation.Assign, false, false, true, true, true)]
        [InlineData(LinkOperation.Assign, false, true, false, true, true)]
        [InlineData(LinkOperation.Assign, false, true, true, false, true)]
        [InlineData(LinkOperation.Assign, false, true, true, true, false)]
        [InlineData(LinkOperation.Unassign, true, true, true, true, true)]
        [InlineData(LinkOperation.Unassign, false, false, true, true, true)]
        [InlineData(LinkOperation.Unassign, false, true, false, true, true)]
        [InlineData(LinkOperation.Unassign, false, true, true, false, true)]
        [InlineData(LinkOperation.Unassign, false, true, true, true, false)]
        public async Task Handle_Does_Not_Send_Audit_Event_In_Case_Of_Error(
            LinkOperation operation,
            bool throwsException,
            bool roleExists,
            bool permissionExists,
            bool isPermissionInFeature,
            bool hasAccessToRole)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(
                operation, throwsException, roleExists, permissionExists, isPermissionInFeature, hasAccessToRole);

            await Assert.ThrowsAnyAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditChangeWasNotPublished();
        }

        [Theory]
        [InlineData(false, true, true, true)]
        [InlineData(true, false, true, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, true, true, false)]
        public async Task Handle_Does_Not_Create_Assignment_If_Validation_Fails(
            bool roleExists, bool permissionExists, bool isPermissionInFeature, bool hasAccessToRole)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(LinkOperation.Assign,
                roleExists: roleExists, permissionExists: permissionExists,
                isPermissionInFeature: isPermissionInFeature, hasAccessToRole: hasAccessToRole);

            await Assert.ThrowsAnyAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
            _adminGraphRepositoryMock.Verify(r => r.DeleteRelationshipAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
        }

        [Theory]
        [InlineData(false, true, true, true)]
        [InlineData(true, false, true, true)]
        [InlineData(true, true, false, true)]
        [InlineData(true, true, true, false)]
        public async Task Handle_Does_Not_Remove_Assignment_If_Validation_Fails(
            bool roleExists, bool permissionExists, bool isPermissionInFeature, bool hasAccessToRole)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(LinkOperation.Unassign,
                roleExists: roleExists, permissionExists: permissionExists,
                isPermissionInFeature: isPermissionInFeature, hasAccessToRole: hasAccessToRole);

            await Assert.ThrowsAnyAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
            _adminGraphRepositoryMock.Verify(r => r.DeleteRelationshipAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
        }

        [Theory]
        [InlineData(LinkOperation.Assign, false, true)]
        [InlineData(LinkOperation.Assign, true, false)]
        [InlineData(LinkOperation.Unassign, false, true)]
        [InlineData(LinkOperation.Unassign, true, false)]
        public async Task Handle_Throws_Not_Found_Exception_If_Role_Or_Permission_Does_Not_Exist(
            LinkOperation operation, bool roleExists, bool permissionExists)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(operation,
                roleExists: roleExists, permissionExists: permissionExists);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(cmd, CancellationToken.None));
        }

        [Theory]
        [InlineData(LinkOperation.Assign)]
        [InlineData(LinkOperation.Unassign)]
        public async Task Handle_Throws_Forbidden_Exception_If_Permission_Not_In_Future(LinkOperation operation)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(operation, isPermissionInFeature: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Theory]
        [InlineData(LinkOperation.Assign)]
        [InlineData(LinkOperation.Unassign)]
        public async Task Handle_Throws_Forbidden_Exception_If_Principal_Has_No_Access_To_Role(LinkOperation operation)
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(operation, hasAccessToRole: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        private (AssignPermissionToRoleCommand cmd, ConnectedNode oldEntity, ConnectedNode newEntity)
            SetupAssignPermissionToRoleCommand(
                LinkOperation linkOperation,
                bool throwsException = false,
                bool roleExists = true,
                bool permissionExists = true,
                bool isPermissionInFeature = true,
                bool hasAccessToRole = true)
        {
            var cmd = new AssignPermissionToRoleCommand(_claimsPrincipal, Guid.NewGuid(), Guid.NewGuid(),
                linkOperation);

            var res = new ValidationResult();

            if (!roleExists)
                res.SetError(ErrorCodes.RoleDoesNotExist);

            if (!permissionExists)
                res.SetError(ErrorCodes.PermissionDoesNotExist);

            if (!isPermissionInFeature)
                res.SetError(ErrorCodes.SubjectCannotAccessPermission);

            if (!hasAccessToRole)
                res.SetError(ErrorCodes.SubjectCannotAccessRole);

            if (!roleExists)
                res.SetError(ErrorCodes.RoleDoesNotExist);

            _accessValidator.Setup(r => r.CanAssignPermissionToRoleAsync(
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(res);

            ConnectedNode oldEntity;
            ConnectedNode newEntity;
            if (linkOperation == LinkOperation.Assign)
            {
                var m = _adminGraphRepositoryMock
                    .Setup(r => r.CreateRelationshipAsync(
                        It.IsAny<Expression<Func<Role, bool>>>(),
                        It.IsAny<Expression<Func<Permission, bool>>>(),
                        It.IsAny<ILink>()));

                if (throwsException)
                    m.ThrowsAsync(new Exception());

                oldEntity = new ConnectedNode(cmd.RoleId, cmd.PermissionId, null);
                newEntity = new ConnectedNode(cmd.RoleId, cmd.PermissionId, Constants.Relationship.CONTAINS);
            }
            else
            {
                var m = _adminGraphRepositoryMock
                    .Setup(r => r.DeleteRelationshipAsync(
                        It.IsAny<Expression<Func<Role, bool>>>(),
                        It.IsAny<Expression<Func<Permission, bool>>>(),
                        It.IsAny<ILink>()));

                if (throwsException)
                    m.ThrowsAsync(new Exception());

                oldEntity = new ConnectedNode(cmd.RoleId, cmd.PermissionId, Constants.Relationship.CONTAINS);
                newEntity = new ConnectedNode(cmd.RoleId, cmd.PermissionId, null);
            }

            return (cmd, oldEntity, newEntity);
        }

        [Fact]
        public async Task Handle_Creates_Assignment_For_Assign_Operation()
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(LinkOperation.Assign);

            await _handler.Handle(cmd, CancellationToken.None);

            var role = new Role {Id = cmd.RoleId};
            var permission = new Permission {Id = cmd.PermissionId};
            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.Is<Expression<Func<Role, bool>>>(e => e.Compile()(role)),
                It.Is<Expression<Func<Permission, bool>>>(e => e.Compile()(permission)),
                It.Is<ILink>(l => l == Constants.ContainsLink)), Times.Once);
            _adminGraphRepositoryMock.Verify(r => r.DeleteRelationshipAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<Expression<Func<Permission, bool>>>(),
                    It.IsAny<ILink>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Removes_Assignment_For_Assign_Operation()
        {
            var (cmd, _, _) = SetupAssignPermissionToRoleCommand(LinkOperation.Unassign);

            await _handler.Handle(cmd, CancellationToken.None);

            var role = new Role {Id = cmd.RoleId};
            var permission = new Permission {Id = cmd.PermissionId};
            _adminGraphRepositoryMock.Verify(r => r.DeleteRelationshipAsync(
                    It.Is<Expression<Func<Role, bool>>>(e => e.Compile()(role)),
                    It.Is<Expression<Func<Permission, bool>>>(e => e.Compile()(permission)),
                    It.Is<ILink>(l =>
                        l.Direction == RelationshipDirection.Outgoing && l.Type == "CONTAINS" && l.Variable == "r")),
                Times.Once);
            // It.Is<ILink>(l => l == Constants.ContainsVariableLink("r"))), Times.Once);
            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<Expression<Func<Permission, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
        }
    }
}