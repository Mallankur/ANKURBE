using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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
using Feature = Adform.Bloom.Contracts.Output.Feature;
using Permission = Adform.Bloom.Contracts.Output.Permission;
using Role = Adform.Bloom.Contracts.Output.Role;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Access;

public class FeatureByRoleAccessProvider : GraphRepository,
    IAccessProvider<Role, QueryParamsTenantIds, Feature>
{
    public FeatureByRoleAccessProvider(ITransactionalGraphClient graphClient) : base(graphClient)
    {
    }

    public async Task<EntityPagination<Feature>> EvaluateAccessAsync(ClaimsPrincipal subject, Role context, int skip, int limit, QueryParamsTenantIds filter,
        CancellationToken cancellationToken = default)
    {
        const string tenantVariable = "tenant";
        const string tenantVariable2 = "tenant2";
        const string roleVariable = "role";
        const string featureVariable = "feature";
        var filterTenants = filter.TenantIds ?? new List<Guid>();
        var accessTenants = subject.GetTenants().ToList();

        var whereHasAccess = subject.IsAdformAdmin() ? "true" : $"{tenantVariable}.Id in {{{nameof(accessTenants)}}} AND {tenantVariable2}.Id in {{{nameof(accessTenants)}}}";
        var andWhereFiltered = filterTenants.Any() ? $"{tenantVariable2}.Id in {{{nameof(filterTenants)}}}" : "true";

        var baseMatch =
            $"({tenantVariable}:{nameof(Tenant)}){Constants.OwnsLink.ToCypher()}({roleVariable}:{nameof(Role)}){Constants.ContainsLink.ToCypher()}(:{nameof(Permission)}){Constants.ContainsIncomingLink.ToCypher()}({featureVariable}:{nameof(Feature)})," +
            $"({tenantVariable2}:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}(:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}({featureVariable})";

        var andWhereRoleIdEq = $"{roleVariable}.Id = {{roleId}}";
        var cypher = (await GraphClient).Cypher
            .Match(baseMatch)
            .Where(whereHasAccess)
            .AndWhere(andWhereFiltered)
            .AndWhere(andWhereRoleIdEq)
            .With($"{featureVariable}, 0 as count")
            .ReturnDistinct((feature, count) => new
            {
                Nodes = feature.As<Feature?>(),
                TotalCount = count.As<int>()
            })
            .OrderByDual(featureVariable, filter)
            .Skip(skip)
            .Limit(limit)
            .UnionAll()
            .Match(baseMatch)
            .Where(whereHasAccess)
            .AndWhere(andWhereFiltered)
            .AndWhere(andWhereRoleIdEq)
            .WithParams(new Dictionary<string, object>
            {
                    {nameof(filterTenants), filterTenants},
                    {nameof(accessTenants), accessTenants},
                    {"roleId", context.Id},
            })
            .With($"null as {featureVariable}, count(DISTINCT {featureVariable}) as count")
            .ReturnDistinct((feature, count) => new 
            {
                Nodes = feature.As<Feature?>(),
                TotalCount = count.As<int>()
            });

        var result = (await cypher.ResultsAsync).ToArray();

        if (result.Length == 0)
            return new EntityPagination<Feature>(skip, limit, 0,
                new List<Feature>(0));

        return new EntityPagination<Feature>(skip, limit,
            result[^1].TotalCount,
            result[..^1].Select(r => r.Nodes!).ToList());
    }
}