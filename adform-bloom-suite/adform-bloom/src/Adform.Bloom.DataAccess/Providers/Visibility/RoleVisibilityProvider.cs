using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Extensions;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Extensions;
using Neo4jClient.Transactions;
using Role = Adform.Bloom.Domain.Entities.Role;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Visibility
{
    public class RoleVisibilityProvider : GraphRepository, IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role>
    {
        static readonly string ExcludeInheritanceFromAdformTenant =
            $"(NOT t0:{Constants.Label.ADFORM} OR t0.Id = t.Id OR NOT role:{Constants.Label.CUSTOM_ROLE})";

        private const string RoleVariable = "role";

        public RoleVisibilityProvider(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public async Task<bool> HasVisibilityAsync(ClaimsPrincipal subject, QueryParamsRoles filter,
            string? label = null)
        {
            var resourceIds = filter.ResourceIds;
            return (await GetVisibleResourcesAsync(subject, filter, label)).Count() ==
                   resourceIds?.Count;
        }

        public async Task<IEnumerable<Guid>> GetVisibleResourcesAsync(ClaimsPrincipal subject,
            QueryParamsRoles filter, string? label = null)
        {
            var tenantIds = filter.TenantIds;
            var resourceIds = filter.ResourceIds;
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in {tenants}";
            var whereLabel = $"{RoleVariable}:{label ?? nameof(Role)}";            
            var andWhereRoleTypeIn = $"({RoleVariable}:{Constants.Label.CUSTOM_ROLE} OR {RoleVariable}:{Constants.Label.TEMPLATE_ROLE})";
            var match = $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}" +
                        $"(t0:{nameof(Tenant)}){Constants.OwnsLink.ToCypher()}" +
                        $"({RoleVariable}:{nameof(Role)})";
            
            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(whereLabel)
                .AndWhere(ExcludeInheritanceFromAdformTenant)
                .AndWhere(andWhereRoleTypeIn)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .AndWhere((Role role) => role.Id.In(resourceIds))
                .ReturnDistinct(role => role.As<Role>().Id);
            return (List<Guid>) await cypher.ResultsAsync;
        }

        public async Task<EntityPagination<Contracts.Output.Role>> EvaluateVisibilityAsync(ClaimsPrincipal subject,
            QueryParamsRoles filter, int skip, int limit)
        {
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var contextId = filter.ContextId;
            if (contextId.HasValue)
                throw new NotImplementedException(
                    $"'{nameof(contextId)}' parameter is set but this feature has not been implemented yet in " +
                    $"'{nameof(RoleVisibilityProvider)}'");

            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var baseMatch =
                $"(t:{nameof(Tenant)}){Constants.ChildOfDepthLink.ToCypher()}" +
                $"(t0:{nameof(Tenant)}){Constants.OwnsLink.ToCypher()}({RoleVariable}:{nameof(Role)})";

            var search = filter?.Search;
            var regex = $"(?i).*{search}.*";

            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in $tenants";

            var andWhere = "true";
            if (search != null)
                andWhere =
                    $"(t.{nameof(Tenant.Name)} =~ $regex AND t0.{nameof(Tenant.Name)} =~ $regex OR {RoleVariable}.{nameof(Role.Name)} =~ $regex)";

            var andAndWhere = "true";
            if (tenantIds.Any())
                andAndWhere = "t0.Id in $tenants";
            var andWhereRoleTypeIn = $"({RoleVariable}:{Constants.Label.CUSTOM_ROLE} OR {RoleVariable}:{Constants.Label.TEMPLATE_ROLE})";
            var cypher = (await GraphClient).Cypher
                .Match(baseMatch)
                .Where(where)
                .AndWhere(ExcludeInheritanceFromAdformTenant)
                .AndWhere(andWhere)
                .AndWhere(andAndWhere)
                .AndWhere(andWhereRoleTypeIn)
                .With($"{RoleVariable.BuildRole()}, 0 as count")
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants},
                    {"regex", regex}
                })
                .ReturnDistinct((role, count) => new RolePaginationResult
                {
                    Node = role.As<RoleWithTenantModel?>(),
                    TotalCount = count.As<int>()
                })
                .RoleOrderBy(RoleVariable, filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(baseMatch)
                .Where(where)
                .AndWhere(ExcludeInheritanceFromAdformTenant)
                .AndWhere(andWhere)
                .AndWhere(andAndWhere)
                .AndWhere(andWhereRoleTypeIn)
                .With($"null as {RoleVariable}, count(DISTINCT {RoleVariable}) as count")
                .ReturnDistinct((role, count) => new RolePaginationResult
                {
                    Node = role.As<RoleWithTenantModel?>(),
                    TotalCount = count.As<int>()
                });
            return (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        }
    }
}