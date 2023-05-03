using System;
using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
#warning To Remove once the flow is fully defined, in the meantime this can be used to maintain the graph manually.
    public class CreateTenantCommand : BaseCommandWithParentId<Tenant>
    {
        public CreateTenantCommand(ClaimsPrincipal principal,
            Guid? parentId, string name, string description = "", bool isEnabled = true)
            : base(principal, parentId, name, description, isEnabled)
        {
        }
    }
}