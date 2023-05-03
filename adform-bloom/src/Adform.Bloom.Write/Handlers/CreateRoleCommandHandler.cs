using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Write.Mappers;
using static Adform.Bloom.DataAccess.Guard;

namespace Adform.Bloom.Write.Handlers
{
    public class CreateRoleCommandHandler : BasePayloadValidationCommandHandler<CreateRoleCommand, Role>
    {
        private readonly Guid? _defaultPolicyId;
        private readonly IRequestToEntityMapper<CreateRoleCommand, Role> _mapper;
        private readonly IAccessValidator _accessValidator;

        public CreateRoleCommandHandler(
            Guid? defaultPolicyId,
            IRequestToEntityMapper<CreateRoleCommand, Role> mapper,
            IAccessValidator accessValidator,
            IAdminGraphRepository repository,
            IMediator mediator)
            : base(repository, mediator)
        {
            _defaultPolicyId = defaultPolicyId;
            _mapper = mapper;
            _accessValidator = accessValidator;
        }

        protected override async Task<Role> HandleInternal(CreateRoleCommand request,
            CancellationToken cancellationToken)
        {
            var principal = request.Principal;
            var policyId = request.PolicyId ?? _defaultPolicyId;
            var tenantId = request.TenantId;
            var featureIds = request.FeatureIds;
            var isTemplateRole = request.IsTemplateRole;

            await Validate(principal, policyId, tenantId, featureIds, isTemplateRole);

            var result = await AdminGraphRepository.CreateNodeAsync(_mapper.Map(request));
            await AdminGraphRepository.CreateRelationshipAsync<Policy, Role>(o => o.Id == policyId,
                p => p.Id == result.Id, Constants.ContainsLink);
            if (request.IsTemplateRole)
            {
                await AdminGraphRepository.AddLabelAsync<Role>(p => p.Id == result.Id, Constants.Label.TEMPLATE_ROLE);
            }
            else
            {
                await AdminGraphRepository.AddLabelAsync<Role>(p => p.Id == result.Id, Constants.Label.CUSTOM_ROLE);
            }

            await AdminGraphRepository.CreateRelationshipAsync<Tenant, Role>(o => o.Id == tenantId,
                p => p.Id == result.Id, Constants.OwnsLink);

            if (featureIds != null)
            {
                await AdminGraphRepository.AssignPermissionsToRoleThroughFeaturesAsync(result.Id, featureIds);
            }

            await Mediator.Publish(
                new AuditEvent(principal, result.Id, nameof(Role), AuditOperation.Create),
                cancellationToken);

            return result;
        }

        private async Task Validate(
            ClaimsPrincipal principal,
            Guid? policyId,
            Guid tenantId,
            IReadOnlyCollection<Guid>? featureIds,
            bool isTemplateRole)
        {
            var res = await _accessValidator.CanCreateRole(
                principal, policyId, tenantId, featureIds, isTemplateRole);

            if (res.HasError(ErrorCodes.PolicyDoesNotExist))
                ThrowNotFound<Policy>(policyId);

            if (res.HasError(ErrorCodes.TenantDoesNotExist))
                ThrowNotFound<Tenant>(tenantId);

            if (res.HasError(ErrorCodes.FeaturesDoNotExist) && featureIds != null)
                ThrowNotFound<Feature>(featureIds);

            if (res.HasError(ErrorCodes.SubjectCannotAccessTenant))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessTenant,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Tenant).ToLowerFirstCharacter(), tenantId}
                    });

            if (res.HasError(ErrorCodes.SubjectCannotAccessFeatures))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessFeatures,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorReasons.AccessControlValidationFailedReason}
                    });
            }

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

            if (res.HasError(ErrorCodes.FeatureDependencyMissing))
            {
                throw new BadRequestException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.FeatureDependencyMissing,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorMessages.FeatureDependencyMissing}
                    });
            }
        }
    }
}