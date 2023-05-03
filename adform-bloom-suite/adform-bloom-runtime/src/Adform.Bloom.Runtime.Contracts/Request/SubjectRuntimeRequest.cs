using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Request
{
    [DataContract]
    public class SubjectRuntimeRequest
    {
        [DataMember(Order = 1)] public Guid SubjectId { get; set; } = Guid.Empty;
        [DataMember(Order = 2)] public IEnumerable<Guid> TenantIds { get; set; } = Enumerable.Empty<Guid>();
        [DataMember(Order = 3)] public IEnumerable<string> PolicyNames { get; set; } = Enumerable.Empty<string>();
        [DataMember(Order = 4)] public IEnumerable<int> TenantLegacyIds { get; set; } = Enumerable.Empty<int>();
        [DataMember(Order = 5)] public string? TenantType { get; set; }
        [DataMember(Order = 6)] public bool InheritanceEnabled { get; set; } = true;
    }
}