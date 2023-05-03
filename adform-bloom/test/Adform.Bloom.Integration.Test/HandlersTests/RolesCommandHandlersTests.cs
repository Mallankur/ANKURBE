using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using MediatR;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Bloom.Write.Mappers;
using Adform.Bloom.Write.Services;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class RolesCommandHandlersTests : IClassFixture<TestsFixture>
    {
        private readonly Guid _defaultPolicyId = Guid.Parse(Graph.Policy2);
        private readonly UpdateRoleToFeatureAssignmentsCommandHandler _updateToFeatureAssignmentHandler;
        private readonly UpdateSubjectAssignmentsCommandHandler _assignSubjectToRoleHandler;
        private readonly AssignPermissionToRoleCommandHandler _assignPermissionToRoleHandler;
        private readonly CreateRoleCommandHandler _createHandler;
        private readonly UpdateRoleCommandHandler _updateHandler;
        private readonly DeleteRoleCommandHandler _deleteHandler;
        private readonly TestsFixture _fixture;

        public RolesCommandHandlersTests(TestsFixture fixture)
        {
            _fixture = fixture;
            var mediator = new Mock<IMediator>().Object;
            var client = new Mock<IBloomRuntimeClient>().Object;
            var producer = new Mock<INotificationService>().Object;
            var logger = new Mock<ILogger<UpdateSubjectAssignmentsCommandHandler>>().Object;

            var cacheManager = new Mock<IBloomCacheManager>().Object;
            var bus = new Mock<IBus>();
            var pubSub = new Mock<IPubSub>();
            bus.SetupGet(b => b.PubSub).Returns(pubSub.Object);

            var adapter = new ValidatorAdapter(
                _fixture.GraphRepository,
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsRoles, Contracts.Output.Role>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Subject>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Permission>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Feature>(),
                Options.Create(new ValidationConfiguration { RoleLimitPerSubject = int.MaxValue })
            );

            var validator = new AccessValidator(
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter);

            _assignSubjectToRoleHandler = new UpdateSubjectAssignmentsCommandHandler(
                _fixture.GraphRepository,
                mediator,
                validator,
                client,
                logger);

            _assignPermissionToRoleHandler = new AssignPermissionToRoleCommandHandler(
                _fixture.GraphRepository,
                validator,
                mediator);

#warning MK 2020-04-09: real validator should be used and not mock but now tests fail in this case

            _deleteHandler = new DeleteRoleCommandHandler(
                validator,
                fixture.GraphRepository,
                mediator);

            _createHandler = new CreateRoleCommandHandler(
                _defaultPolicyId,
                new NamedNodeMapper<CreateRoleCommand, Role>(),
                validator,
                fixture.GraphRepository,
                mediator);

            _updateHandler = new UpdateRoleCommandHandler(
                fixture.GraphRepository,
                mediator,
                validator,
                new NamedNodeMapper<UpdateRoleCommand, Role>(),
                cacheManager
            );

            _updateToFeatureAssignmentHandler = new UpdateRoleToFeatureAssignmentsCommandHandler(
                fixture.GraphRepository,
                mediator,
                validator,
                cacheManager
            );
        }

        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_SubjectId_For_Role_Assignment_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Create_Role__Creates_Role__Without_Group_Assignments(Guid? policyId, Guid tenantId,
            Guid subjectId)
        {
            // Arrange
            const string roleName = "RoleX";

            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);
            var roleId = created.Id;

            // Assert
            var role = await _fixture.GraphRepository.GetNodeAsync<Role>(r => r.Id == roleId);
            Assert.StartsWith(roleName, role.Name);

            var tenants =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Tenant>(r => r.Id == roleId,
                    Constants.OwnsIncomingLink);
            Assert.Equal(tenantId, tenants.First().Id);

            var groups =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Group>(r => r.Id == roleId,
                    Constants.AssignedIncomingLink);
            Assert.Empty(groups);

            var policies =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Policy>(r => r.Id == roleId,
                    Constants.ContainsIncomingLink);
            Assert.Equal(policyId ?? _defaultPolicyId, policies.Single().Id);
        }

        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_SubjectId_For_Role_Assignment_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Create_Role_Assign_User__Creates_Role_Creates_Group_Assigns_User(Guid? policyId,
            Guid tenantId,
            Guid subjectId)
        {
            // Arrange
            const string roleName = "RoleX";

            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);
            var roleId = created.Id;

            await _assignSubjectToRoleHandler.Handle(
                new UpdateSubjectAssignmentsCommand(_fixture.BloomApiPrincipal[Graph.Subject0], subjectId, new[]
                {
                    new RoleTenant
                    {
                        RoleId = roleId,
                        TenantId = tenantId
                    }
                }), CancellationToken.None);

            // Assert
            var role = await _fixture.GraphRepository.GetNodeAsync<Role>(r => r.Id == roleId);
            Assert.StartsWith(roleName, role.Name);

            var groups =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Group>(r => r.Id == roleId,
                    Constants.AssignedIncomingLink);
            Assert.Single(groups);

            var group = groups.First();
            Assert.StartsWith($"{tenantId}_{roleId}", group.Name);

            var groupExists = (await _fixture.GraphRepository.GetNodesAsync<Group>(g => g.Id == group.Id)).Any();
            Assert.True(groupExists);

            var assignedTenant =
                await _fixture.GraphRepository.GetConnectedAsync<Group, Tenant>(g => g.Id == group.Id,
                    Constants.BelongsLink);
            Assert.Single(assignedTenant);
            Assert.Equal(tenantId, assignedTenant.First().Id);
        }

        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_SubjectId_For_Role_Assignment_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Create_Role__Assign_User__Delete_Role__Scenario(Guid? policyId, Guid tenantId, Guid subjectId)
        {
            // Arrange
            const string roleName = "RoleX";

            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);
            var roleId = created.Id;

            await _assignSubjectToRoleHandler.Handle(
                new UpdateSubjectAssignmentsCommand(_fixture.BloomApiPrincipal[Graph.Subject0], subjectId, new[]
                {
                    new RoleTenant
                    {
                        RoleId = roleId,
                        TenantId = tenantId
                    }
                }), CancellationToken.None);

            var groupToBeDeleted =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Group>(r => r.Id == roleId,
                    Constants.AssignedIncomingLink);
            Assert.Single(groupToBeDeleted);

            var groupId = groupToBeDeleted.First().Id;
            var deletedGroup =
                await _fixture.GraphRepository.GetNodeAsync<Group>(g => g.Id == groupId);
            Assert.NotNull(deletedGroup);

            var tenants =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Tenant>(r => r.Id == roleId,
                    Constants.OwnsIncomingLink);
            Assert.Equal(tenantId, tenants.First().Id);

            var assignedTenant =
                await _fixture.GraphRepository.GetConnectedAsync<Group, Tenant>(g => g.Id == groupId,
                    Constants.BelongsLink);
            Assert.Single(assignedTenant);
            Assert.Equal(tenantId, assignedTenant.First().Id);

            await _deleteHandler.Handle(new DeleteRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], roleId),
                CancellationToken.None);

            // Assert
            var deletedRole = await _fixture.GraphRepository.GetNodeAsync<Role>(p => p.Id == roleId);
            Assert.Null(deletedRole);

            groupToBeDeleted =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Group>(r => r.Id == roleId,
                    Constants.AssignedIncomingLink);
            Assert.Empty(groupToBeDeleted);

            deletedGroup =
                await _fixture.GraphRepository.GetNodeAsync<Group>(g => g.Id == groupId);
            Assert.Null(deletedGroup);

            tenants =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Tenant>(r => r.Id == roleId,
                    Constants.OwnsIncomingLink);
            Assert.Empty(tenants);
        }

        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_SubjectId_SubjectId2_For_Role_Assignment_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Assign_User__Creates_Single_Group__For_Multiple_Assignments(Guid? policyId, Guid tenantId,
            Guid subjectId, Guid subjectId2)
        {
            // Arrange
            const string roleName = "RoleX";
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);
            var roleId = created.Id;

            // Act
            await _assignSubjectToRoleHandler.Handle(
                new UpdateSubjectAssignmentsCommand(_fixture.BloomApiPrincipal[Graph.Subject0], subjectId, new[]
                {
                    new RoleTenant
                    {
                        RoleId = roleId,
                        TenantId = tenantId
                    }
                }), CancellationToken.None);
            await _assignSubjectToRoleHandler.Handle(
                new UpdateSubjectAssignmentsCommand(_fixture.BloomApiPrincipal[Graph.Subject0], subjectId2, new[]
                {
                    new RoleTenant
                    {
                        RoleId = roleId,
                        TenantId = tenantId
                    }
                }), CancellationToken.None);

            // Assert
            var roleToGroupToTenantAssignments =
                await _fixture.GraphRepository.GetConnectedWithIntermediateAsync<Role, Group, Tenant>(
                    r => r.Id == roleId, Constants.AssignedIncomingLink, Constants.BelongsLink);
            Assert.Single(roleToGroupToTenantAssignments);
        }

        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_Error_For_Role_Update_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Update_Role__Updates_Role__Scenario(
            Guid policyId,
            Guid updateSubjectId,
            Guid tenantId,
            ErrorCodes? error,
            bool concurrency)
        {
            // Arrange
            var roleName = $"RoleX_{Guid.NewGuid()}";
            var result = new ValidationResult();
            if (error != null)
            {
                result.SetError((ErrorCodes) error);
            }

            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);
            var principal = _fixture.BloomApiPrincipal[updateSubjectId.ToString()];
            var updatedTask = _updateHandler.Handle(new UpdateRoleCommand(
                    principal, created.Id, created.Name, roleName,
                    created.IsEnabled,
                    !concurrency ? created.UpdatedAt :  created.UpdatedAt - DateTimeOffset.UtcNow.AddSeconds(5).ToUnixTimeSeconds()
                ),
                CancellationToken.None);

            // Assert
            if (error == ErrorCodes.SubjectCannotAccessRole)
            {
                await Assert.ThrowsAsync<ForbiddenException>(async () => await updatedTask);
            }

            if (concurrency)
            {
                await Assert.ThrowsAsync<ConflictException>(async () =>
                {
                    await updatedTask;
                });
            }

            if (result.Error == 0 && !concurrency)
            {
                var updated = await updatedTask;
                Assert.Equal(created.Id, updated.Id);
                Assert.Equal(created.Name, updated.Name);
                Assert.Equal(created.Name, updated.Description);
                Assert.Equal(created.CreatedAt, updated.CreatedAt);
            }
        }


        [Theory]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_Error_For_RoleFeature_Update_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Update_Role__Assign_Feature__Scenario(string caseName, Guid subjectId,
            Guid? policyId, Guid tenantId, ErrorCodes? error,
            IReadOnlyCollection<Guid> assignFeatureIds = null,
            IReadOnlyCollection<Guid> unassignFeatureIds = null)
        {
            // Arrange
            var roleName = $"TEMPORAL_ROLE{Guid.NewGuid()}";
            var result = new ValidationResult();
            if (error != null)
            {
                result.SetError((ErrorCodes) error);
            }

            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[subjectId.ToString()], policyId, tenantId,
                    roleName), CancellationToken.None);

            var updatedTask = _updateToFeatureAssignmentHandler.Handle(new UpdateRoleToFeatureAssignmentsCommand(
                _fixture.BloomApiPrincipal[subjectId.ToString()],
                created.Id, assignFeatureIds,
                unassignFeatureIds), CancellationToken.None);

            // Assert
            if (result.HasError(ErrorCodes.SubjectCannotAccessFeatures))
            {
                var exceptionResult = await Record.ExceptionAsync(() => updatedTask);
                Assert.Equal(exceptionResult.GetType(), typeof(ForbiddenException));
            }

            if (result.Error == 0)
            {
                var assigned = (assignFeatureIds ?? new List<Guid>()).Except(unassignFeatureIds ?? new List<Guid>())
                    .FirstOrDefault();

                if (assigned == Guid.Empty) return;

                var permissionsFeature = await _fixture.GraphRepository.GetConnectedAsync<Feature, Permission>(
                    r => r.Id == assigned,
                    Constants.ContainsLink);

                var permissionsRole = await _fixture.GraphRepository.GetConnectedAsync<Role, Permission>(
                    r => r.Id == created.Id,
                    Constants.ContainsLink);

                var permissionsFeatureIds = permissionsFeature.Select(o => o.Id);
                var permissionsRoleIds = permissionsRole.Select(o => o.Id);
                Assert.True(permissionsRoleIds.SequenceEqual(permissionsFeatureIds));
            }
        }


        [Theory, Order(100)]
        [MemberData(nameof(Scenarios.PolicyId_TenantId_SubjectId_Access_For_Role_Delete_Scenarios),
            MemberType = typeof(Scenarios))]
        public async Task Delete_Role__Scenario(
            Guid? policyId,
            Guid tenantId,
            Guid subjectId,
            bool hasAccess)
        {
            // Arrange
            var roleName = $"RoleDel_{Guid.NewGuid()}";
            // Act
            var created = await _createHandler.Handle(
                new CreateRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], policyId, tenantId,
                    roleName), CancellationToken.None);

            var roleId = created.Id;

            await _assignSubjectToRoleHandler.Handle(
                new UpdateSubjectAssignmentsCommand(_fixture.BloomApiPrincipal[Graph.Subject0], subjectId, new[]
                {
                    new RoleTenant
                    {
                        RoleId = roleId,
                        TenantId = tenantId
                    }
                }), CancellationToken.None);

            var groupToBeDeleted =
                await _fixture.GraphRepository.GetConnectedAsync<Role, Group>(r => r.Id == roleId,
                    Constants.AssignedIncomingLink);
            Assert.Single(groupToBeDeleted);


            var deleteTask = _deleteHandler.Handle(
                new DeleteRoleCommand(_fixture.BloomApiPrincipal[subjectId.ToString()], roleId),
                CancellationToken.None);
            // Assert
            if (!hasAccess)
            {
                await Assert.ThrowsAsync<ForbiddenException>(async () => await deleteTask);
            }
            else
            {
                await deleteTask;
                var group = groupToBeDeleted.First();
                var deletedGroup =
                    await _fixture.GraphRepository.GetNodeAsync<Group>(g => g.Id == group.Id);
                Assert.Null(deletedGroup);
                var deletedRole =
                    await _fixture.GraphRepository.GetNodeAsync<Role>(g => g.Id == created.Id);
                Assert.Null(deletedRole);
            }
        }

        [Fact]
        public async Task Delete_Non_Existing_Role_Throws_NotFound()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            // Act
            var deleteTask = _deleteHandler.Handle(
                new DeleteRoleCommand(_fixture.BloomApiPrincipal[Graph.Subject0], roleId),
                CancellationToken.None);
            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () => await deleteTask);
        }
    }
}