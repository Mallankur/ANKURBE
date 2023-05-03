using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Write;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class AssignFeatureCoDependencyCommandHandlerTests : BaseTests
    {
        private readonly Mock<IAccessValidator> _accessValidatorMock = new Mock<IAccessValidator>();
        private readonly AssignFeatureCoDependencyCommandHandler _handler;

        public AssignFeatureCoDependencyCommandHandlerTests()
        {
            _handler = new AssignFeatureCoDependencyCommandHandler(_adminGraphRepositoryMock.Object,
                _mediatorMock.Object, _accessValidatorMock.Object);
        }

        [Fact]
        public async Task Feature_Does_Not_Exist_Throws_Exception()
        {
            // Arrange
            SetupAccessValidator(ErrorCodes.FeaturesDoNotExist);
            
            // Act
            // Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _handler.Handle(
                    new AssignFeatureCoDependencyCommand(Common.BuildPrincipal(), Guid.NewGuid(), Guid.NewGuid(),
                        LinkOperation.Assign), CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Principal_Doesnt_Have_Access_To_Feature_Throws_Exception()
        {
            // Arrange
            SetupAccessValidator(ErrorCodes.SubjectCannotAccessFeatures);
            
            // Act
            // Assert
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(
                    new AssignFeatureCoDependencyCommand(Common.BuildPrincipal(), Guid.NewGuid(), Guid.NewGuid(),
                        LinkOperation.Assign), CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Feature_Circular_Dependency_Throws_Exception()
        {
            // Arrange
            SetupAccessValidator(ErrorCodes.CircularDependency);
            
            // Act
            // Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _handler.Handle(
                    new AssignFeatureCoDependencyCommand(Common.BuildPrincipal(), Guid.NewGuid(), Guid.NewGuid(),
                        LinkOperation.Assign), CancellationToken.None));
            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Creates_Relationship()
        {
            // Arrange
            SetupAccessValidator();
            
            // Act
            await _handler.Handle(
                new AssignFeatureCoDependencyCommand(Common.BuildPrincipal(), Guid.NewGuid(), Guid.NewGuid(),
                    LinkOperation.Assign), CancellationToken.None);
            
            // Assert
            _adminGraphRepositoryMock.Verify(x =>
                x.CreateRelationshipAsync(It.IsAny<Expression<Func<Feature, bool>>>(),
                    It.IsAny<Expression<Func<Feature, bool>>>(), It.IsAny<ILink>()));
        }

        [Fact]
        public async Task Handle_Removes_Relationship()
        {
            // Arrange
            SetupAccessValidator();
            
            // Act
            await _handler.Handle(
                new AssignFeatureCoDependencyCommand(Common.BuildPrincipal(), Guid.NewGuid(), Guid.NewGuid(),
                    LinkOperation.Unassign), CancellationToken.None);
            
            // Assert
            _adminGraphRepositoryMock.Verify(x =>
                x.DeleteRelationshipAsync(It.IsAny<Expression<Func<Feature, bool>>>(),
                    It.IsAny<Expression<Func<Feature, bool>>>(), It.IsAny<ILink>()));
        }

        private void SetupAccessValidator(ErrorCodes codes = 0)
        {
            var res = new ValidationResult();
            res.SetError(codes);
            _accessValidatorMock
                .Setup(x => x.CanCreateFeatureCoDependency(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(),
                    It.IsAny<Guid>(), It.IsAny<bool>())).ReturnsAsync(res);
        }
    }
}