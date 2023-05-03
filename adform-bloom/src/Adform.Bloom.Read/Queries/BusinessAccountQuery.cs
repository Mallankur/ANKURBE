using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;

namespace Adform.Bloom.Read.Queries
{
    public class BusinessAccountQuery : BaseSingleQuery<QueryParamsBusinessAccountInput, BusinessAccount>
    {
        public BusinessAccountQuery(ClaimsPrincipal principal, Guid id)
            : base(principal, id, new QueryParamsBusinessAccountInput())
        {
        }
        
        public BusinessAccountQuery(ClaimsPrincipal principal, Guid id, QueryParamsBusinessAccountInput filter)
            : base(principal, id, filter)
        {
        }
    }
}