using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Permission = Adform.Bloom.Contracts.Output.Permission;

namespace Adform.Bloom.Read.Queries
{
    public class PermissionQuery : BaseSingleQuery<QueryParamsTenantIdsInput, Permission>
    {
        public PermissionQuery(ClaimsPrincipal principal, Guid id)
            : base(principal, id, new QueryParamsTenantIdsInput())
        {
        }
        
        public PermissionQuery(ClaimsPrincipal principal, Guid id, QueryParamsTenantIdsInput filter)
            : base(principal, id, filter)
        {
        }
    }
}