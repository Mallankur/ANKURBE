using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.OngDb.Core.Interfaces;
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
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class CreateRoleCommandHandlerTests : BaseTests
    {
        private static readonly Guid DefaultPolicyId = Guid.NewGuid();

        public CreateRoleCommandHandlerTests()
        {
            _handler = new CreateRoleCommandHandler(
                DefaultPolicyId,
                new NamedNodeMapper<CreateRoleCommand, Role>(),
                _accessValidatorMock.Object,
                _adminGraphRepositoryMock.Object,
                _mediatorMock.Object);
        }

        private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();

        private readonly CreateRoleCommandHandler _handler;

        private (Role roleToBeCreated, CreateRoleCommand command) SetupCreateRoleCommand(
            bool createRoleThrowsException = false,
            bool createLinkWithPolicyThrowsException = false,
            bool createLinkWithTenantThrowsException = false,
            bool addLabelsThrowsException = false,
            bool tenantExists = true,
            bool policyExists = true,
            bool featuresExist = true,
            bool hasAccessToTenant = true,
            bool hasAccessToFeatures = true,
            bool useDefaultPolicyId = false,
            Role role = null)
        {
            var roleToBeCreated = role ?? new Role("aaa");
            var tenantId = Guid.NewGuid();

            var cmd = new CreateRoleCommand(
                Common.BuildPrincipal(tenantId.ToString()),
                useDefaultPolicyId ? (Guid?)null : Guid.NewGuid(),
                tenantId,
                roleToBeCreated.Name,
                roleToBeCreated.Description,
                roleToBeCreated.IsEnabled,
                new[] { Guid.NewGuid() });

            var res = new ValidationResult();

            if (!policyExists)
                res.SetError(ErrorCodes.PolicyDoesNotExist);

            if (!tenantExists)
                res.SetError(ErrorCodes.TenantDoesNotExist);

            if (!featuresExist)
                res.SetError(ErrorCodes.FeaturesDoNotExist);

            if (!hasAccessToTenant)
                res.SetError(ErrorCodes.SubjectCannotAccessTenant);

            if (!hasAccessToFeatures)
                res.SetError(ErrorCodes.SubjectCannotAccessFeatures);
            
            _accessValidatorMock.Setup(r => r.CanCreateRole(
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(res);

            if (createRoleThrowsException)
                _adminGraphRepositoryMock.Setup(r => r.CreateNodeAsync(It.IsAny<Role>())).ThrowsAsync(new Exception());
            else
                _adminGraphRepositoryMock.Setup(r => r.CreateNodeAsync(It.IsAny<Role>())).ReturnsAsync(roleToBeCreated);

            if (createLinkWithPolicyThrowsException)
                _adminGraphRepositoryMock.Setup(r => r.CreateRelationshipAsync(
                    It.IsAny<Expression<Func<Policy, bool>>>(),
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<ILink>())).ThrowsAsync(new Exception());

            if (createLinkWithTenantThrowsException)
                _adminGraphRepositoryMock.Setup(r => r.CreateRelationshipAsync(
                    It.IsAny<Expression<Func<Tenant, bool>>>(),
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<ILink>())).ThrowsAsync(new Exception());

            if (addLabelsThrowsException)
                _adminGraphRepositoryMock.Setup(r => r.AddLabelAsync(
                    It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<string>())).ThrowsAsync(new Exception());

            return (roleToBeCreated, cmd);
        }

        [Fact]
        public async Task Handle_Creates_Item()
        {
            var (roleToBeCreated, cmd) = SetupCreateRoleCommand();

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateItem(res, roleToBeCreated);
        }

        [Fact]
        public async Task Handle_Creates_Link_With_Policy()
        {
            var (roleToBeCreated, cmd) = SetupCreateRoleCommand();

            await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateLink(new Policy { Id = cmd.PolicyId.Value }, roleToBeCreated,
                Constants.ContainsLink);
        }

        [Fact]
        public async Task Handle_Creates_Link_With_DefaultPolicyId()
        {
            var (roleToBeCreated, cmd) = SetupCreateRoleCommand(useDefaultPolicyId: true);

            await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateLink(new Policy { Id = DefaultPolicyId }, roleToBeCreated,
                Constants.ContainsLink);
        }

        [Fact]
        public async Task Handle_Creates_Link_With_Tenant_And_Label_If_TenantId_Is_Present()
        {
            var (roleToBeCreated, cmd) = SetupCreateRoleCommand();

            await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateLink(new Tenant { Id = cmd.TenantId }, roleToBeCreated,
                Constants.OwnsLink);

            _adminGraphRepositoryMock.Verify(r => r.AddLabelAsync(
                It.Is<Expression<Func<Role, bool>>>(e => e.Compile()(roleToBeCreated)),
                It.Is<string>(s => s == Constants.Label.CUSTOM_ROLE)), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Create_Link_With_Policy_If_Creation_Of_Role_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Policy, bool>>>(),
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
        }

        [Fact]
        public async Task
            Handle_Does_Not_Create_Link_With_Tenant_And_Label_If_TenantId_Is_Present_But_Creation_Of_Link_With_Policy_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(createLinkWithPolicyThrowsException: true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<ILink>()), Times.Never);

            _adminGraphRepositoryMock.Verify(r => r.AddLabelAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task
            Handle_Does_Not_Create_Link_With_Tenant_And_Label_If_TenantId_Is_Present_But_Creation_Of_Role_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<Tenant, bool>>>(),
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<ILink>()), Times.Never);

            _adminGraphRepositoryMock.Verify(r => r.AddLabelAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_If_Creation_Of_Label_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(addLabelsThrowsException: true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_If_Creation_Of_Link_With_Policy_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(createLinkWithPolicyThrowsException: true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_If_Creation_Of_Link_With_Tenant_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(createLinkWithTenantThrowsException: true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_If_Creation_Of_Role_Failed()
        {
            var (_, cmd) = SetupCreateRoleCommand(true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Send_Audit_Event()
        {
            var (roleToBeCreated, cmd) = SetupCreateRoleCommand();

            await _handler.Handle(cmd, CancellationToken.None);

            _mediatorMock.AssertAuditEventWasPublished<Role>(roleToBeCreated.Id, _claimsPrincipal,
                AuditOperation.Create);
        }

        [Fact]
        public async Task Handle_Throws_Forbidden_Exception_If_Principal_Has_No_Access_To_Tenant()
        {
            var (_, cmd) = SetupCreateRoleCommand(hasAccessToTenant: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Forbidden_Exception_If_Principal_Has_No_Access_To_Features()
        {
            var (_, cmd) = SetupCreateRoleCommand(hasAccessToFeatures: false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Not_Found_Exception_If_Tenant_Does_Not_Exist()
        {
            var (_, cmd) = SetupCreateRoleCommand(tenantExists: false);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Not_Found_Exception_If_Policy_Does_Not_Exist()
        {
            var (_, cmd) = SetupCreateRoleCommand(policyExists: false);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Not_Found_Exception_If_Features_Do_Not_Exist()
        {
            var (_, cmd) = SetupCreateRoleCommand(featuresExist: false);

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(cmd, CancellationToken.None));
        }

        [Theory]
        [InlineData("")]
        [InlineData("<script")]
        public async Task Handle_Throws_Bad_Request_Exception_For_Invalid_Name(string name)
        {
            var (_, cmd) = SetupCreateRoleCommand(role: new Role
            {
                Name = name,
            });

            var exception =
                await Assert.ThrowsAsync<BadRequestException>(async () =>
                    await _handler.Handle(cmd, CancellationToken.None));
            Assert.True(exception.Params.ContainsKey("name"));
        }

        [Theory]
        [InlineData("<script")]
        public async Task Handle_Throws_Bad_Request_Exception_For_Invalid_Description(string description)
        {
            var (_, cmd) = SetupCreateRoleCommand(role: new Role
            {
                Name = Guid.NewGuid().ToString(),
                Description = description
            });

            var exception =
                await Assert.ThrowsAsync<BadRequestException>(async () =>
                    await _handler.Handle(cmd, CancellationToken.None));
            Assert.True(exception.Params.ContainsKey("description"));
        }
    }
}