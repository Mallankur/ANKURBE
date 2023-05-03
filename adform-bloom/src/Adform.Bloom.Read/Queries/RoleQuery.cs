using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;

namespace Adform.Bloom.Read.Queries
{
    public class RoleQuery : BaseSingleQuery<QueryParamsRolesInput, Role>
    {
        public RoleQuery(ClaimsPrincipal principal, Guid id) 
            : base(principal, id, new QueryParamsRolesInput())
        {
        }
        
        public RoleQuery(ClaimsPrincipal principal, Guid id, QueryParamsRolesInput input) 
            : base(principal, id, input)
        {
        }
    }
}