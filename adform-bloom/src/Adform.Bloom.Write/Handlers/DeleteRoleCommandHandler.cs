using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Neo4jClient.Extensions;
using Adform.Bloom.Domain.Entities;
using static Adform.Bloom.DataAccess.Guard;

namespace Adform.Bloom.Write.Handlers
{
    public class DeleteRoleCommandHandler : BaseDeleteCommandHandler<Role>
    {
        private readonly IAccessValidator _accessValidator;

        public DeleteRoleCommandHandler(
            IAccessValidator accessValidator,
            IAdminGraphRepository repository,
            IMediator mediator)
            : base(repository, mediator)
        {
            _accessValidator = accessValidator;
        }

        protected override async Task PreDeleteValidation(BaseDeleteEntityCommand request, CancellationToken cancellationToken)
        {
            var res = await _accessValidator.CanDeleteRoleAsync(request.Principal, request.IdOfEntityToDeleted);
            if (res.HasError(ErrorCodes.RoleDoesNotExist))
                ThrowNotFound<Role>(request.IdOfEntityToDeleted);
            
            if (res.HasError(ErrorCodes.SubjectCannotAccessRole))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotDeleteEntity);
        }

        protected override async Task PreDeleteAction(BaseDeleteEntityCommand request,
            CancellationToken cancellationToken)
        {
            var groups = (await AdminGraphRepository
                .GetConnectedAsync<Role, Group>(
                    r => r.Id == request.IdOfEntityToDeleted, Constants.AssignedIncomingLink))
                .Select(g => g.Id)
                .ToArray();

            if (!groups.Any()) return;

            await AdminGraphRepository.DeleteNodeAsync<Group>(g => g.Id.In(groups), true);
        }
    }
}