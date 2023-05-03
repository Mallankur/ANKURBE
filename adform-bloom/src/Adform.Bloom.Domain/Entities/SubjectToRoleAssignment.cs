using System;

namespace Adform.Bloom.Domain.Entities
{
    public class SubjectToRoleAssignment
    {
        public Guid RoleId { get; set; }
        public Guid TenantId { get; set; }
        public Guid SubjectId { get; set; }
        public Group? Group { get; set; }
    }
}