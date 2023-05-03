using System;

namespace Adform.Bloom.Contracts.Input
{
    public class AssignPermissionToRole
    {
        public AssignPermissionToRole()
        {
        }

        public AssignPermissionToRole(Guid permissionId, Guid roleId, LinkOperation operation)
        {
            PermissionId = permissionId;
            RoleId = roleId;
            Operation = operation;
        }

        public Guid PermissionId { get; set; }
        public Guid RoleId { get; set; }
        public LinkOperation Operation { get; set; }
    }
}