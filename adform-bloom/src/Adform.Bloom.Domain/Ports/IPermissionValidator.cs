using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Adform.Bloom.Domain.Ports
{
    public interface IPermissionValidator
    {
        Task<bool> DoesPermissionExist(Guid permissionId);

        Task<bool> HasVisibilityToPermissionAsync(ClaimsPrincipal principal, Guid permissionId,
            IReadOnlyCollection<Guid>? tenantIds = null);
    }
}
