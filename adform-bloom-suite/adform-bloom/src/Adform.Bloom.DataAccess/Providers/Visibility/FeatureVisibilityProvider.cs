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
using Adform.Ciam.SharedKernel.Extensions;
using Neo4jClient.Extensions;
using Neo4jClient.Transactions;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class FeatureVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature>
    {
        public FeatureVisibilityProvider(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public async Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter,
            string? label = null)
        {
            const string whereIsEnabled = "f.IsEnabled";
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();
            var subjectId = subject.GetActorId()!;

            var match =
                $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}" +
                $"(t0:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                $"(f:{nameof(Feature)})";
            var where = "t:Tenant";
            if (!isAdmin)
            {
                match = $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                        $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                        $"(f:{nameof(Feature)})";
            }
            
            if (tenants.Any())
            {
                where = "t.Id in {tenants}";
            }
            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(whereIsEnabled)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants.Select(x => x.ToString())}
                })
                .AndWhere((Feature f) => f.Id.In(resourceIds))
                .ReturnDistinct(f => f.As<Feature>().Id);
            return await cypher.ResultsAsync;
        }

        public async Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsTenantIds filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds;
            return (await GetVisibleResourcesAsync(subject, filter, label)).Count() == resourceIds?.Count;
        }

        public async Task<EntityPagination<Contracts.Output.Feature>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsTenantIds filter, int skip, int limit)
        {
            const string whereIsEnabled = "f.IsEnabled";
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();
            var subjectId = subject.GetActorId()!;

            var match =
                $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}" +
                $"(t0:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                $"(f:{nameof(Feature)})";
            var where = "t:Tenant";
            if (!isAdmin)
            {
                match = $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                        $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                        $"(f:{nameof(Feature)})";
                if (tenants.Any())
                {
                    where = "t.Id in {tenants}";
                }
            }
            else
            {
                if (tenants.Any())
                {
                    where = "t.Id in {tenants}";
                }
            }

            var whereContext = "";
            if (contextId.HasValue)
            {
                whereContext = 
                    @$"lf.Id = ""{contextId.Value}""";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(whereIsEnabled)
                .AndWhereIf(!string.IsNullOrEmpty(whereContext), whereContext)
                .With("f, 0 as c")
                .ReturnDistinct((f, c) => new
                {
                    Nodes = f.As<Contracts.Output.Feature>(), TotalCount = c.As<int>()
                })
                .OrderByDual("f", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .Where(where)
                .AndWhere(whereIsEnabled)
                .AndWhereIf(!string.IsNullOrEmpty(whereContext), whereContext)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants.Select(x => x.ToString())}
                })
                .With("null as f, count(distinct f) as c")
                .ReturnDistinct((f, c) => new
                {
                    Nodes = f.As<Contracts.Output.Feature?>(), TotalCount = c.As<int>()
                });
            var result = (await cypher.ResultsAsync).ToArray();

            if (result.Length == 0)
                return new EntityPagination<Contracts.Output.Feature>(skip, limit, 0,
                    new List<Contracts.Output.Feature>(0));

            return new EntityPagination<Contracts.Output.Feature>(skip, limit,
                result[^1].TotalCount,
                result[..^1].Select(r => r.Nodes!).ToList());
        }
    }
}