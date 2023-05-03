using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using LicensedFeature = Adform.Bloom.Domain.Entities.LicensedFeature;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Access;

public class BusinessAccountByFeatureAccessProvider : GraphRepository,
    IAccessProvider<Feature, QueryParams, BusinessAccount>
{
    private readonly IBusinessAccountReadModelProvider _readModelProvider;

    public BusinessAccountByFeatureAccessProvider(ITransactionalGraphClient graphClient,
        IBusinessAccountReadModelProvider readModelProvider) : base(graphClient)
    {
        _readModelProvider = readModelProvider;
    }

    public async Task<EntityPagination<BusinessAccount>> EvaluateAccessAsync(ClaimsPrincipal subject, Feature context,
        int skip, int limit, QueryParams filter,
        CancellationToken cancellationToken = default)
    {
        var tenantIds = filter.ResourceIds ?? new List<Guid>();
        var tenants = subject.GetTenants(limitTo: tenantIds);
        var isAdmin = subject.IsAdformAdmin();
        if (filter.ContextId.HasValue)
            throw new NotImplementedException(
                $"'{nameof(filter.ContextId)}' parameter is set but this feature has not been implemented yet in '{nameof(BusinessAccountByFeatureAccessProvider)}'");

        var match =
            $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}(f:{nameof(Feature)})";
        var where = "t:Tenant";
        if (!isAdmin || tenantIds.Any()) where = "t.Id in {tenants}";

        const string whereContext = $"f.Id = ${nameof(context)}";

        var cypher = (await GraphClient).Cypher
            .Match(match)
            .Where(where)
            .AndWhere(whereContext)
            .With("t, 0 as c")
            .Return((t, c) => new TenantPaginationResult
                {Node = t.As<Tenant>(), TotalCount = c.As<int>()})
            .OrderByDual("t", filter)
            .Skip(0)
            .Limit(int.MaxValue)
            .UnionAll()
            .Match(match)
            .Where(where)
            .AndWhere(whereContext)
            .WithParam("tenants", tenants.Select(x => x.ToString()))
            .WithParam(nameof(context), context.Id)
            .With("null as t, count(distinct t) as c")
            .Return((t, c) => new TenantPaginationResult
                {Node = t.As<Tenant?>(), TotalCount = c.As<int>()});

        var ids = (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        if (!(filter?.Search?.Length >= 3) && filter != null) filter.Search = null;

        if (ids.Data.Count < 1 || limit == 0)
            return new EntityPagination<BusinessAccount>(ids.Offset, ids.Limit, ids.TotalItems,
                new List<BusinessAccount>(0));

        return await _readModelProvider.SearchForResourcesAsync(
            skip,
            limit,
            filter ?? new QueryParams(),
            ids.Data.Select(x => x.Id), null,
            cancellationToken);
    }
}