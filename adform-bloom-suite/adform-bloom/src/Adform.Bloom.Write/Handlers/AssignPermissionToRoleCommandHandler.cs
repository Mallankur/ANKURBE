using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using static Adform.Bloom.DataAccess.Guard;

namespace Adform.Bloom.Write.Handlers
{
    public class AssignPermissionToRoleCommandHandler : BaseCommandHandler,
        IRequestHandler<AssignPermissionToRoleCommand, Unit>
    {
        private readonly IAccessValidator _accessValidator;

        public AssignPermissionToRoleCommandHandler(
            IAdminGraphRepository repository,
            IAccessValidator roleAccessRepository,
            IMediator mediator) : base(repository, mediator)
        {
            _accessValidator = roleAccessRepository;
        }

        public async Task<Unit> Handle(AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
        {
            ConnectedNode oldEntity;
            ConnectedNode newEntity;

            await Validate(request);

            if (request.Operation == LinkOperation.Assign)
            {
                await AdminGraphRepository.CreateRelationshipAsync<Role, Permission>(o => o.Id == request.RoleId,
                    p => p.Id == request.PermissionId, Constants.ContainsLink);
                oldEntity = new ConnectedNode(request.RoleId, request.PermissionId, null);
                newEntity = new ConnectedNode(request.RoleId, request.PermissionId, Constants.Relationship.CONTAINS);
            }
            else
            {
                await AdminGraphRepository.DeleteRelationshipAsync<Role, Permission>(o => o.Id == request.RoleId,
                    p => p.Id == request.PermissionId, Constants.ContainsVariableLink);
                oldEntity = new ConnectedNode(request.RoleId, request.PermissionId, Constants.Relationship.CONTAINS);
                newEntity = new ConnectedNode(request.RoleId, request.PermissionId, null);
            }

            await Mediator.Publish(new AuditChange(request.Principal, oldEntity, newEntity, AuditOperation.Update),
                cancellationToken);
            return Unit.Value;
        }

        private async Task Validate(AssignPermissionToRoleCommand request)
        {
           var res = await _accessValidator.CanAssignPermissionToRoleAsync(request.Principal, request.PermissionId, request.RoleId);

            if(res.HasError(ErrorCodes.PermissionDoesNotExist))
                ThrowNotFound<Permission>(request.PermissionId);

            if (res.HasError(ErrorCodes.RoleDoesNotExist))
                ThrowNotFound<Role>(request.RoleId);

            if (res.HasError(ErrorCodes.SubjectCannotAccessPermission))
                throw new ForbiddenException(message: ErrorMessages.PermissionIsNotInFeature);

            if (res.HasError(ErrorCodes.SubjectCannotAccessRole))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessRole);
        }
    }
}