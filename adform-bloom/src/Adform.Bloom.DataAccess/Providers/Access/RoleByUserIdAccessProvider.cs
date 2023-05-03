using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Extensions;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Extensions;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Transactions;
using Role = Adform.Bloom.Contracts.Output.Role;
using Subject = Adform.Bloom.Contracts.Output.Subject;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.DataAccess.Providers.Access
{
    public class RoleByUserIdAccessProvider : GraphRepository,
        IAccessProvider<Subject, QueryParamsTenantIds, Role>
    {
        public RoleByUserIdAccessProvider(ITransactionalGraphClient graphClient) : base(graphClient)
        {
        }

        public async Task<EntityPagination<Role>> EvaluateAccessAsync(ClaimsPrincipal subject, Subject context,
            int skip, int limit, QueryParamsTenantIds filter, CancellationToken cancellationToken = default)
        {
            const string roleVariable = "role";
            var tenantIds = filter.TenantIds ?? new List<Guid>();
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var search = filter.Search;
            var regex = $"(?i).*{search}.*";
            var where = "t.Id in {tenants}";
            if (subject.IsAdformAdmin() && !tenantIds.Any())
            {
                where = "true";
            }

            var andWhere = "true";
            if (search != null)
            {
                andWhere = $"(t.{nameof(Tenant.Name)} =~ $regex OR {roleVariable}.Name =~ $regex)";
            }

            var baseMatch =
                $"(t:{nameof(Tenant)}){Constants.OwnsLink.ToCypher()}({roleVariable}){Constants.AssignedIncomingLink.ToCypher()}(g:{nameof(Group)}){Constants.MemberOfIncomingLink}(s:{nameof(Subject)} {{Id:$subjectId}})";
            
            var andWhereRoleTypeIn = $"({roleVariable}:{Constants.Label.CUSTOM_ROLE} OR {roleVariable}:{Constants.Label.TEMPLATE_ROLE} OR {roleVariable}:{Constants.Label.TRANSITIONAL_ROLE})";
            var cypher = (await GraphClient).Cypher
                .Match(baseMatch)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(andWhereRoleTypeIn)
                .With($"{roleVariable.BuildRole("t")}, 0 as count")
                .ReturnDistinct((role, count) => new RolePaginationResult
                {
                    Node = role.As<RoleWithTenantModel?>(),
                    TotalCount = count.As<int>()
                })
                .RoleOrderBy(roleVariable, filter)
                .Skip(skip)
                .Limit(limit)
                .UnionAll()
                .Match(baseMatch)
                .Where(where)
                .AndWhere(andWhere)
                .AndWhere(andWhereRoleTypeIn)
                .WithParams(new Dictionary<string, object>
                {
                    {"tenants", tenants},
                    {"regex", regex},
                    {"subjectId", context.Id.ToString()}
                })
                .With($"null as {roleVariable}, count(DISTINCT {roleVariable}) as count")
                .ReturnDistinct((role, count) => new RolePaginationResult
                {
                    Node = role.As<RoleWithTenantModel?>(),
                    TotalCount = count.As<int>()
                });

            return (await cypher.ResultsAsync).ToEntityPagination(skip, limit);
        }

    }
}