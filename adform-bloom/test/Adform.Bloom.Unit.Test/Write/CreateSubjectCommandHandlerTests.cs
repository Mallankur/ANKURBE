using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Bloom.Write.Mappers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using CorrelationId;
using CorrelationId.Abstractions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write;

public class CreateSubjectCommandHandlerTests : BaseTests
{
    private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();
    private readonly Mock<ICorrelationContextAccessor> _correlationContextMock = new Mock<ICorrelationContextAccessor>();
    private CreateSubjectCommandHandler _handler;

    public CreateSubjectCommandHandlerTests()
    {
        _correlationContextMock.Setup(p => p.CorrelationContext)
            .Returns(new CorrelationContext(Guid.Empty.ToString(), "CorrelationId"));
        _mediatorMock.Setup(o => o.Send(It.IsAny<UpdateSubjectAssignmentsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RuntimeResponse>());
        _handler = new CreateSubjectCommandHandler(
            new SubjectMapper(),
            _accessValidatorMock.Object,
            _adminGraphRepositoryMock.Object,
            _mediatorMock.Object,
            _correlationContextMock.Object);
    }

    [Fact]
    public async Task Handle_Creates_Subject_Fails_If_Subject_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();
        _accessValidatorMock.Setup(p => p.CanCreateSubjectAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);
        var request =
            new CreateSubjectCommand(Common.BuildPrincipal(Guid.NewGuid().ToString()), id, $"{id}@test", true);
        _adminGraphRepositoryMock.Setup(m => m.CreateNodeAsync(It.IsAny<Subject>()))
            .ReturnsAsync(new Subject() { Id = id });
        // Act
        var exceptionResult = await Record.ExceptionAsync(() => _handler.Handle(request, CancellationToken.None));

        // Assert
        Assert.Equal(exceptionResult.GetType(), typeof(ForbiddenException));
        _adminGraphRepositoryMock.Verify(m => m.CreateNodeAsync(It.IsAny<Subject>()), Times.Never);
        _mediatorMock.Verify(p => p.Send(It.IsAny<UpdateSubjectAssignmentsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Creates_Subject()
    {
        // Arrange
        var id = Guid.NewGuid();
        _accessValidatorMock.Setup(p => p.CanCreateSubjectAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);
        var request =
            new CreateSubjectCommand(Common.BuildPrincipal(Guid.NewGuid().ToString()), id, $"{id}@test", true);
        _adminGraphRepositoryMock.Setup(m => m.CreateNodeAsync(It.IsAny<Subject>()))
            .ReturnsAsync(new Subject() { Id = id });
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(id, result.Id);
        _adminGraphRepositoryMock.Verify(m => m.CreateNodeAsync(It.IsAny<Subject>()), Times.Once);
        _mediatorMock.Verify(p => p.Send(It.IsAny<UpdateSubjectAssignmentsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Creates_Subject_With_Assignments()
    {
        // Arrange
        _accessValidatorMock.Setup(p => p.CanCreateSubjectAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()))
            .ReturnsAsync(true);
        var subId = Guid.NewGuid();

        var request =
            new CreateSubjectCommand(Common.BuildPrincipal(Guid.NewGuid().ToString()), subId, $"{subId}@test", true,
                roleTenantIds: new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.NewGuid(),
                        TenantId = Guid.NewGuid()
                    },
                    new RoleTenant
                    {
                        RoleId = Guid.NewGuid(),
                        TenantId = Guid.NewGuid()
                    }
                });
        _adminGraphRepositoryMock.Setup(m => m.CreateNodeAsync(It.IsAny<Subject>()))
            .ReturnsAsync(new Subject()
            {
                Id = subId
            });
        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Equal(subId, result.Id);
        _adminGraphRepositoryMock.Verify(m => m.CreateNodeAsync(It.IsAny<Subject>()), Times.Once);
        _mediatorMock.Verify(p => p.Send(
            It.Is<UpdateSubjectAssignmentsCommand>(p => p.AssignRoleTenantIds.SequenceEqual(request.RoleTenantIds) 
                                                        && p.SubjectId == request.Id), It.IsAny<CancellationToken>()), Times.Once);
    }
}