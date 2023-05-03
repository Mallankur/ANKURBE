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
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Bloom.Write.Services;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ErrorCodes = Adform.Bloom.Domain.ValueObjects.ErrorCodes;

namespace Adform.Bloom.Unit.Test.Write;

public class UpdateSubjectAssignmentsCommandTests : BaseTests
{
    private readonly Mock<INotificationService> _notificationService = new Mock<INotificationService>();
    private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();
    private readonly Mock<IBloomRuntimeClient> _client = new Mock<IBloomRuntimeClient>();

    private readonly Mock<ILogger<UpdateSubjectAssignmentsCommandHandler>> _logger =
        new Mock<ILogger<UpdateSubjectAssignmentsCommandHandler>>();

    private readonly Mock<IBloomCacheManager> _cacheManager = new Mock<IBloomCacheManager>();
    private readonly UpdateSubjectAssignmentsCommandHandler _handler;

    public UpdateSubjectAssignmentsCommandTests()
    {
        _client.Setup(o => o.InvokeAsync(It.IsAny<SubjectRuntimeRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RuntimeResponse>
            {
                new RuntimeResponse
                {
                    TenantId = Guid.Empty,
                    TenantName = "tenant1",
                    Roles = new List<string> {"role1"},
                    Permissions = new List<string> {"permission1"}
                }
            });

        _handler = new UpdateSubjectAssignmentsCommandHandler(_adminGraphRepositoryMock.Object, _mediatorMock.Object,
            _accessValidatorMock.Object, _client.Object, _logger.Object);
    }

    [Theory]
    [InlineData(ErrorCodes.TenantDoesNotExist)]
    [InlineData(ErrorCodes.RoleDoesNotExist)]
    [InlineData(ErrorCodes.SubjectDoesNotExist)]
    public async Task Handle_Assign_Throws_NotFoundException_When_Node_DoesntExist(ErrorCodes errorCodes)
    {
        var tenantId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = Guid.NewGuid(),
                TenantId = tenantId
            }
        };
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(Array.Empty<Group>());
        SetupAccessValidator(errorCodes);
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(
                new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(), Guid.NewGuid(), assignment),
                CancellationToken.None));

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Theory]
    [InlineData(ErrorCodes.TenantDoesNotExist)]
    [InlineData(ErrorCodes.RoleDoesNotExist)]
    [InlineData(ErrorCodes.SubjectDoesNotExist)]
    public async Task Handle_Unassign_Throws_NotFoundException_When_Node_DoesntExist(ErrorCodes errorCodes)
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var unassignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }
        };
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(new[] { new Group { Id = Guid.NewGuid() } });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Tenant>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.BelongsLink))
            .ReturnsAsync(new[] { new Tenant { Id = tenantId } });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Role>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.AssignedLink))
            .ReturnsAsync(new[] { new Role { Id = roleId } });
        SetupAccessValidator(errorCodes, true);
        await Assert.ThrowsAsync<NotFoundException>(async () =>
            await _handler.Handle(
                new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(), Guid.NewGuid(), null,
                    unassignment),
                CancellationToken.None));

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Theory]
    [InlineData(ErrorCodes.SubjectCannotAccessTenant)]
    [InlineData(ErrorCodes.SubjectCannotAccessSubject)]
    [InlineData(ErrorCodes.SubjectCannotAccessRole)]
    public async Task Handle_Unassign_Throws_ForbiddenException_When_Doesnt_Have_Access(ErrorCodes errorCodes)
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }
        };
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(new[] {new Group{Id = Guid.NewGuid()}});
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Tenant>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.BelongsLink))
            .ReturnsAsync(new [] {new Tenant{Id = tenantId}});
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Role>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.AssignedLink))
            .ReturnsAsync(new []{new Role{Id = roleId}});

        SetupAccessValidator(errorCodes, true);
        await Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _handler.Handle(
                new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(), Guid.NewGuid(), null,
                    assignment),
                CancellationToken.None));

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Theory]
    [InlineData(ErrorCodes.SubjectCannotAccessTenant)]
    [InlineData(ErrorCodes.SubjectCannotAccessRole)]
    public async Task Handle_Assign_Throws_ForbiddenException_When_Doesnt_Have_Access(ErrorCodes errorCodes)
    {
        var tenantId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = Guid.NewGuid(),
                TenantId = tenantId
            }
        };
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(Array.Empty<Group>());
        SetupAccessValidator(errorCodes);
        await Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _handler.Handle(
                new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(), Guid.NewGuid(),
                    assignment, null),
                CancellationToken.None));

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Assign_Throws_ForbiddenException_When_Exceeds_AssignmentLimit()
    {
        var tenantId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = Guid.NewGuid(),
                TenantId = tenantId
            }
        };
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(Array.Empty<Group>());
        SetupAccessValidator(ErrorCodes.SubjectCannotExceedRoleAssignmentLimit);
        await Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _handler.Handle(
                new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(), Guid.NewGuid(),
                    assignment, null),
                CancellationToken.None));

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Removes_Assignment_For_Unassign()
    {
        var tenantId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var groupId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = roleId,
                TenantId = tenantId
            }
        };

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Tenant>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Tenant>()
            {
                new Tenant(tenantId.ToString())
                {
                    Id = tenantId
                }
            });

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Role>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Role>()
            {
                new Role(roleId.ToString())
                {
                    Id = roleId
                }
            });

        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(new[] {new Group{Id = groupId}});
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Tenant>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.BelongsLink))
            .ReturnsAsync(new[] { new Tenant { Id = tenantId } });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Group, Role>(It.IsAny<Expression<Func<Group, bool>>>(),
                    Constants.AssignedLink))
            .ReturnsAsync(new[] { new Role { Id = roleId } });

        SetupAccessValidator(setupUnassign: true);

        await _handler.Handle(
            new UpdateSubjectAssignmentsCommand(Common.BuildPrincipal(tenantId.ToString()), subjectId, null,
                assignment),
            CancellationToken.None);

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
                It.Is<Guid>(o => o == subjectId), It.Is<IEnumerable<RoleTenant>>(o => o.SequenceEqual(assignment))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Adds_Assignment_For_Assign()
    {
        var tenantId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var tenantLegacyId = 123;
        var subjectId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = Guid.NewGuid(),
                TenantId = tenantId
            }
        };

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Tenant>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Tenant>()
            {
                new Tenant(tenantId.ToString())
                {
                    Id = tenantId
                }
            });

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Role>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Role>()
            {
                new Role(roleId.ToString())
                {
                    Id = roleId
                }
            });


        var principal = Common.BuildPrincipal(tenantId.ToString());
        var correlationId = Guid.NewGuid();
        var @params = new Dictionary<string, object> { ["email"] = "abc" };

        _adminGraphRepositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<Tenant, bool>>>()))
            .ReturnsAsync(new Tenant { LegacyId = tenantLegacyId });
        _adminGraphRepositoryMock.Setup(r => r.GetLabelsAsync(It.IsAny<Expression<Func<Tenant, bool>>>()))
            .ReturnsAsync(new[] { "Tenant", "Agency" });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(Array.Empty<Group>());

        SetupAccessValidator();

        await _handler.Handle(
            new UpdateSubjectAssignmentsCommand(principal, subjectId,
                assignment, new RoleTenant[0]),
            CancellationToken.None);

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
                It.Is<Guid>(o => o == subjectId), It.Is<IEnumerable<RoleTenant>>(o => o.SequenceEqual(assignment))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Adds_ManyAssignment_For_Assign()
    {
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();
        var tenantLegacyId1 = 123;
        var tenantLegacyId2 = 321;
        var subjectId = Guid.NewGuid();
        var assignment = new List<RoleTenant>
        {
            new RoleTenant
            {
                RoleId = roleId1,
                TenantId = tenantId1
            },
            new RoleTenant
            {
                RoleId = roleId2,
                TenantId = tenantId2
            }
        };

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Tenant>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Tenant>()
            {
                new Tenant(tenantId1.ToString())
                {
                    Id = tenantId1
                },
                new Tenant(tenantId2.ToString())
                {
                    Id = tenantId2
                }
            });

        _adminGraphRepositoryMock.Setup(
                o => o.GetByIdsAsync<Role>(It.IsAny<IReadOnlyCollection<Guid>>()))
            .ReturnsAsync(new List<Role>()
            {
                new Role(roleId1.ToString())
                {
                    Id = roleId1
                },
                new Role(roleId2.ToString())
                {
                    Id = roleId2
                }
            });


        var principal = Common.BuildPrincipal(new[] { tenantId1, tenantId2 });

        _adminGraphRepositoryMock.Setup(r =>
                r.GetNodeAsync(
                    It.Is<Expression<Func<Tenant, bool>>>(ex => ex.Compile()(new Tenant { Id = tenantId1 }))))
            .ReturnsAsync(new Tenant { LegacyId = tenantLegacyId1 });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetNodeAsync(
                    It.Is<Expression<Func<Tenant, bool>>>(ex => ex.Compile()(new Tenant { Id = tenantId2 }))))
            .ReturnsAsync(new Tenant { LegacyId = tenantLegacyId2 });

        _adminGraphRepositoryMock.Setup(r =>
                r.GetLabelsAsync(
                    It.Is<Expression<Func<Tenant, bool>>>(ex => ex.Compile()(new Tenant { Id = tenantId1 }))))
            .ReturnsAsync(new[] { "Tenant", "Agency" });
        _adminGraphRepositoryMock.Setup(r =>
                r.GetLabelsAsync(
                    It.Is<Expression<Func<Tenant, bool>>>(ex => ex.Compile()(new Tenant { Id = tenantId2 }))))
            .ReturnsAsync(new[] { "Tenant", "DataProvider" });

        _adminGraphRepositoryMock.Setup(r =>
                r.GetConnectedAsync<Subject, Group>(It.IsAny<Expression<Func<Subject, bool>>>(),
                    Constants.MemberOfLink))
            .ReturnsAsync(Array.Empty<Group>());

        SetupAccessValidator();

        await _handler.Handle(
            new UpdateSubjectAssignmentsCommand(principal, subjectId,
                assignment, new RoleTenant[0]),
            CancellationToken.None);

        _adminGraphRepositoryMock.Verify(r => r.BulkLazyCreateGroupAsync(
                It.Is<Guid>(o => o == subjectId), It.Is<IEnumerable<RoleTenant>>(o => o.SequenceEqual(assignment))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(r => r.BulkUnassignSubjectFromRolesAsync(
            It.IsAny<Guid>(), It.IsAny<IEnumerable<RoleTenant>>()), Times.Never);
    }

    private void SetupAccessValidator(ErrorCodes codes = 0, bool setupUnassign = false)
    {
        var res = new ValidationResult();
        res.SetError(codes);

        if (setupUnassign)
        {
            _accessValidatorMock
                .Setup(x => x.CanUnassignSubjectFromRolesAsync(It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<Guid>())).ReturnsAsync(res);
        }
        else
        {
            _accessValidatorMock
                .Setup(x => x.CanAssignSubjectToRolesAsync(It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<IEnumerable<RoleTenant>>(), It.IsAny<Guid>())).ReturnsAsync(res);
        }
    }
}