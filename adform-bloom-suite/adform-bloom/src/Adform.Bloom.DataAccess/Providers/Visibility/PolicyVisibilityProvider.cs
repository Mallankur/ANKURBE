using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class PolicyVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Policy>
    {
        public PolicyVisibilityProvider(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public Task<bool> HasItemVisibilityAsync(ClaimsPrincipal subject, Guid resourceId, string? label = null)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsTenantIds filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds;
            return (await GetVisibleResourcesAsync(subject, filter, label)).Count() ==
                   resourceIds?.Count;
        }

        public Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter, string? label = null)
        {
            var resourceIds = filter?.ResourceIds;
            return Task.FromResult((resourceIds ?? new List<Guid>()).AsEnumerable());
        }

        public async Task<EntityPagination<Contracts.Output.Policy>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter, int skip, int limit)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            if (contextId.HasValue)
                throw new NotImplementedException(
                    $"'{nameof(contextId)}' parameter is set but this feature has not been implemented yet in '{nameof(PolicyVisibilityProvider)}'");

            if (tenantIds.Any())
                throw new NotImplementedException(
                    $"'{nameof(tenantIds)}' parameter is set but this feature his not supported in '{nameof(PolicyVisibilityProvider)}'");

            var match = $"(r:{nameof(Policy)})";
            var cypher = (await GraphClient).Cypher
                .Match(match)
                .With("r, 0 as c")
                .ReturnDistinct((r, c) => new {Nodes = r.As<Contracts.Output.Policy>(), TotalCount = c.As<int>()})
                .OrderByDual("r", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .With("null as r, count(distinct r) as c")
                .ReturnDistinct((r, c) => new {Nodes = r.As<Contracts.Output.Policy?>(), TotalCount = c.As<int>()});
            var result = (await cypher.ResultsAsync).ToArray();

            if (result.Length == 0)
                return new EntityPagination<Contracts.Output.Policy>(skip, limit, 0,
                    new List<Contracts.Output.Policy>(0));

            return new EntityPagination<Contracts.Output.Policy>(skip, limit,
                result[^1].TotalCount,
                result[..^1].Select(r => r.Nodes!).ToList());
        }
    }
}