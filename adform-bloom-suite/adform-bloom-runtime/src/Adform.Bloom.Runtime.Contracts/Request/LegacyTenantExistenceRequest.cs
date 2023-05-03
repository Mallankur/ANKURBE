using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Request
{
    [DataContract]
    public class LegacyTenantExistenceRequest
    {
        [DataMember(Order = 1)] public List<int> TenantLegacyIds { get; set; } = new List<int>();
        [DataMember(Order = 2)] public string TenantType { get; set; } = string.Empty;
    }
}