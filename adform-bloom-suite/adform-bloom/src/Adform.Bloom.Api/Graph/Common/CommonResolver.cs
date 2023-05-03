using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Ciam.OngDb.Repository;
using HotChocolate;
using HotChocolate.Resolvers;

namespace Adform.Bloom.Api.Graph.Common
{
    public static class CommonResolver
    {
        public static ClaimsPrincipal ResolveUser(this IResolverContext context)
        {
            return context.GetUser() ?? new ClaimsPrincipal();
        }

        public static (PaginationInput pagination, T filterInput)
            ResolveQueryParameters<T>(this IResolverContext context, IReadOnlyCollection<Guid>? resourceIds = null)
            where T : class
        {
            var pagination = context.ArgumentValue<PaginationInput>(Constants.Parameters.Pagination);
            var search = context.ArgumentValue<string?>(Constants.Parameters.Search);
            var sortInput = context.ArgumentValue<SortingParamsInput?>(Constants.Parameters.SortBy);
            var orderBy = "Id";
            var sortOrder = (int) SortingOrder.Ascending;

            if (sortInput!=null)
            {
                orderBy = sortInput?.FieldName?.ToPascalCase().ToDomain();
                sortOrder = sortInput?.Order ?? (int) SortingOrder.Ascending;
            }
            var type = typeof(T);
            var filter = type switch
            {
                Type _ when type == typeof(QueryParamsBusinessAccountInput) =>
                    ResolveBusinessAccountInputToQueryParameters(context, search,
                        orderBy, sortOrder, resourceIds),
                Type _ when type == typeof(QueryParamsRolesInput) =>
                    ResolveRoleInputToQueryParameters(context, search,
                        orderBy, sortOrder, resourceIds),
                Type _ when type == typeof(QueryParamsTenantIdsInput) => ResolveTenantIdsInputToQueryParameters(context,
                    search,
                    orderBy, sortOrder, resourceIds),
                Type _ when type == typeof(QueryParamsTenantIdsAndPolicyTypesInput) => ResolveTenantIdsAndPolicyNamesInputToQueryParameters(context,
                    search, orderBy, sortOrder, resourceIds),
                _ => ResolveToQueryParameters(context, search, orderBy, sortOrder, resourceIds)
            };

            return (pagination, filterInput: (T) Convert.ChangeType(filter, type));
        }

        private static QueryParamsInput ResolveToQueryParameters(
            IResolverContext context, string? search,
            string? orderBy, int sortingOrder, IReadOnlyCollection<Guid>? resourceIds)
        {
            return new QueryParamsInput
            {
                Search = search,
                FieldName = orderBy,
                Order = sortingOrder,
                ResourceIds = resourceIds
            };
        }

        private static QueryParamsTenantIdsInput ResolveTenantIdsInputToQueryParameters(
            IResolverContext context, string? search,
            string? orderBy, int sortingOrder, IReadOnlyCollection<Guid>? resourceIds)
        {
            var tenantIds = new List<Guid>();
            if (context.Field.Arguments.ContainsField(Constants.Parameters.BusinessAccountIds))
                tenantIds = (List<Guid>) context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters
                    .BusinessAccountIds);
            return new QueryParamsTenantIdsInput
            {
                Search = search,
                FieldName = orderBy,
                Order = sortingOrder,
                TenantIds = tenantIds,
                ResourceIds = resourceIds
            };
        }
        
        private static QueryParamsTenantIdsAndPolicyTypesInput ResolveTenantIdsAndPolicyNamesInputToQueryParameters(
            IResolverContext context, string? search, 
            string? orderBy, int sortingOrder, IReadOnlyCollection<Guid>? resourceIds)
        {
            var policyTypes = new List<string>();
            if (context.Field.Arguments.ContainsField(Constants.Parameters.ProductNames))
            {               
                var types = (List<PolicyTypeInput>)context.ArgumentValue<IReadOnlyCollection<PolicyTypeInput>>(Constants.Parameters.ProductNames);
                if(types != null && types.Any())
                    policyTypes = types.Select(x => x.ToString()).ToList();
            }
            var filter = ResolveTenantIdsInputToQueryParameters(context, search, orderBy, sortingOrder, resourceIds);
            return new QueryParamsTenantIdsAndPolicyTypesInput
            {
                Search = search,
                FieldName = orderBy,
                Order = sortingOrder,
                TenantIds = filter.TenantIds,
                PolicyTypes = policyTypes,
                ResourceIds = resourceIds
            };
        }

        private static QueryParamsRolesInput ResolveRoleInputToQueryParameters(
            IResolverContext context, string? search,
            string? orderBy, int sortingOrder, IReadOnlyCollection<Guid>? resourceIds)
        {
            var prioritizeTemplateRoles = false;
            if (context.Field.Arguments.ContainsField(Constants.Parameters.PrioritizeTemplateRoles))
                prioritizeTemplateRoles = context.ArgumentValue<bool>(
                    Constants.Parameters
                        .PrioritizeTemplateRoles);
            return new QueryParamsRolesInput
            {
                Search = search,
                FieldName = orderBy,
                Order = sortingOrder,
                PrioritizeTemplateRoles = prioritizeTemplateRoles,
                ResourceIds = resourceIds
            };
        }

        private static QueryParamsBusinessAccountInput ResolveBusinessAccountInputToQueryParameters(
            IResolverContext context, string? search,
            string? orderBy, int sortingOrder, IReadOnlyCollection<Guid>? resourceIds)
        {
            BusinessAccountType? tenantType = null;
            if (context.Field.Arguments.ContainsField(Constants.Parameters.BusinessAccountType))
                tenantType = context.ArgumentValue<BusinessAccountType?>(
                    Constants.Parameters.BusinessAccountType);

            var filter = ResolveTenantIdsInputToQueryParameters(context, search, orderBy, sortingOrder, resourceIds);
            return new QueryParamsBusinessAccountInput
            {
                Search = search,
                FieldName = orderBy,
                Order = sortingOrder,
                BusinessAccountType = (int?) tenantType,
                TenantIds = filter.TenantIds,
                ResourceIds = resourceIds
            };
        }
    }
}