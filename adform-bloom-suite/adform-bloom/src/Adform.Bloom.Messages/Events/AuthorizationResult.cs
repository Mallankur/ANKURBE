using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Adform.Bloom.Messages.Events
{
    [DataContract]
    public class AuthorizationResult
    {
        [DataMember(Name = "business_account_id", Order = 1)]
        public Guid TenantId { get; set; } = Guid.Empty;
        [DataMember(Name = "business_account_name", Order = 2)]
        public string TenantName { get; set; } = string.Empty;
        [DataMember(Name = "roles", Order = 3)]
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        [DataMember(Name = "permissions", Order = 4)]
        public IEnumerable<string> Permissions { get; set; } = new List<string>();
    }
}