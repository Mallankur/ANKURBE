using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Domain.Ports
{
    public interface ISubjectValidator
    {
        Task<bool> IsSameSubject(ClaimsPrincipal principal, Guid subjectId);

        Task<bool> SubjectExists(Guid subjectId);

        Task<bool> HasVisibilityToSubjectAsync(ClaimsPrincipal principal, Guid subjectId,
            IReadOnlyCollection<Guid>? tenantIds = null);

        Task<bool> HasEnoughRoleAssignmentCapacityAsync(Guid subjectId, IEnumerable<RoleTenant>? assignments,
            IEnumerable<RoleTenant>? unassignments);
    }
}
