using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;

namespace Adform.Bloom.Write.Handlers
{
    public class UpdateLicensedFeatureToTenantAssignmentsCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateLicensedFeatureToTenantAssignmentsCommand, Unit>
    {
        private readonly IAccessValidator _accessValidator;

        public UpdateLicensedFeatureToTenantAssignmentsCommandHandler(
            IAdminGraphRepository repository,
            IAccessValidator accessValidator,
            IMediator mediator) 
            : base(repository, mediator)
        {
            _accessValidator = accessValidator;
        }

        public async Task<Unit> Handle(UpdateLicensedFeatureToTenantAssignmentsCommand request, CancellationToken cancellationToken)
        {
            await Validate(request.Principal, request.AssignLicensedFeaturesIds, request.UnassignLicensedFeaturesIds,
                request.TenantId);

            var assignTask = request.AssignLicensedFeaturesIds != null
                ? AdminGraphRepository.AssignLicensedFeaturesToTenantAsync(request.TenantId, request.AssignLicensedFeaturesIds)
                : Task.CompletedTask;
            var assignPermissionsToTraffickersTask = request.AssignLicensedFeaturesIds != null
                ? AdminGraphRepository.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(
                    request.AssignLicensedFeaturesIds,
                    request.TenantId)
                : Task.CompletedTask;
            var unassignTask = request.UnassignLicensedFeaturesIds != null
                ? AdminGraphRepository.UnassignLicensedFeaturesFromTenantAsync(request.TenantId,
                    request.UnassignLicensedFeaturesIds)
                : Task.CompletedTask;

            var unassignPermissionsFromTraffickersTask = request.UnassignLicensedFeaturesIds != null
                ? AdminGraphRepository.UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(
                    request.UnassignLicensedFeaturesIds,
                    request.TenantId)
                : Task.CompletedTask;

            await Task.WhenAll(assignTask, unassignTask,assignPermissionsToTraffickersTask, unassignPermissionsFromTraffickersTask);
            return Unit.Value;
        }
        private async Task Validate(ClaimsPrincipal principal, IEnumerable<Guid>? assignLicensedFeatureIds,
            IEnumerable<Guid>? unassignLicensedFeatureId, Guid tenantId)
        {
            var res = new ValidationResult();

            if (assignLicensedFeatureIds != null)
            {
                var result =
                    await _accessValidator.CanAssignLicensedFeatureToTenantAsync(principal, tenantId,
                        assignLicensedFeatureIds.ToList());
                res.SetError(result.Error);
            }

            if (unassignLicensedFeatureId != null)
            {
                var result =
                    await _accessValidator.CanAssignLicensedFeatureToTenantAsync(principal, tenantId,
                        unassignLicensedFeatureId.ToList());
                res.SetError(result.Error);
            }

            if (res.HasError(ErrorCodes.TenantDoesNotExist))
                Guard.ThrowNotFound<Tenant>();

            if (res.HasError(ErrorCodes.LicensedFeaturesDoNotExist))
                Guard.ThrowNotFound<LicensedFeature>();

            if (res.HasError(ErrorCodes.SubjectCannotAccessTenant))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessTenant);

        }
    }
}