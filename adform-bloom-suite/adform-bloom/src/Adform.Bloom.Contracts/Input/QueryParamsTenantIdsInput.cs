using System;
using System.Collections.Generic;

namespace Adform.Bloom.Contracts.Input
{
    public class QueryParamsTenantIdsInput : QueryParamsInput
    {       
        public IReadOnlyCollection<Guid>? TenantIds { get; set; }
    }
}