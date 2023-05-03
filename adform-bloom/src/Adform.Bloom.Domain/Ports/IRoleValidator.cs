using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface IRoleValidator
    {
        Task<bool> DoesRoleExist(Guid roleId, string? label = null);

        Task<Guid> GetRoleOwner(Guid roleId);

        Task<bool> HasVisibilityToRoleAsync(ClaimsPrincipal principal, Guid roleId,
            IReadOnlyCollection<Guid>? tenantIds = null);

        Task<bool> CanEditRoleAsync(ClaimsPrincipal principal, Guid roleId);

        Task<bool> DoRolesExist(IReadOnlyCollection<Guid> roleIds);

        Task<bool> HasVisibilityToRolesAsync(ClaimsPrincipal principal, IReadOnlyCollection<Guid> roleIds,
            IReadOnlyCollection<Guid>? tenantIds = null);
    }
}
