using System;
using System.Collections.Generic;

namespace Adform.Bloom.Infrastructure.Models
{
    public class QueryParamsTenantIdsAndPolicyTypes : QueryParams
    {
        public IReadOnlyCollection<string>? PolicyTypes { get; set; }
        public IReadOnlyCollection<Guid>? TenantIds { get; set; }
    }
}