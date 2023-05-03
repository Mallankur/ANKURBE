using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;

namespace Adform.Bloom.Read.Queries
{
    public class LicensedFeatureQuery : BaseSingleQuery<QueryParamsTenantIdsAndPolicyTypesInput, LicensedFeature>
    {
        public LicensedFeatureQuery(ClaimsPrincipal principal, Guid id) :
            base(principal, id, new QueryParamsTenantIdsAndPolicyTypesInput())
        {
        }
        
        public LicensedFeatureQuery(ClaimsPrincipal principal, Guid id, QueryParamsTenantIdsAndPolicyTypesInput filter) :
            base(principal, id, filter)
        {
        }
    }
}