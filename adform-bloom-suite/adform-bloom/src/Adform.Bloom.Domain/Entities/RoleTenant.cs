using System;

namespace Adform.Bloom.Domain.Entities
{
    public class RoleTenant
    {
        public Guid RoleId { get; set; }
        public Guid TenantId { get; set; }
    }
}