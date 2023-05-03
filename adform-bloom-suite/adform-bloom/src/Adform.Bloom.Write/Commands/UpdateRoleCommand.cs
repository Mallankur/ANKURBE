using System;
using System.Security.Claims;
using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Commands
{
    public class UpdateRoleCommand : NamedCreateCommand<Role>
    {
        public UpdateRoleCommand(ClaimsPrincipal principal,
            Guid roleId,
            string name,
            string description = "",
            bool isEnabled = true,
            long updatedAt = 0)
            : base(principal, name, description, isEnabled)
        {
            RoleId = roleId;
            UpdatedAt = updatedAt;
        }

        public Guid RoleId { get; }
    }
}