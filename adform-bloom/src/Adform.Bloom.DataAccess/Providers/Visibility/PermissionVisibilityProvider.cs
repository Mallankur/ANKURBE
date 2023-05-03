using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class PermissionVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Permission>
    {
        public PermissionVisibilityProvider(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public async Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsTenantIds filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds;
            return (await GetVisibleResourcesAsync(subject, filter, label)).Count() ==
                   resourceIds?.Count;
        }

        public async Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter, string? label = null)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var preMatch = isAdmin
                ? $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}(t0:{nameof(Tenant)})"
                : $"(t:{nameof(Tenant)})";
            var match =
                $"{preMatch}{Constants.AssignedLink.ToCypher()}(lf:{nameof(LicensedFeature)}){Constants.ContainsLink}(f:{nameof(Feature)}){Constants.ContainsLink.ToCypher()}(p:{nameof(Permission)})";
            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in {tenants}";
            if (resourceIds.Any())
            {
                where += " AND p.Id in {permissions}";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .WithParam("permissions", resourceIds.Select(x => x.ToString()))
                .ReturnDistinct(p => p.As<Permission>().Id);
            return await cypher.ResultsAsync;
        }

        public async Task<EntityPagination<Contracts.Output.Permission>> EvaluateVisibilityAsync(
            ClaimsPrincipal subject,
            QueryParamsTenantIds filter, int skip, int limit)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            if (contextId.HasValue)
                throw new NotImplementedException(
                    $"'{nameof(contextId)}' parameter is set but this feature has not been implemented yet in '{nameof(PermissionVisibilityProvider)}'");

            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();
            var preMatch = isAdmin
                ? $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}(t0:{nameof(Tenant)})"
                : $"(t:{nameof(Tenant)})";
            var match =
                $"{preMatch}{Constants.AssignedLink.ToCypher()}(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}(f:{nameof(Feature)}){Constants.ContainsLink.ToCypher()}(p:{nameof(Permission)})";
            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in {tenants}";
            if (resourceIds.Any())
            {
                where += " AND p.Id in {permissions}";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .With("p, 0 as c")
                .ReturnDistinct((p, c) => new {Nodes = p.As<Contracts.Output.Permission>(), TotalCount = c.As<int>()})
                .OrderByDual("p", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .Where(where)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .WithParam("permissions", resourceIds.Select(x => x.ToString()))
                .With("null as p, count(distinct p) as c")
                .ReturnDistinct((p, c) => new {Nodes = p.As<Contracts.Output.Permission?>(), TotalCount = c.As<int>()});
            var result = (await cypher.ResultsAsync).ToArray();

            if (result.Length == 0)
                return new EntityPagination<Contracts.Output.Permission>(skip, limit, 0,
                    new List<Contracts.Output.Permission>(0));

            return new EntityPagination<Contracts.Output.Permission>(skip, limit,
                result[^1].TotalCount,
                result[..^1].Select(r => r.Nodes!).ToList());
        }
    }
}