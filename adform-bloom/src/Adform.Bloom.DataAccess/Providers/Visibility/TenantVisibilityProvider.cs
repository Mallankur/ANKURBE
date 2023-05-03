using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class TenantVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsBusinessAccount, Contracts.Output.Tenant>
    {
        public TenantVisibilityProvider(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public Task<bool> HasItemVisibilityAsync(ClaimsPrincipal subject, Guid resourceId, string? label = null)
        {
            var isAdmin = subject.IsAdformAdmin();
            var tenants = isAdmin
                ? GetExistingTenants(null, new[] {resourceId}).Result.Select(i => i.ToString())
                : subject.GetTenants();

            return Task.FromResult(tenants.Contains(resourceId.ToString()));
        }

        public Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsBusinessAccount filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var tenantType = (BusinessAccountType?) filter.BusinessAccountType;
            var isAdmin = subject.IsAdformAdmin();
            var tenants = isAdmin
                ? GetExistingTenants(tenantType, resourceIds).Result.Select(i => i.ToString())
                : subject.GetTenants(limitTo: tenantIds);

            return Task.FromResult(tenants.Select(Guid.Parse).Intersect(resourceIds).Count() == resourceIds.Count);
        }

        public async Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsBusinessAccount filter, string? label = null)
        {
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var tenantType = (BusinessAccountType?) filter.BusinessAccountType;
            if (subject.IsAdformAdmin())
                return await GetExistingTenants(tenantType, resourceIds);
            var tenants = subject.GetTenants(limitTo: tenantIds);
            return tenants.Select(Guid.Parse).Intersect(resourceIds);
        }

        private async Task<IEnumerable<Guid>> GetExistingTenants(BusinessAccountType? businessAccountType,
            IReadOnlyCollection<Guid> tenantIds)
        {
            var match = $"(t:{nameof(Tenant)})";
            var where = "t.Id in {tenantIds}"; 
            if (businessAccountType.HasValue)
            {
                where += $" AND t:{businessAccountType.Value.ToString()}";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .WithParam("tenantIds", tenantIds.Select(x => x.ToString()))
                .Return(t => t.As<Contracts.Output.Tenant>());
            var result = (await cypher.ResultsAsync).ToArray();
            return result.Select(t => t.Id);
        }

        public async Task<EntityPagination<Contracts.Output.Tenant>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsBusinessAccount filter, int skip, int limit)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var businessAccountType = (BusinessAccountType?) filter.BusinessAccountType;
            var contextId = filter.ContextId;
            if (contextId.HasValue)
                throw new NotImplementedException(
                    $"'{nameof(contextId)}' parameter is set but this feature has not been implemented yet in '{nameof(TenantVisibilityProvider)}'");

            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var match = $"(t:{nameof(Tenant)})";
            var where = "t:Tenant";
            if (!isAdmin || tenantIds.Any())
            {
                where = "t.Id in {tenants}";
            }

            if (businessAccountType.HasValue)
            {
                where += $" AND t:{businessAccountType.Value.ToString()}";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .With("t, 0 as c")
                .Return((t, c) => new TenantPaginationResult
                    {Node = t.As<Contracts.Output.Tenant>(), TotalCount = c.As<int>()})
                .OrderByDual("t", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .Where(where)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .With("null as t, count(distinct t) as c")
                .Return((t, c) => new TenantPaginationResult
                    {Node = t.As<Contracts.Output.Tenant?>(), TotalCount = c.As<int>()});

            return (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        }
    }
}