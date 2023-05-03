using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class DeleteRoleCommand : BaseDeleteEntityCommand
    {
        public DeleteRoleCommand(ClaimsPrincipal principal, Guid roleId)
            : base(principal, roleId)
        {
        }
    }
}