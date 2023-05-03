using System;

namespace Adform.Bloom.Contracts.Input
{
    public class AssignSubjectToRole
    {
        public Guid TenantId { get; set; }
        public Guid SubjectId { get; set; }
        public Guid RoleId { get; set; }
        public LinkOperation Operation { get; set; }
    }
}