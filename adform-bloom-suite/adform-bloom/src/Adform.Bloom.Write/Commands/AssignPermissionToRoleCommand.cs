using MediatR;
using System;
using System.Security.Claims;

namespace Adform.Bloom.Write.Commands
{
    public class AssignPermissionToRoleCommand : IRequest
    {
        public AssignPermissionToRoleCommand(ClaimsPrincipal principal, Guid permissionId, Guid roleId,
            LinkOperation operation)
        {
            Principal = principal;
            PermissionId = permissionId;
            RoleId = roleId;
            Operation = operation;
        }

        public ClaimsPrincipal Principal { get; }
        public Guid PermissionId { get; }
        public Guid RoleId { get; }
        public LinkOperation Operation { get; }
    }
}