using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Policy = Adform.Bloom.Contracts.Output.Policy;

namespace Adform.Bloom.Read.Queries
{
    public class PolicyQuery : BaseSingleQuery<QueryParamsTenantIdsInput, Policy>
    {
        public PolicyQuery(ClaimsPrincipal principal, Guid id)
            : base(principal, id, new QueryParamsTenantIdsInput())
        {
        }
        
        public PolicyQuery(ClaimsPrincipal principal, Guid id, QueryParamsTenantIdsInput filter)
            : base(principal, id, filter)
        {
        }
    }
}