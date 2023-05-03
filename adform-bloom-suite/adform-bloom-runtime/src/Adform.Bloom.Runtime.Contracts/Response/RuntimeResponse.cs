using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Adform.Bloom.Client.Contracts.Response
{
    [DataContract]
    public class RuntimeResponse
    {
        [DataMember(Order = 1)]
        public int TenantLegacyId { get; set; } = 0; 
        [DataMember(Order = 2)]
        public string TenantType { get; set; } = string.Empty;
        [DataMember(Order = 3)]
        public Guid TenantId { get; set; } = Guid.Empty;
        [DataMember(Order = 4)]
        public string TenantName { get; set; } = string.Empty; 
        [DataMember(Order = 5)]
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
        [DataMember(Order = 6)]
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }
}