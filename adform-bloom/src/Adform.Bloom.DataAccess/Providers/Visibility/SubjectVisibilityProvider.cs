using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Extensions;
using Neo4jClient.Transactions;
using Subject = Adform.Bloom.Domain.Entities.Subject;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class SubjectVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Subject>
    {
        public SubjectVisibilityProvider(ITransactionalGraphClient graphClient)
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
            QueryParamsTenantIds filter,
            string? label = null)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in {tenants}";
            var cypher = (await GraphClient).Cypher
                .Match(
                    $"(t:{nameof(Tenant)}){Constants.ChildOfIncomingDepthLink.ToCypher()}" +
                    $"(t0:{nameof(Tenant)}){Constants.BelongsIncomingLink.ToCypher()}" +
                    $"(g:{nameof(Group)}){Constants.MemberOfIncomingLink.ToCypher()}" +
                    $"(s:{nameof(Subject)})")
                .Where(where)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .AndWhere((Subject s) => s.Id.In(resourceIds))
                .ReturnDistinct(s => s.As<Subject>().Id);
            return await cypher.ResultsAsync;
        }

        public async Task<EntityPagination<Contracts.Output.Subject>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter, int skip, int limit)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var whereNotTrafficker = $"NOT s:{Constants.Label.TRAFFICKER}";
            var where = tenants.Any()?"t.Id in {tenants}": "t:Tenant";
            
            if (resourceIds.Any())
            {
                where += " and s.Id in {includeIds}";
            }

            var andWhere = "true";

            if (tenantIds.Any())
            {
                andWhere = "t0.Id in {tenants}";
            }

            var match =
                $"(t:{nameof(Contracts.Output.Tenant)}){Constants.ChildOfIncomingDepthLink.ToCypher()}" +
                $"(t0:{nameof(Contracts.Output.Tenant)}){Constants.BelongsIncomingLink.ToCypher()}" +
                $"(g:{nameof(Group)}){Constants.MemberOfIncomingLink.ToCypher()}" +
                $"(s:{nameof(Contracts.Output.Subject)})";
            if (contextId.HasValue)
            {
                match =
                    $"(t:{nameof(Contracts.Output.Tenant)}){Constants.ChildOfIncomingDepthLink.ToCypher()}" +
                    $"(t0:{nameof(Contracts.Output.Tenant)}){Constants.BelongsIncomingLink.ToCypher()}" +
                    $"(g:{nameof(Group)}){Constants.AssignedLink.ToCypher()}" +
                    $"(r:{nameof(Contracts.Output.Role)}{{Id:\"{contextId}\"}})," +
                    $"(g){Constants.MemberOfIncomingLink.ToCypher()}(s:{nameof(Contracts.Output.Subject)})";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(whereNotTrafficker)
                .With("s, 0 as c")
                .ReturnDistinct((s, c) => new CypherPaginationResult<Contracts.Output.Subject>
                    {Node = s.As<Contracts.Output.Subject?>(), TotalCount = c.As<int>()})
                .OrderByDual("s", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(whereNotTrafficker)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants.Select(x => x.ToString())},
                    {"includeIds", resourceIds ?? new List<Guid>()}
                })
                .With("null as s, count(distinct s) as c")
                .ReturnDistinct((s, c) => new CypherPaginationResult<Contracts.Output.Subject>
                    {Node = s.As<Contracts.Output.Subject?>(), TotalCount = c.As<int>()});
            return (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        }
    }
}