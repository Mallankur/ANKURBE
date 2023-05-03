using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using HotChocolate.Resolvers;

namespace Adform.Bloom.Api.Graph.Role
{
    public static class RolesResolver
    {
        public static (PaginationInput pagination, QueryParamsRolesInput? queryParamsInput) ResolveRolesQueryParameters(
            this IResolverContext context)
        {
            var prioritizeTemplateRoles = context.ArgumentValue<bool?>(Constants.Parameters.PrioritizeTemplateRoles);
            var (pagination, queryParams) = context.ResolveQueryParameters<QueryParamsRolesInput>();

            if (prioritizeTemplateRoles == null) return (pagination, queryParams);

            queryParams ??= new QueryParamsRolesInput();
            queryParams.PrioritizeTemplateRoles = prioritizeTemplateRoles;

            return (pagination, queryParams);
        }
    }
}