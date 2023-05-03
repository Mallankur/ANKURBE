using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;

namespace Adform.Bloom.Read.Queries
{
    public class UserQuery : BaseSingleQuery<QueryParamsTenantIdsInput, User>
    {
        public UserQuery(ClaimsPrincipal principal, Guid id)
            : base(principal, id, new QueryParamsTenantIdsInput())
        {
        }
        
        public UserQuery(ClaimsPrincipal principal, Guid id, QueryParamsTenantIdsInput filter)
            : base(principal, id, filter)
        {
        }
    }
}