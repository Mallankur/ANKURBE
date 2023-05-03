using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
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
using Neo4jClient.Transactions;
using Role = Adform.Bloom.Domain.Entities.Role;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Access
{
    public class RoleByBusinessAccountAccessProvider : GraphRepository,
        IAccessProvider<Contracts.Output.BusinessAccount, QueryParamsTenantIds, Contracts.Output.Role>
    {
        public RoleByBusinessAccountAccessProvider(ITransactionalGraphClient graphClient) : base(graphClient)
        {
        }

        public async Task<EntityPagination<Contracts.Output.Role>> EvaluateAccessAsync(ClaimsPrincipal subject,
            BusinessAccount context, int skip, int limit, QueryParamsTenantIds filter,
            CancellationToken cancellationToken = default)
        {
            const string roleVariable = "r";
            var tenants = subject.GetTenants(limitTo: new[] {context.Id});

            var match =
                $"(t:{nameof(Contracts.Output.Tenant)}){Constants.OwnsLink.ToCypher()}({roleVariable}:{nameof(Contracts.Output.Role)})";
            var search = filter?.Search;
            var regex = $"(?i).*{search}.*";
            var where = "t.Id in {tenants}";
            var whereNotTraffickerRoles = $"NOT {roleVariable}:{Constants.Label.TRAFFICKER_ROLE}";
            var andWhere = "true";
            if (search != null)
            {
                andWhere = $"({roleVariable}.Name =~ $regex)";
            }

            var cypher = (await GraphClient).Cypher
                .Match(match)
                .Where(where)
                .AndWhere(whereNotTraffickerRoles)
                .AndWhere(andWhere)
                .With($"{BuildRole(roleVariable)}, t, 0 as c")
                .ReturnDistinct((r, c) => new RolePaginationResult
                {
                    Node = r.As<Contracts.Output.Role>(),
                    TotalCount = c.As<int>(),
                })
                .OrderByDual(roleVariable, filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(match)
                .Where(where)
                .AndWhere(whereNotTraffickerRoles)
                .AndWhere(andWhere)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants},
                    {"regex", regex}
                })
                .With($"null as {roleVariable}, count(distinct {roleVariable}) as c")
                .ReturnDistinct((r, c) => new RolePaginationResult
                {
                    Node = r.As<Contracts.Output.Role?>(),
                    TotalCount = c.As<int>(),
                });

            return (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        }
        
        private static string BuildRole(string variable)
        {
            return $"{{ {nameof(RoleWithTenantModel.Id)}:{variable}.{nameof(RoleWithTenantModel.Id)}, " +
                   $"{nameof(RoleWithTenantModel.Name)}: {variable}.{nameof(RoleWithTenantModel.Name)}, {nameof(RoleWithTenantModel.Description)}:{variable}.{nameof(RoleWithTenantModel.Description)}, " +
                   $"{nameof(RoleWithTenantModel.Enabled)}:{variable}.{nameof(BaseNode.IsEnabled)}, {nameof(RoleWithTenantModel.TenantName)}:t.{nameof(Contracts.Output.Tenant.Name)}, " +
                   $"{nameof(RoleWithTenantModel.CreatedAt)}:{variable}.{nameof(RoleWithTenantModel.CreatedAt)}, {nameof(RoleWithTenantModel.UpdatedAt)}:{variable}.{nameof(RoleWithTenantModel.UpdatedAt)} }} as {variable}";
        }

    }
}