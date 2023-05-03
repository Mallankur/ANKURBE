using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Runtime.Contracts.Response;
using MediatR;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
    public class UpdateSubjectAssignmentsCommand : IRequest<IEnumerable<RuntimeResponse>>
    {
        public ClaimsPrincipal Principal { get; }
        public Guid SubjectId { get; }
        public IReadOnlyCollection<RoleTenant>? AssignRoleTenantIds { get; }
        public IReadOnlyCollection<RoleTenant>? UnassignRoleTenantIds { get; }
        public IReadOnlyCollection<AssetsReassignment>? AssetsReassignments { get; }

        public UpdateSubjectAssignmentsCommand(ClaimsPrincipal principal,
            Guid subjectId,
            IReadOnlyCollection<RoleTenant>? assignRoleTenantIds = null,
            IReadOnlyCollection<RoleTenant>? unassignRoleTenantIds = null,
            IReadOnlyCollection<AssetsReassignment>? assetsReassignments = null)
        {
            Principal = principal;
            SubjectId = subjectId;
            AssignRoleTenantIds = assignRoleTenantIds;
            UnassignRoleTenantIds = unassignRoleTenantIds;
            AssetsReassignments = assetsReassignments;
        }
    }
}