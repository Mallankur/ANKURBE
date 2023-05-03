using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.SharedKernel.Extensions;
using Moq;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Interfaces;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class DeleteSubjectCommandHandlerTests : BaseTests
    {
        public DeleteSubjectCommandHandlerTests()
        {
            _handler = new DeleteSubjectCommandHandler(
                _adminGraphRepositoryMock.Object,
                _validator.Object,
                _mediatorMock.Object);
        }

        private readonly DeleteSubjectCommandHandler _handler;

        private readonly Mock<IAccessValidator> _validator =
            new Mock<IAccessValidator>();

        [Fact]
        public async Task Access_Validator_Is_Used_To_Validate_Access_To_Entity()
        {
            _validator.Setup(r => r.CanDeleteSubjectAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(true);

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await _handler.Handle(cmd, CancellationToken.None);

            _validator.Verify(r => r.CanDeleteSubjectAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()), Times.Once);
            _validator.Verify(r => r.CanDeleteSubjectAsync(
                It.Is<ClaimsPrincipal>(p =>
                    p.GetSubId() == _claimsPrincipal.GetSubId() &&
                    p.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Count ==
                        _claimsPrincipal.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Count &&
                    p.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).All(
                        t => _claimsPrincipal.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Contains(t))),
                It.Is<Guid>(id => id == cmd.IdOfEntityToDeleted)), Times.Once);
        }

        [Fact]
        public async Task Handle_Throws_Exception_If_Principal_Has_No_Access_To_Entity()
        {
            _validator.Setup(r => r.CanDeleteSubjectAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(false);

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Exception_If_ActorId_Equals_SubjectId()
        {
            _validator.Setup(r => r.CanDeleteSubjectAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(false);

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.Parse(_claimsPrincipal.GetActorId()));

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }
    }
}