using System;

namespace Adform.Bloom.Client.Contracts.Request
{
    public class RoleExistenceRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public Guid TenantId { get; set; } = Guid.Empty;
    }
}
