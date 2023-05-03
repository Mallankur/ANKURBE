using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Repository;
using Neo4jClient.Cypher;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Extensions;

public static class RoleProviderExtensions
{
    public static string BuildRole(this string variable, string tenantVariable = "t0")
    {
        var type =
            $"CASE WHEN \"{Constants.Label.CUSTOM_ROLE}\" in LABELS({variable}) THEN \"{RoleCategory.Custom}\"" +
            $"WHEN \"{Constants.Label.TRANSITIONAL_ROLE}\" IN LABELS({variable}) THEN \"{RoleCategory.Transitional}\"" +
            $"ELSE \"{RoleCategory.Template}\" END";
        return
            $"{{ {nameof(RoleWithTenantModel.Id)}:{variable}.{nameof(RoleWithTenantModel.Id)}, {nameof(RoleWithTenantModel.Type)}:({type}), " +
            $"{nameof(RoleWithTenantModel.Name)}: {variable}.{nameof(RoleWithTenantModel.Name)}, {nameof(RoleWithTenantModel.Description)}:{variable}.{nameof(RoleWithTenantModel.Description)}, " +
            $"{nameof(RoleWithTenantModel.Enabled)}:{variable}.{nameof(BaseNode.IsEnabled)}, {nameof(RoleWithTenantModel.TenantName)}:{tenantVariable}.{nameof(Tenant.Name)}, " +
            $"{nameof(RoleWithTenantModel.CreatedAt)}:{variable}.{nameof(RoleWithTenantModel.CreatedAt)}, {nameof(RoleWithTenantModel.UpdatedAt)}:{variable}.{nameof(RoleWithTenantModel.UpdatedAt)} }} as {variable}";
    }

    public static ICypherFluentQuery<T> RoleOrderBy<T>(this ICypherFluentQuery<T> query, string variable,
        SortingParams? queryParams)
    {
        var dualSort = new HashSet<string>();
        var primarySort =
            $"{variable}.{nameof(RoleWithTenantModel.Id)} {ToString(queryParams == null ? SortingOrder.Descending : queryParams.SortingOrder)}";
        if (queryParams is QueryParamsRoles)
        {
            var prioritizeTemplateRoles = (queryParams as QueryParamsRoles)?.PrioritizeTemplateRoles ?? false;
            if (prioritizeTemplateRoles)
                dualSort.Add($"{variable}.Type {ToString(SortingOrder.Descending)}");
        }

        if (queryParams?.OrderBy != null)
            dualSort.Add($"TOLOWER(TOSTRING({variable}.{queryParams.OrderBy})) {ToString(queryParams.SortingOrder)}");
        dualSort.Add(primarySort);

        return query.OrderBy(dualSort.ToArray());
    }

    private static string ToString(SortingOrder sortingOrder)
    {
        return sortingOrder == SortingOrder.Ascending ? "asc" : "desc";
    }
}