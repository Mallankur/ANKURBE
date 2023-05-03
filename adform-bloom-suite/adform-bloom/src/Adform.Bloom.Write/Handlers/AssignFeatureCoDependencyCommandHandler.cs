using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;

namespace Adform.Bloom.Write.Handlers
{
    public class AssignFeatureCoDependencyCommandHandler : BaseCommandHandler,
        IRequestHandler<AssignFeatureCoDependencyCommand, Unit>
    {
        private readonly IAccessValidator _validator;

        public AssignFeatureCoDependencyCommandHandler(IAdminGraphRepository repository, IMediator mediator,
            IAccessValidator validator)
            : base(repository, mediator)
        {
            _validator = validator;
        }

        public async Task<Unit> Handle(AssignFeatureCoDependencyCommand request, CancellationToken cancellationToken)
        {
            await Task.WhenAll(
                Validate(request.Principal, request.FeatureId, request.DependentOnId, request.Operation),
                request.Operation == LinkOperation.Assign
                    ? AdminGraphRepository.CreateRelationshipAsync<Feature, Feature>(f => f.Id == request.FeatureId,
                        d => d.Id == request.DependentOnId, Constants.DependsOnLink)
                    : AdminGraphRepository.DeleteRelationshipAsync<Feature, Feature>(f => f.Id == request.FeatureId,
                        d => d.Id == request.DependentOnId, Constants.DependsOnLink));
            await Mediator.Publish(
                new AuditChange(
                    request.Principal,
                    new ConnectedNode(request.FeatureId, request.DependentOnId, null),
                    new ConnectedNode(request.FeatureId, request.DependentOnId, Constants.Relationship.DEPENDS_ON),
                    AuditOperation.Update), cancellationToken);

            return Unit.Value;
        }

        private async Task Validate(ClaimsPrincipal principal, Guid featureId, Guid dependentOnId,
            LinkOperation operation)
        {
            var result = await _validator.CanCreateFeatureCoDependency(principal, featureId, dependentOnId,
                operation == LinkOperation.Assign);

            if (result.HasError(ErrorCodes.FeaturesDoNotExist))
            {
                Guard.ThrowNotFound<Feature>();
            }

            if (result.HasError(ErrorCodes.SubjectCannotAccessFeatures))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessFeatures,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorReasons.AccessControlValidationFailedReason}
                    });
            }

            if (result.HasError(ErrorCodes.CircularDependency))
            {
                throw new BadRequestException(ErrorReasons.ConstraintsViolationReason,
                    ErrorMessages.FeatureCannotBeCoDependentOnFeature,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Feature).ToLowerFirstCharacter(), ErrorMessages.FeatureCannotBeCoDependentOnFeature}
                    });
            }
        }
    }
}