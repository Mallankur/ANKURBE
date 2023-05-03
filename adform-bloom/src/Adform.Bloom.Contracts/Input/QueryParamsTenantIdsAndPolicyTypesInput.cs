using System;
using System.Collections.Generic;

namespace Adform.Bloom.Contracts.Input
{
    public class QueryParamsTenantIdsAndPolicyTypesInput : QueryParamsInput
    {       
        public IReadOnlyCollection<string>? PolicyTypes { get; set; }
        public IReadOnlyCollection<Guid>? TenantIds { get; set; }
    }
}