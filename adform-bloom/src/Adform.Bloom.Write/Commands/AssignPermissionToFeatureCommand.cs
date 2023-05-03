using MediatR;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class   AssignPermissionToFeatureCommand : IRequest
    {
        public AssignPermissionToFeatureCommand(ClaimsPrincipal principal, Guid featureId, Guid permissionId,
            LinkOperation operation)
        {
            Principal = principal;
            PermissionId = permissionId;
            FeatureId = featureId;
            Operation = operation;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid PermissionId { get; }
        public Guid FeatureId { get; }
        public LinkOperation Operation { get; }
    }
}