using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
#warning To Remove once the flow is fully defined, in the meantime this can be used to maintain the graph manually.
    public class DeleteTenantCommand : BaseDeleteEntityCommand
    {
        public DeleteTenantCommand(ClaimsPrincipal principal, Guid tenantId)
            : base(principal, tenantId)
        {
        }
    }
}