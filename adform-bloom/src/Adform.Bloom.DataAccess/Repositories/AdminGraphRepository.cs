using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.OngDb.Core.Extensions;
using Adform.Ciam.OngDb.Core.Interfaces;
using Adform.Ciam.OngDb.Repository;
using Adform.Ciam.SharedKernel.Entities;
using Neo4jClient.Cypher;
using Neo4jClient.Extensions;
using Neo4jClient.Transactions;

namespace Adform.Bloom.DataAccess.Repositories
{
    public class AdminGraphRepository : GraphRepository, IAdminGraphRepository
    {
        public AdminGraphRepository(ITransactionalGraphClient graphClient)
            : base(graphClient)
        {
        }

        public async Task<IReadOnlyCollection<TEntity>> GetByIdsAsync<TEntity>(IReadOnlyCollection<Guid> ids)
        {
            var expressionVariable = typeof(TEntity).Name.ToLowerInvariant();
            var query = (await GraphClient).Cypher
                .Match(
                    $"({expressionVariable}:{typeof(TEntity).Name})")
                .Where($"{expressionVariable}.Id in {{ids}}")
                .WithParams(new Dictionary<string, object>
                {
                    {"ids", ids.Select(x => x.ToString())}
                })
                .Return<TEntity>(expressionVariable);
            return (List<TEntity>)await query.ResultsAsync;
        }

        public async Task<EntityPagination<TEntity>> SearchPaginationAsync<TEntity>(
            Expression<Func<TEntity, bool>> expression, int skip = 0, int limit = 100, string? label = default,
            string? notLabel = default, QueryParams? queryParams = null) where TEntity : class
        {
            var expressionVariable = expression.Parameters[0].Name;
            Func<ICypherFluentQuery, ICypherFluentQuery>? func = null;
            if (!string.IsNullOrEmpty(queryParams?.Search))
            {
                func = x => x
                    .AndWhere($"{expressionVariable}.{nameof(NamedNode.Name)} =~ {{regex}}")
                    .WithParam("regex", $"(?i).*{queryParams.Search}.*");
            }

            var totalDocuments = await GetCountAsync(expression, label, notLabel, func);
            var result = await GetNodesAsync(expression, skip, limit, label, notLabel, queryParams, func);

            return new EntityPagination<TEntity>(skip, limit, (int)totalDocuments, result);
        }

        public async Task<IReadOnlyList<TChild>> GetConnectedWithIntermediateAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild)
        {
            var parentExpressionVariable = startNodeExpression.Parameters[0];
            var childExpressionVariable = typeof(TChild).Name;
            var query = (await GraphClient).Cypher
                .Match(
                    $"({parentExpressionVariable}:{typeof(TParent).Name}){linkParentToIntermediate.ToCypher()}(:{typeof(TIntermediate).Name}){linkIntermediateToChild.ToCypher()}({childExpressionVariable.ToLowerInvariant()}:{childExpressionVariable})")
                .Where(startNodeExpression)
                .Return<TChild>(childExpressionVariable.ToLowerInvariant());
            return (List<TChild>)await query.ResultsAsync;
        }

