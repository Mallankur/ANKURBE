using System;
using System.Collections.Generic;

namespace Adform.Bloom.Infrastructure.Models
{
    public class QueryParamsTenantIds : QueryParams
    {
        public IReadOnlyCollection<Guid>? TenantIds { get; set; }
    }
}