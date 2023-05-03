using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.ExceptionHandling.Abstractions.Extensions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write;

public class UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests : BaseTests
{
    private readonly Mock<IAccessValidator> _accessValidatorMock;
    private readonly UpdateLicensedFeatureToTenantAssignmentsCommandHandler _handler;

    public UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests()
    {
        _accessValidatorMock = new Mock<IAccessValidator>();

        _handler = new UpdateLicensedFeatureToTenantAssignmentsCommandHandler(
            _adminGraphRepositoryMock.Object,
            _accessValidatorMock.Object,
            _mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_Assign_Invokes_Repository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var assignments = new[] {Guid.NewGuid()};
        SetupAccessValidator();
        var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(
            Common.BuildPrincipal(),
            tenantId,
            assignments,
            null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _adminGraphRepositoryMock.Verify(r => r.AssignLicensedFeaturesToTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(
            r => r.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(assignments, tenantId, null), Times.Once);
        _adminGraphRepositoryMock.Verify(r => r.UnassignLicensedFeaturesFromTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Never);
        _adminGraphRepositoryMock.Verify(
            r => r.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(assignments, tenantId,null), Times.Never);
    }

    [Fact]
    public async Task Handle_Unassign_Invokes_Repository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var assignments = new[] {Guid.NewGuid()};
        SetupAccessValidator();
        var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(
            Common.BuildPrincipal(),
            tenantId,
            null,
            assignments);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _adminGraphRepositoryMock.Verify(r => r.UnassignLicensedFeaturesFromTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(
            r => r.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(assignments, tenantId,null), Times.Once);
        _adminGraphRepositoryMock.Verify(r => r.AssignLicensedFeaturesToTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Never);
        _adminGraphRepositoryMock.Verify(
            r => r.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(assignments, tenantId,null), Times.Never);
    }

    [Fact]
    public async Task Handle_Invokes_Repository()
    {
        var tenantId = Guid.NewGuid();
        var assignments = new[] {Guid.NewGuid()};
        SetupAccessValidator();
        var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(
            Common.BuildPrincipal(),
            tenantId,
            assignments, assignments
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);
        // Assert
        _adminGraphRepositoryMock.Verify(r => r.UnassignLicensedFeaturesFromTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(
            r => r.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(assignments, tenantId, null), Times.Once);
        _adminGraphRepositoryMock.Verify(r => r.AssignLicensedFeaturesToTenantAsync(
                It.Is<Guid>(p => p.Equals(tenantId)),
                It.Is<IReadOnlyCollection<Guid>>(p => p.SequenceEqual(assignments))),
            Times.Once);
        _adminGraphRepositoryMock.Verify(
            r => r.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(assignments, tenantId, null), Times.Once);
    }

    [Theory]
    [MemberData(nameof(Data),
        MemberType = typeof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests))]
    public async Task Handle_Assign_Throws_Exception_When_Doesnt_Have_Access(ErrorCodes errorCodes, Exception exception)
    {
        // Arrange
        SetupAccessValidator(errorCodes);
        var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(
            Common.BuildPrincipal(),
            Guid.NewGuid(),
            new[] {Guid.NewGuid()},
            null);

        // Act
        var exceptionResult = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(exceptionResult.GetReason(), exception.GetReason());
        Assert.Equal(exceptionResult.Message, exception.Message);
        _adminGraphRepositoryMock.Verify(r => r.AssignLicensedFeaturesToTenantAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(Data),
        MemberType = typeof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests))]
    public async Task Handle_Unassign_Throws_Exception_When_Doesnt_Have_Access(ErrorCodes errorCodes,
        Exception exception)
    {
        // Arrange
        SetupAccessValidator(errorCodes);
        var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(
            Common.BuildPrincipal(),
            Guid.NewGuid(),
            null,
            new[] {Guid.NewGuid()});

        // Act
        var exceptionResult = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal(exceptionResult.GetReason(), exception.GetReason());
        Assert.Equal(exceptionResult.Message, exception.Message);
        _adminGraphRepositoryMock.Verify(r => r.UnassignLicensedFeaturesFromTenantAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>()), Times.Never);
    }

    public static TheoryData<ErrorCodes, Exception> Data()
    {
        var data = new TheoryData<ErrorCodes, Exception>
        {
            {
                ErrorCodes.SubjectCannotAccessTenant,
                new ForbiddenException(ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessTenant)
            },
            {
                ErrorCodes.TenantDoesNotExist,
                new NotFoundException(message: $"{typeof(Tenant).Name} not found.")
            },
            {
                ErrorCodes.LicensedFeaturesDoNotExist,
                new NotFoundException(message: $"{typeof(LicensedFeature).Name} not found.")
            }
        };

        return data;
    }

    private void SetupAccessValidator(ErrorCodes codes = 0)
    {
        var res = new ValidationResult();
        _accessValidatorMock
            .Setup(x => x.CanAssignLicensedFeatureToTenantAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<Guid>>())).ReturnsAsync(res);

        res.SetError(codes);
    }
}