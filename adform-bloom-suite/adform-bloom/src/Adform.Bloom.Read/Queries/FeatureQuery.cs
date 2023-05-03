using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Feature = Adform.Bloom.Contracts.Output.Feature;

namespace Adform.Bloom.Read.Queries
{
    public class FeatureQuery : BaseSingleQuery<QueryParamsTenantIdsInput, Feature>
    {
        public FeatureQuery(ClaimsPrincipal principal, Guid id)
            : base(principal, id, new QueryParamsTenantIdsInput())
        {
        }
        
        public FeatureQuery(ClaimsPrincipal principal, Guid id, QueryParamsTenantIdsInput filter)
            : base(principal, id, filter)
        {
        }
    }
}