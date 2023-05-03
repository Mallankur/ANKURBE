using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class DeletePermissionCommand : BaseDeleteEntityCommand
    {
        public DeletePermissionCommand(ClaimsPrincipal principal, Guid permissionId)
            : base(principal, permissionId)
        {
        }
    }
}