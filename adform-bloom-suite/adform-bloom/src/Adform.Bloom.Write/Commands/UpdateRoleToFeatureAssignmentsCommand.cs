using MediatR;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class UpdateRoleToFeatureAssignmentsCommand : IRequest<MediatR.Unit>
    {
        public UpdateRoleToFeatureAssignmentsCommand(ClaimsPrincipal principal,
            Guid roleId,
            IReadOnlyCollection<Guid>? assignFeatureIds = null,
            IReadOnlyCollection<Guid>? unassignFeatureIds = null)
        {
            Principal = principal;
            RoleId = roleId;
            AssignFeatureIds = assignFeatureIds;
            UnassignFeatureIds = unassignFeatureIds;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid RoleId { get; }
        public IReadOnlyCollection<Guid>? AssignFeatureIds { get; }
        public IReadOnlyCollection<Guid>? UnassignFeatureIds { get; }
    }
}