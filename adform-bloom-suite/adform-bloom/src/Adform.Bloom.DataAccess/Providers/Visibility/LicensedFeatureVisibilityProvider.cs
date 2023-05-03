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
using Neo4jClient.Transactions;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using LicensedFeature = Adform.Bloom.Contracts.Output.LicensedFeature;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class LicensedFeatureVisibilityProvider : GraphRepository,
        IVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, LicensedFeature>
    {
        public LicensedFeatureVisibilityProvider(ITransactionalGraphClient graphClient) : base(
            graphClient)
        {
        }

        public async Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsTenantIdsAndPolicyTypes filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds;
            return (await GetVisibleResourcesAsync(subject, filter, label)).Count() ==
                   resourceIds?.Count;
        }

        public async Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsTenantIdsAndPolicyTypes filter, string? label = null)
        {
            const string whereIsEnabled = "lf.IsEnabled";
            var tenantIds = filter.TenantIds?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var policies = filter.PolicyTypes ?? new List<string>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();
            var subjectId = subject.GetActorId()!;
            var match =
                $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                $"(f: {nameof(Feature)})";
            var where = "t:Tenant";
            var with = "lf, t";
            if (!isAdmin)
            {
                match = $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                        $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                        $"(f: {nameof(Feature)})";
            }
            match += $",(pol:{nameof(Policy)}){Constants.ChildOfDepthLink.ToCypher()}" +
                     $"(p0:{nameof(Policy)}){Constants.ContainsLink.ToCypher()}" +
                     $"(r:{nameof(Role)}:{Constants.Label.SYSTEM}){Constants.ContainsLink.ToCypher()}" +
                     $"(per:{nameof(Permission)}){Constants.ContainsIncomingLink.ToCypher()}" +
                     $"(f)";
            
            if (tenants.Any())
            {
                where = "t.Id in {tenants}";
            }
            
            if (policies.Any())
            {
                where += " AND ANY(x in {policies} WHERE x IN LABELS(pol))";
                with += ", LABELS(pol) AS labels";
            }
            
            if (resourceIds.Any())
            {
                where += " AND lf.Id in {licensedFeatures}";
            }
            

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .With(with)
                .Where(where)
                .AndWhere(whereIsEnabled)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants.Select(x => x.ToString())},
                    {"policies", policies.Select(x => x.ToString())},
                    {"licensedFeatures", resourceIds.Select(x => x.ToString())}
                })
                .ReturnDistinct(lf => lf.As<LicensedFeature>().Id);
            return await cypher.ResultsAsync;
        }

        public async Task<EntityPagination<LicensedFeature>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsTenantIdsAndPolicyTypes filter, int skip, int limit)
        {
            const string whereIsEnabled = "lf.IsEnabled";
            var tenantIds = filter.TenantIds?? new List<Guid>();
            var resourceIds = filter.ResourceIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            if (contextId.HasValue)
                throw new NotImplementedException(
                    $"'{nameof(contextId)}' parameter is set but this feature has not been implemented yet in '{nameof(LicensedFeatureVisibilityProvider)}'");

            var tenants = subject.GetTenants(limitTo: tenantIds);
            var policies = filter.PolicyTypes ?? new List<string>();
            var isAdmin = subject.IsAdformAdmin();
            var match =
                $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                $"(lf: {nameof(LicensedFeature)})" +
                $"{Constants.ContainsLink.ToCypher()}(f:{nameof(Feature)})";
            var where = "t:Tenant";
            var with = "lf, t";
            if (!isAdmin)
            {
                match =
                    $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                    $"(lf: {nameof(LicensedFeature)})" +
                    $"{Constants.ContainsLink.ToCypher()}(f:{nameof(Feature)})";
            }
            
            match += $",(pol:{nameof(Policy)}){Constants.ChildOfDepthLink.ToCypher()}" +
                     $"(p0:{nameof(Policy)}){Constants.ContainsLink.ToCypher()}" +
                     $"(r:{nameof(Role)}:{Constants.Label.SYSTEM}){Constants.ContainsLink.ToCypher()}" +
                     $"(per:{nameof(Permission)}){Constants.ContainsIncomingLink.ToCypher()}" +
                     $"(f)";
            
            if (tenants.Any())
            {
                where = "t.Id in {tenants}";
            }

            if (policies.Any())
            {
                where += " AND ANY(x in {policies} WHERE x IN LABELS(pol))";
                with += ", LABELS(pol) AS labels";
            }

            if (resourceIds.Any())
            {
                where += " AND lf.Id in {licensedFeatures}";
            }

            var andWhere = "true";
            var search = filter?.Search;
            var regex = $"(?i).*{search}.*";
            if (search != null)
            {
                andWhere = $"lf.Name =~ $regex";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .With(with)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(whereIsEnabled)
                .With("lf, 0 as c")
                .ReturnDistinct((lf, c) => new {Nodes = lf.As<LicensedFeature>(), TotalCount = c.As<int>()})
                .OrderByDual("lf", filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .With(with)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(whereIsEnabled)
                .WithParams(new Dictionary<string, object>
                {
                    {"licensedFeatures", resourceIds.Select(x => x.ToString())},
                    {"policies", policies.Select(x => x.ToString())},
                    {"tenants", tenants.Select(x => x.ToString())},
                    {"regex", regex}
                })
                .With("null as lf, count(distinct lf) as c")
                .ReturnDistinct((lf, c) => new {Nodes = lf.As<LicensedFeature?>(), TotalCount = c.As<int>()});
            var result = (await cypher.ResultsAsync).ToArray();

            if (result.Length == 0)
                return new EntityPagination<LicensedFeature>(skip, limit, 0,
                    new List<LicensedFeature>(0));

            return new EntityPagination<LicensedFeature>(skip, limit,
                result[^1].TotalCount,
                result[..^1].Select(r => r.Nodes!).ToList());
        }
    }
}