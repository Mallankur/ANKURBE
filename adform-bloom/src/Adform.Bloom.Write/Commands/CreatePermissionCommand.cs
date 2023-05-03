using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
    public class CreatePermissionCommand : NamedCreateCommand<Permission>
    {
        public CreatePermissionCommand(ClaimsPrincipal principal, 
            string name, string description = "", bool isEnabled = true)
            : base(principal, name, description, isEnabled)
        {
        }
    }
}