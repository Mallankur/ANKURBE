using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using static Adform.Bloom.DataAccess.Guard;

namespace Adform.Bloom.Write.Handlers
{
    public class UpdateRoleToFeatureAssignmentsCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateRoleToFeatureAssignmentsCommand, Unit>
    {
        private readonly IAccessValidator _accessValidator;
        private readonly IBloomCacheManager _cache;

        public UpdateRoleToFeatureAssignmentsCommandHandler(
            IAdminGraphRepository repository,
            IMediator mediator,
            IAccessValidator accessValidator, IBloomCacheManager cache)
            : base(repository, mediator)
        {
            _accessValidator = accessValidator;
            _cache = cache;
        }

        public async Task<Unit> Handle(UpdateRoleToFeatureAssignmentsCommand request, CancellationToken cancellationToken)
        {
            var principal = request.Principal;
            var roleId = request.RoleId;

            await Validate(principal, roleId, request.AssignFeatureIds, request.UnassignFeatureIds);
            var node = await AdminGraphRepository.GetNodeAsync<Role>(o => o.Id == roleId);

            if (request.AssignFeatureIds != null && request.AssignFeatureIds.Any())
            {
                await AdminGraphRepository.AssignPermissionsToRoleThroughFeaturesAsync(roleId,
                    request.AssignFeatureIds);
            }

            if (request.UnassignFeatureIds != null && request.UnassignFeatureIds.Any())
            {
                await AdminGraphRepository.UnassignPermissionsFromRoleThroughFeaturesAsync(roleId,
                    request.UnassignFeatureIds);
            }

            await _cache.ForgetByRoleAsync(node!.Name, cancellationToken);
            return Unit.Value;
        }


        private async Task Validate(
            ClaimsPrincipal principal,
            Guid roleId,
            IReadOnlyCollection<Guid>? assignFeatureIds,
            IReadOnlyCollection<Guid>? unassignFeatureIds)
        {
            //We need to check the principal 

            var res = await _accessValidator.CanUpdateRole(principal, roleId);

            if (res.HasError(ErrorCodes.RoleDoesNotExist))
                ThrowNotFound<Role>(roleId);

            if (res.HasError(ErrorCodes.SubjectCannotAccessRole))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessRole,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Role).ToLowerFirstCharacter(), ErrorMessages.SubjectCannotAccessRole}
                    });
            }

            var featuresWithoutAccess = new List<Guid>();
            if (assignFeatureIds != null && assignFeatureIds.Any())
            {
                var assRes = await _accessValidator.CanAssignRoleToFeaturesAsync(principal, roleId, assignFeatureIds);
                res.SetError(assRes.Error);
                if (assRes.HasError(ErrorCodes.SubjectCannotAccessFeatures))
                {
                    featuresWithoutAccess.AddRange(await _accessValidator.FilterFeatureIdsWithAccessDeniedAsync(principal, roleId, assignFeatureIds));
                }
            }

            if (unassignFeatureIds != null && unassignFeatureIds.Any())
            {
                var assRes = await _accessValidator.CanAssignRoleToFeaturesAsync(principal, roleId, unassignFeatureIds);
                res.SetError(assRes.Error);
                if (assRes.HasError(ErrorCodes.SubjectCannotAccessFeatures))
                {
                    featuresWithoutAccess.AddRange(await _accessValidator.FilterFeatureIdsWithAccessDeniedAsync(principal, roleId, unassignFeatureIds));
                }

            }

            if (res.HasError(ErrorCodes.SubjectCannotAccessFeatures))
            {

                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessFeatures,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorMessages.SubjectCannotAccessFeatures},
                        {"denied_features", featuresWithoutAccess}
                    });
            }
        }
    }
}