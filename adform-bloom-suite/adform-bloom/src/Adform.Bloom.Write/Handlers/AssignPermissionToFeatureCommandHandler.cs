using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Adform.Bloom.DataAccess;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;

namespace Adform.Bloom.Write.Handlers
{
    public class
        AssignPermissionToFeatureCommandHandler : BaseCommandHandler,
            IRequestHandler<AssignPermissionToFeatureCommand, Unit>
    {
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> _visibilityProvider;

        public AssignPermissionToFeatureCommandHandler(IAdminGraphRepository repository,
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> visibilityProvider, IMediator mediator)
            : base(repository, mediator)
        {
            _visibilityProvider = visibilityProvider;
        }

        public async Task<Unit> Handle(AssignPermissionToFeatureCommand request,
            CancellationToken cancellationToken)
        {
            ConnectedNode oldEntity;
            ConnectedNode newEntity;

            await Validate(request.Principal, request.PermissionId, request.FeatureId, request.Operation);

            if (request.Operation == LinkOperation.Assign)
            {
                await AdminGraphRepository.CreateRelationshipAsync<Feature, Permission>(p => p.Id == request.FeatureId,
                    o => o.Id == request.PermissionId, Constants.ContainsLink);
                oldEntity = new ConnectedNode(request.FeatureId, request.PermissionId, null);
                newEntity = new ConnectedNode(request.FeatureId, request.PermissionId,
                    Constants.Relationship.CONTAINS);
                await AdminGraphRepository.AssignPermissionsToRolesThroughFeatureAssignmentAsync(request.FeatureId, new Guid[] { request.PermissionId });
            }
            else
            {
                await AdminGraphRepository.UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(request.FeatureId, new Guid[] { request.PermissionId });
                await AdminGraphRepository.DeleteRelationshipAsync<Feature, Permission>(o => o.Id == request.FeatureId,
                    p => p.Id == request.PermissionId, Constants.ContainsLink);
                await AdminGraphRepository.DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(
                    request.PermissionId);

                oldEntity = new ConnectedNode(request.FeatureId, request.PermissionId,
                    Constants.Relationship.CONTAINS);
                newEntity = new ConnectedNode(request.FeatureId, request.PermissionId, null);
            }

            await Mediator.Publish(new AuditChange(request.Principal, oldEntity, newEntity, AuditOperation.Update),
                cancellationToken);
            return Unit.Value;
        }

        private async Task Validate(ClaimsPrincipal principal, Guid permissionId, Guid featureId,
            LinkOperation operation)
        {
            await AdminGraphRepository.ThrowIfNotFound<Permission>(permissionId);
            await AdminGraphRepository.ThrowIfNotFound<Feature>(featureId);

            if (!await _visibilityProvider.HasItemVisibilityAsync(principal, featureId))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessFeatures,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorMessages.SubjectCannotAccessFeatures}
                    });
            }

            if (operation == LinkOperation.Assign && (await AdminGraphRepository.GetConnectedAsync<Permission, Feature>(
                x => x.Id == permissionId,
                Constants.ContainsIncomingLink)).Any())
            {
                throw new BadRequestException(ErrorReasons.ConstraintsViolationReason,
                    ErrorMessages.PermissionCannotBeAssignedToFeature,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Permission).ToLowerFirstCharacter(), ErrorMessages.PermissionCannotBeAssignedToFeature}
                    });
            }
        }
    }
}