using System.Text;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.OngDb.Core.Extensions;
using Neo4j.Driver;

namespace Adform.Bloom.Runtime.Infrastructure.Persistence
{
    public class RuntimeProvider : IRuntimeProvider
    {
        private const string ResultWithTenant =
            " RETURN t.Id as TenantId, t.Name as TenantName, [x IN LABELS(t) WHERE NOT x = 'Tenant'][0] as TenantType, t.LegacyId as TenantLegacyId, collect(distinct r.Name) as Roles, collect(distinct per.Name) as Permissions";

        private static readonly string OptionalMatch =
            $" OPTIONAL MATCH (r){Constants.ContainsLink.ToCypher()}(per:Permission)";

        private static readonly string ChildOfLink = Constants.ChildOfDepthLink.ToCypher();
        private static readonly string ChildOfIncomingLink = Constants.ChildOfDepthIncomingLink.ToCypher();
        private static readonly string BelongsIncomingLink = Constants.BelongsIncomingLink.ToCypher();
        private static readonly string BelongsLink = Constants.BelongsLink.ToCypher();
        private static readonly string ContainsLink = Constants.ContainsLink.ToCypher();
        private static readonly string MemberOfLink = Constants.MemberOfLink.ToCypher();
        private static readonly string MemberOfIncomingLink = Constants.MemberOfIncomingLink.ToCypher();
        private static readonly string SubjectToRole =
            $"{Constants.MemberOfLink.ToCypher()}(g:Group){Constants.AssignedLink.ToCypher()}";

        private readonly IDriver _driver;
        private readonly ICursorToResultConverter _cursorToResultConverter;

        public RuntimeProvider(IDriver driver, ICursorToResultConverter cursorToResultConverter)
        {
            _cursorToResultConverter = cursorToResultConverter;
            _driver = driver;
        }

        public async Task<IEnumerable<RuntimeResult>> GetSubjectEvaluation(SubjectQueryBase dto, CancellationToken cancellationToken = default)
        {
            var match = new StringBuilder();
            var parameters = new Dictionary<string, object> { { "subjectId", dto.SubjectId.ToString() } };

            match.Append(
                $"MATCH (s:Subject {{Id: $subjectId}}){SubjectToRole}(r:Role),");

            TryAddMatch(match, dto);
            TryAddWhere(match, dto, parameters);
            match.Append(OptionalMatch);
            match.Append(ResultWithTenant);
            var session = _driver.AsyncSession(o => o
                .WithDefaultAccessMode(AccessMode.Read));
            IEnumerable<RuntimeResult> result;
            try
            {
                var reader = await session.RunAsync(match.ToString(), parameters);
                result = await _cursorToResultConverter.ConvertToRuntimeResultAsync(reader);
            }
            finally
            {
                await session.CloseAsync();
            }

            return result;
        }

        public async Task<IEnumerable<RuntimeResult>> GetSubjectIntersection(SubjectIntersectionQuery dto, CancellationToken cancellationToken = default)
        {
            if (dto.ActorId.Equals(dto.SubjectId))
            {
                return await GetSubjectEvaluation(dto);
            }
            var match = new StringBuilder();
            var parameters = new Dictionary<string, object>
            {
                {"subjectId", dto.SubjectId.ToString()},
                {"actorId", dto.ActorId.ToString()}
            };

            match.Append($"MATCH (s:{Constants.Subject} {{Id: $actorId}}){SubjectToRole}(r:{Constants.Role})," +
                         $"(s2:{Constants.Subject}{{Id: $subjectId}}){MemberOfLink}(:{Constants.Group}){BelongsLink}(t"
            );

            if (dto.InheritanceEnabled)
            {
                match.Append($"0:{Constants.Tenant}){ChildOfIncomingLink}(t");
            }
            match.Append($":Tenant) MATCH");

            if (dto.InheritanceEnabled)
            {
                match.Append(@$"(t0){ChildOfLink}(:{Constants.Tenant}){BelongsIncomingLink}(:{Constants.Group}){MemberOfIncomingLink}(s)
                MATCH ");
            }
            TryAddMatch(match, dto);
            TryAddWhere(match, dto, parameters);
            match.Append(OptionalMatch);
            match.Append(ResultWithTenant);
            var session = _driver.AsyncSession(o => o
                .WithDefaultAccessMode(AccessMode.Read));
            IEnumerable<RuntimeResult> result;
            try
            {
                var reader = await session.RunAsync(match.ToString(), parameters);
                result = await _cursorToResultConverter.ConvertToRuntimeResultAsync(reader);
            }
            finally
            {
                await session.CloseAsync();
            }

            return result;
        }

        private static void TryAddMatch(StringBuilder builder, SubjectQueryBase dto)
        {
            var inheritanceTenant = "";
            if (dto.InheritanceEnabled)
            {
                inheritanceTenant = $"{ChildOfLink}(:{Constants.Tenant})";
            }

            if (dto.TenantType == null)
            {
                builder.Append($"(t:Tenant){inheritanceTenant}{BelongsIncomingLink}(g)");
            }
            else
            {
                builder.Append($"(t:Tenant:{dto.TenantType}){inheritanceTenant}{BelongsIncomingLink}(g)");
            }

            if (dto.PolicyNames.Any())
            {
                var inheritancePolicy =
                    dto.InheritanceEnabled ? $"{ChildOfLink}(p0:Policy)" : "";
                builder.Append($",(p:Policy){inheritancePolicy}{ContainsLink}(r)");
            }
        }

        private static void TryAddWhere(StringBuilder builder, SubjectQueryBase dto,
            Dictionary<string, object> parameters)
        {
            if (dto.TenantIds.Any())
            {
                builder.Append(" WHERE t.Id in $tenantIds");
                parameters.Add("tenantIds", dto.TenantIds.Select(x => x.ToString()));
            }

            if (dto.TenantLegacyIds.Any())
            {
                builder.Append(!dto.TenantIds.Any() ? " WHERE" : " AND");
                builder.Append(" t.LegacyId in $tenantLegacyIds");
                parameters.Add("tenantLegacyIds", dto.TenantLegacyIds.Select(x => x));
            }

            if (dto.PolicyNames.Any())
            {
                builder.Append(!dto.TenantIds.Any() && !dto.TenantLegacyIds.Any() ? " WHERE" : " AND");
                builder.Append(" p.Name in $policyNames");
                parameters.Add("policyNames", dto.PolicyNames);
            }
        }
    }
}