        public async Task<bool> HasRelationshipAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            Expression<Func<TChild, bool>> targetNodeExpression,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild)
        {
            var parentExpressionVariable = startNodeExpression.Parameters[0];
            var childExpressionVariable = targetNodeExpression.Parameters[0];
            var query = (await GraphClient).Cypher
                .Match(
                    $"({parentExpressionVariable}:{typeof(TParent).Name}){linkParentToIntermediate.ToCypher()}(:{typeof(TIntermediate).Name}){linkIntermediateToChild.ToCypher()}({childExpressionVariable}:{typeof(TChild).Name})")
                .Where(startNodeExpression)
                .AndWhere(targetNodeExpression)
                .With($"'{linkParentToIntermediate.Type}' in collect(type(r)) as res")
                .Return(res => res.As<bool>());

            return (await query.ResultsAsync).FirstOrDefault();
        }

        public async Task BulkLazyCreateGroupAsync(Guid subjectId, IEnumerable<RoleTenant> assignments)
        {
            var assignmentsWithGroups = assignments.Select(x => new SubjectToRoleAssignment
            {
                RoleId = x.RoleId,
                TenantId = x.TenantId,
                SubjectId = subjectId,
                Group = GenerateGroup(x.RoleId, x.TenantId)
            });

            var query = (await GraphClient).Cypher
                .With("{a} as asses")
                .WithParam("a", assignmentsWithGroups)
                .Unwind("asses", "ass")
                .Match(
                    $"(r:{nameof(Role)}{{Id:ass.{nameof(SubjectToRoleAssignment.RoleId)}}})," +
                    $"(t:{nameof(Tenant)}{{Id:ass.{nameof(SubjectToRoleAssignment.TenantId)}}})," +
                    $"(s:{nameof(Subject)}{{Id:ass.{nameof(SubjectToRoleAssignment.SubjectId)}}})"
              )
                .Merge($"(t){Constants.BelongsIncomingLink.ToCypher()}(g:{nameof(Group)}){Constants.AssignedLink.ToCypher()}(r)")
                .OnCreate()
                .Set(
                $"g={{{nameof(Group.Id)}:ass.{nameof(SubjectToRoleAssignment.Group)}.{nameof(Group.Id)}," +
                $"{nameof(Group.Name)}:ass.{nameof(SubjectToRoleAssignment.Group)}.{nameof(Group.Name)}," +
                $"{nameof(Group.CreatedAt)}:{{date}}," +
                $"{nameof(Group.UpdatedAt)}:{{date}}}}"
                )
                .WithParam("date", DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .Merge($"(g){Constants.MemberOfIncomingLink.ToCypher()}(s)");


            await query.ExecuteWithoutResultsAsync();
        }

        public async Task DeleteLinkWithIntermediateAsync<TParent, TIntermediate, TChild>(
            Expression<Func<TParent, bool>> startNodeExpression,
            Expression<Func<TChild, bool>> targetNodeExpression,
            ILink linkParentToIntermediate,
            ILink linkIntermediateToChild)
        {
            var parentExpressionVariable = startNodeExpression.Parameters[0];
            var childExpressionVariable = targetNodeExpression.Parameters[0];
            var query = new[]
            {
                $"(g:{typeof(TIntermediate).Name}){linkIntermediateToChild.ToCypher()}({childExpressionVariable.Name}:{typeof(TChild).Name})",
                $"({parentExpressionVariable.Name}:{typeof(TParent).Name}){linkParentToIntermediate.ToCypher()}(g)"
            };
            var cypher = (await GraphClient).Cypher
                .Match(query)
                .Where(startNodeExpression)
                .AndWhere(targetNodeExpression)
                .Delete("r");

            await cypher.ExecuteWithoutResultsAsync();
        }

        public async Task DeletePermissionAssignmentsForDeassignedFeatureAsync(Guid featureId, Guid tenantId)
        {
            var query = (await GraphClient).Cypher
                .Match(
                    $"(ff:{nameof(Feature)}" +
                    $"{{Id:\"{featureId}\"}})" +
                    $"{Constants.AssignedWithVariableIncomingLink.ToCypher()}" +
                    $"(tenant:{nameof(Tenant)}{{Id:\"{tenantId}\"}})" +
                    $"{Constants.OwnsLink.ToCypher()}" +
                    $"(role:{nameof(Role)})" +
                    $"{Constants.ContainsVariableLink.ToCypher()}" +
                    $"(rp:{nameof(Permission)})")
                .Where(
                    $"(rp)<--(ff) and size((rp)<--(:{nameof(Feature)})<--(:{nameof(Tenant)}{{Id:\"{tenantId}\"}})) < 2")
                .Delete("assigned,r");
            await query.ExecuteWithoutResultsAsync();
        }

        public async Task DeletePermissionAssignmentsFromRolesForDeassignedPermissionFromFeatureAsync(
            Guid permissionId)
        {
            var query = (await GraphClient).Cypher
                .Match(
                    $"(per:{nameof(Permission)}" +
                    $"{{Id:\"{permissionId}\"}})" +
                    $"{Constants.ContainsVariableIncomingLink.ToCypher()}" +
                    $"(role:{nameof(Role)})" +
                    $"{Constants.OwnsIncomingLink.ToCypher()}" +
                    $"(t:{nameof(Tenant)})")
                .Where($"NOT (t)-[]->(:{nameof(Feature)})-->(per)")
                .Delete("r");
            await query.ExecuteWithoutResultsAsync();
        }

        public async Task<IReadOnlyList<Guid>> AssignPermissionsToRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds)
        {
            const string roleParam = "r";
            const string permissionParam = "p";
            const string featureParam = "f";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({roleParam}:{nameof(Role)}{{Id:\"{roleId}\"}}), ({featureParam}:{nameof(Feature)}){Constants.ContainsLink.ToCypher()}({permissionParam}:{nameof(Permission)})")
                .Where($"{featureParam}.Id in {{features}}")
                .WithParam("features", featureIds)
                .Merge($"({roleParam}){Constants.ContainsLink.ToCypher()}({permissionParam})")
                .Return<Guid>("p.Id");

            return (List<Guid>)await query.ResultsAsync;
        }

        public async Task UnassignPermissionsFromRoleThroughFeaturesAsync(Guid roleId,
            IReadOnlyCollection<Guid> featureIds)
        {
            const string roleParam = "r";
            const string featureParam = "f";
            const string relationshipParam = "rel";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({roleParam}:{nameof(Role)}{{{nameof(Role.Id)}:\"{roleId}\"}})" +
                    $"-[{relationshipParam}:CONTAINS]->(:Permission)<-[:CONTAINS]-({featureParam}:Feature)")
                .Where($"{featureParam}.{nameof(Feature.Id)} in {{features}}")
                .WithParam("features", featureIds)
                .Delete(relationshipParam);

            await query.ExecuteWithoutResultsAsync();
        }

        public async Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughFeatureAssignmentAsync(Guid featureId,
            IReadOnlyCollection<Guid> permissionIds)
        {
            const string roleParam = "r";
            const string permissionParam = "p0";
            const string permissionExistParam = "p";
            const string featureParam = "f";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({roleParam}:{nameof(Role)}){Constants.ContainsLink.ToCypher()}({permissionExistParam}:{nameof(Permission)})" +
                    $"{Constants.ContainsIncomingLink.ToCypher()}({featureParam}:{nameof(Feature)}{{Id:\"{featureId}\"}})" +
                    $"{Constants.ContainsLink.ToCypher()}({permissionParam}:{nameof(Permission)})")
                .Where(
                    $"{permissionParam}.Id IN {{permissions}} AND NOT ({permissionExistParam}.Id IN {{permissions}})")
                .WithParam("permissions", permissionIds)
                .Merge($"({roleParam}){Constants.ContainsLink.ToCypher()}({permissionParam})")
                .Return<Guid>("r.Id");
            return (List<Guid>)await query.ResultsAsync;
        }

        public async Task<IReadOnlyList<Guid>> AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds,
            Guid tenantId, IReadOnlyCollection<string>? roleLabels = null)
        {
            const string roleParam = "r";
            const string tenantParam = "t";
            const string licensedFeatureParam = "lf";
            const string featureParam = "f";
            const string permissionParam = "p";
            var roleLabelsValue = roleLabels is not null && roleLabels.Any() ? ":" + string.Join(':', roleLabels.Where(r => !string.IsNullOrEmpty(r))) : "";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({roleParam}:{nameof(Role)}{roleLabelsValue}){Constants.OwnsIncomingLink.ToCypher()}({tenantParam} {{Id:${nameof(tenantId)}}})," +
                    $"({licensedFeatureParam}:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}({featureParam}:{nameof(Feature)})" +
                    $"{Constants.ContainsLink.ToCypher()}({permissionParam}:{nameof(Permission)})")
                .Where($"{licensedFeatureParam}.Id IN {{{nameof(licensedFeatureIds)}}}")
                .WithParam(nameof(tenantId), tenantId)
                .WithParam(nameof(licensedFeatureIds), licensedFeatureIds)
                .Merge($"({roleParam}){Constants.ContainsLink.ToCypher()}({permissionParam})")
                .Return<Guid>("r.Id");
            return (List<Guid>)await query.ResultsAsync;
        }

        public async Task UnassignPermissionsFromRolesThroughFeatureUnassignmentAsync(Guid featureId,
            IReadOnlyCollection<Guid> permissionIds)
        {
            const string roleParam = "rol";
            const string permissionParam = "p";
            const string featureParam = "f";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({roleParam}:{nameof(Role)}){Constants.ContainsVariableLink.ToCypher()}({permissionParam}:{nameof(Permission)})" +
                    $"{Constants.ContainsIncomingLink.ToCypher()}({featureParam}:{nameof(Feature)}{{Id:\"{featureId}\"}})")
                .Where($"{permissionParam}.Id IN {{permissions}}")
                .WithParam("permissions", permissionIds)
                .Delete(Constants.ContainsVariableLink.Variable);
            await query.ExecuteWithoutResultsAsync();
        }
        
        public async Task UnassignPermissionsFromRolesThroughLicensedFeatureUnassignmentsAsync(IReadOnlyCollection<Guid> licensedFeatureIds,
            Guid tenantId, IReadOnlyCollection<string>? roleLabels = null)
        {
            const string roleParam = "tr";
            const string tenantParam = "t";
            const string licensedFeatureParam = "lf";
            const string featureParam = "f";
            const string permissionParam = "p";
            var roleLabelsValue = roleLabels is not null && roleLabels.Any() ? ":" + string.Join(':', roleLabels.Where(r => !string.IsNullOrEmpty(r))) : "";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({tenantParam} {{Id: ${nameof(tenantId)}}}){Constants.OwnsLink.ToCypher()}({roleParam}:{nameof(Role)}{roleLabelsValue}){Constants.ContainsVariableLink.ToCypher()}({permissionParam}:{nameof(Permission)})" +
                    $"{Constants.ContainsIncomingLink.ToCypher()}({featureParam}:{nameof(Feature)}){Constants.ContainsIncomingLink.ToCypher()}({licensedFeatureParam}:{nameof(LicensedFeature)})")
                .Where($"{licensedFeatureParam}.Id IN {{{nameof(licensedFeatureIds)}}}")
                .WithParam(nameof(tenantId), tenantId)
                .WithParam(nameof(licensedFeatureIds), licensedFeatureIds)
                .Delete(Constants.ContainsVariableLink.Variable);
            await query.ExecuteWithoutResultsAsync();
        }

        public async Task AssignLicensedFeaturesToTenantAsync(Guid tenantId, IReadOnlyCollection<Guid> licensedFeaturesIds)
        {
            const string lfParam = "lf";
            const string tenantParam = "p";

            var query = (await GraphClient).Cypher
                .Match(
                    $"({lfParam}:{nameof(LicensedFeature)})," +
                    $"({tenantParam}:{nameof(Tenant)}{{Id:\"{tenantId}\"}})")
                .Where($"{lfParam}.Id IN {{licensedFeatures}}")
                .WithParam("licensedFeatures", licensedFeaturesIds)
                .Merge($"({tenantParam}){Constants.AssignedLink.ToCypher()}({lfParam})");

            await query.ExecuteWithoutResultsAsync();
        }

        public async Task UnassignLicensedFeaturesFromTenantAsync(Guid tenantId, IReadOnlyCollection<Guid> licensedFeaturesIds)
        {
            const string lfParam = "lf";
            const string tenantParam = "p";
            var assignLink = Constants.AssignedWithVariableIncomingLink;
            var query = (await GraphClient).Cypher
                .Match(
                    $"({lfParam}:{nameof(LicensedFeature)})" +
                    $"{assignLink.ToCypher()}({tenantParam}:{nameof(Tenant)}{{Id:\"{tenantId}\"}})")
                .Where($"{lfParam}.Id IN {{licensedFeatures}}")
                .WithParam("licensedFeatures", licensedFeaturesIds)
                .Delete($"{assignLink.Variable}");

            await query.ExecuteWithoutResultsAsync();
        }

        public async Task BulkUnassignSubjectFromRolesAsync(Guid subjectId, IEnumerable<RoleTenant> assignments)
        {
            var usassignmentsWithGroups = assignments.Select(x => new SubjectToRoleAssignment
            {
                SubjectId = subjectId,
                RoleId = x.RoleId,
                TenantId = x.TenantId
            });

            var query = (await GraphClient).Cypher
                .With("{a} as unasses")
                .WithParam("a", usassignmentsWithGroups)
                .Unwind("unasses", "unass")
                .OptionalMatch(
                    $"(g:{nameof(Group)})" +
                    $"{Constants.AssignedLink.ToCypher()}" +
                    $"(:{nameof(Role)} {{{nameof(Role.Id)}:unass.RoleId}})," +
                    $"({nameof(Tenant)} {{{nameof(Tenant.Id)}:unass.TenantId}})" +
                    $"{Constants.BelongsIncomingLink.ToCypher()}" +
                    $"(g)" +
                    $"{Constants.MemberOfIncomingLink.ToCypher()}" +
                    $"(:{nameof(Subject)} {{{nameof(Subject.Id)}:unass.SubjectId}})")
                .Delete(Constants.MemberOfIncomingLink.GetVariableName());

            await query.ExecuteWithoutResultsAsync();
        }

        public async Task<IReadOnlyList<Dependency>> GetFeaturesDependenciesAsync(IReadOnlyCollection<Guid> featureIds)
        {
            const string featureParam = "f";
            const string dependencyParam = "d";
            var query = (await GraphClient).Cypher
                .Match(
                    $"({featureParam}:{nameof(Feature)}){Constants.DependsOnRecursiveLink.ToCypher()}({dependencyParam}:{nameof(Feature)})")
                .Where($"{featureParam}.{nameof(Feature.Id)} in {{features}}")
                .WithParam("features", featureIds)
                .With(
                    $"{{ {nameof(Dependency.Id)}: {featureParam}.{nameof(Feature.Id)}," +
                    $"{nameof(Dependency.Dependencies)}: collect({dependencyParam}.{nameof(Feature.Id)})}} as res")
                .Return(res => res.As<Dependency>());
            return (List<Dependency>)await query.ResultsAsync;
        }

        public async Task<bool> IsTenantAssignedToFeatureCoDependenciesAsync(Guid tenantId, Guid featureId)
        {
            const string tenantParam = "t";
            const string dependencyParam = "fd";
            var query = (await GraphClient).Cypher
                .Match(
                    $"({tenantParam}:{nameof(Tenant)}{{{nameof(Tenant.Id)}:\"{tenantId}\"}})" +
                    $"{Constants.AssignedLink.ToCypher()}(:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}(:{nameof(Feature)}{{{nameof(Feature.Id)}:\"{featureId}\"}})" +
                    $"{Constants.DependsOnRecursiveLink.ToCypher()}({dependencyParam}:{nameof(Feature)})")
                .OptionalMatch(
                    $"({tenantParam}){Constants.AssignedWithVariableLink.ToCypher()}(:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}({dependencyParam})")
                .Return<bool>($"(count({Constants.AssignedWithVariableLink.GetVariableName()}) = " +
                              $"count({Constants.DependsOnRecursiveLink.GetVariableName()}))");
            return (await query.ResultsAsync).First();
        }

        public async Task<bool> IsTenantAssignedToFeatureWithoutDependants(Guid tenantId, Guid featureId)
        {
            const string tenantParam = "t";
            const string dependencyParam = "fd";
            const string featureParam = "f";
            var query = (await GraphClient).Cypher
                .Match($"({tenantParam}:{nameof(Tenant)}{{{nameof(Tenant.Id)}:\"{tenantId}\"}})" +
                       $"{Constants.AssignedLink.ToCypher()}(:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                       $"({featureParam}:{nameof(Feature)}{{{nameof(Feature.Id)}:\"{featureId}\"}})")
                .OptionalMatch($"({dependencyParam}:{nameof(Feature)}){Constants.DependsOnRecursiveLink.ToCypher()}" +
                               $"({featureParam})")
                .Return<bool>($"count({Constants.DependsOnRecursiveLink.GetVariableName()}) = 1");
            return (await query.ResultsAsync).First();
        }

        public async Task<Dictionary<string, int>> GetStats()
        {
            var query = (await GraphClient).Cypher
                .Match($"(r:{nameof(Role)})")
                .With($"'{nameof(Role)}' as type,count(r) as count")
                .Return((type, count) => new
                {
                    Type = type.As<string>(),
                    Count = count.As<int>()
                })
                .Union()
                .Match($"(t:{nameof(Tenant)})")
                .With($"'{nameof(Tenant)}' as type,count(t) as count")
                .Return((type, count) => new
                {
                    Type = type.As<string>(),
                    Count = count.As<int>()
                })
                .Union()
                .Match($"(f:{nameof(Feature)})")
                .With($"'{nameof(Feature)}' as type,count(f) as count")
                .Return((type, count) => new
                {
                    Type = type.As<string>(),
                    Count = count.As<int>()
                });
            return (await query.ResultsAsync).ToDictionary(x => x.Type, x => x.Count);
        }

        private static Group GenerateGroup(Guid roleId, Guid tenantId) =>
            new Group
            {
                Id = tenantId.Combine(roleId),
                Name = $"{tenantId}_{roleId}"
            };

        public async Task<IReadOnlyCollection<Guid>> FilterFeatureIdsWithAccessDeniedAsync(ClaimsPrincipal subject, IReadOnlyCollection<Guid> featureIds, IReadOnlyCollection<Guid>? tenantIds = null)
        {
            var tenants = subject.GetTenants(limitTo: tenantIds);
            var isAdmin = subject.IsAdformAdmin();

            var where = isAdmin && !tenants.Any() ? "t:Tenant" : "t.Id in {tenants}";
            var cypher = (await GraphClient).Cypher
                .Match(
                    $"(t:{nameof(Tenant)}){Constants.AssignedLink.ToCypher()}" +
                    $"(lf:{nameof(LicensedFeature)}){Constants.ContainsLink.ToCypher()}" +
                    $"(f:{nameof(Feature)})")
                .Where(where)
                .WithParam("tenants", tenants.Select(x => x.ToString()))
                .AndWhere((Feature f) => f.Id.In(featureIds))
                .ReturnDistinct(f => f.As<Feature>());
            var result = await cypher.ResultsAsync;

            return featureIds.Except(result.Select(f => f.Id)).ToList().AsReadOnly();
        }
    }

    public class Dependency
    {
        public Guid Id { get; set; }
        public List<Guid> Dependencies { get; set; } = new List<Guid>();
    }
}