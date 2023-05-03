using System;
using System.Security.Claims;
using MediatR;

namespace Adform.Bloom.Write.Commands
{
    public class AssignFeatureCoDependencyCommand : IRequest
    {
        public AssignFeatureCoDependencyCommand(ClaimsPrincipal principal, Guid featureId, Guid dependentOnId,
            LinkOperation operation)
        {
            Principal = principal;
            FeatureId = featureId;
            DependentOnId = dependentOnId;
            Operation = operation;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid FeatureId { get; }
        public Guid DependentOnId { get; }
        public LinkOperation Operation { get; }
    }
}