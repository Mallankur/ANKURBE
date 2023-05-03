using System;
using System.Collections.Generic;
using System.Linq;

namespace Adform.Bloom.Runtime.Read.Entities
{
    public class RuntimeResult
    {
        public int TenantLegacyId { get; set; } = 0;
        public string TenantType { get; set; } = string.Empty;
        public Guid TenantId { get; set; } = Guid.Empty;
        public string TenantName { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }
}