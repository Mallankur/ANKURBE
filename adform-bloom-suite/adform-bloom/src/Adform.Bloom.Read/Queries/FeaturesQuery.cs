using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;

namespace Adform.Bloom.Read.Queries
{
    public class FeaturesQuery : BaseRangeQuery<QueryParamsTenantIdsInput, Feature>
    {
        public FeaturesQuery(ClaimsPrincipal principal, QueryParamsTenantIdsInput filter,
            int offset = 0, int limit = 100)
            : base(principal, filter, offset, limit)
        {
        }
    }
}