using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.SharedKernel.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Ciam.OngDb.Core.Interfaces;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class DeleteRoleCommandHandlerTests : BaseTests
    {
        public DeleteRoleCommandHandlerTests()
        {
            _handler = new DeleteRoleCommandHandler(_accessService.Object,
                _adminGraphRepositoryMock.Object, _mediatorMock.Object);
        }

        private readonly DeleteRoleCommandHandler _handler;

        private readonly Mock<IAccessValidator> _accessService =
            new Mock<IAccessValidator>();

        [Fact]
        public async Task Access_Repository_Is_Used_To_Validate_Access_To_Entity()
        {
            _accessService.Setup(r => r.CanDeleteRoleAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(new ValidationResult());
            _adminGraphRepositoryMock
                .Setup(x => x.GetConnectedAsync<Role, Group>(It.IsAny<Expression<Func<Role, bool>>>(),
                    It.IsAny<ILink>())).ReturnsAsync(new List<Group>());

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await _handler.Handle(cmd, CancellationToken.None);

            _accessService.Verify(r => r.CanDeleteRoleAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>()), Times.Once);
            _accessService.Verify(r => r.CanDeleteRoleAsync(
                It.Is<ClaimsPrincipal>(p =>
                    p.GetSubId() == _claimsPrincipal.GetSubId() &&
                    p.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Count ==
                        _claimsPrincipal.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Count &&
                    p.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).All(t =>
                        _claimsPrincipal.GetTenants(Adform.Bloom.Domain.Constants.Authentication.Bloom, null).Contains(t))),
                It.Is<Guid>(id => id == cmd.IdOfEntityToDeleted)), Times.Once);
        }

        [Fact]
        public async Task Handle_Throws_Exception_If_Role_Does_Not_Exist()
        {
            var validationResult = new ValidationResult();
            validationResult.SetError(ErrorCodes.RoleDoesNotExist);
            _accessService.Setup(r => r.CanDeleteRoleAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(validationResult);

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Throws_Exception_If_Principal_Has_No_Access_To_Entity()
        {
            var validationResult = new ValidationResult();
            validationResult.SetError(ErrorCodes.SubjectCannotAccessRole);
            _accessService.Setup(r => r.CanDeleteRoleAsync(
                It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>())).ReturnsAsync(validationResult);

            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(cmd, CancellationToken.None));
        }
    }
}