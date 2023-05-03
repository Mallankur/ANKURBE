using System.Text;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Infrastructure.Services;
using FluentResults;
using MediatR;
using Neo4j.Driver;

namespace Adform.Bloom.Runtime.Infrastructure.Persistence
{
    public class ExistenceProvider : IExistenceProvider
    {
        private readonly IDriver _driver;
        private readonly ICursorToResultConverter _cursorToResultConverter;


        public ExistenceProvider(IDriver driver, ICursorToResultConverter cursorToResultConverter)
        {
            _driver = driver;
            _cursorToResultConverter = cursorToResultConverter;
        }

        public async Task<Result<bool>> CheckExistence(IRequest<Result<bool>> existenceRequest, CancellationToken cancellationToken = default)
        {
            return existenceRequest switch
            {
                NodeExistenceQuery nodeQuery => await CheckExistenceInternal(nodeQuery),
                LegacyTenantExistenceQuery legacyTenantQuery => await CheckExistenceInternal(legacyTenantQuery),
                RoleExistenceQuery roleQuery => await CheckExistenceInternal(roleQuery),
                _ => Result.Fail<bool>(new Error($"ExistenceProvider does not support query:{existenceRequest.GetType().FullName}"))
            };
        }

        private async Task<Result<bool>> CheckExistenceInternal(RoleExistenceQuery request)
        {
            const string match = "MATCH (x:Tenant {Id: $tenantId})-[:OWNS]->(r:Role {Name: $roleName}) RETURN count(*)";
            var parameters = new Dictionary<string, object>
            {
                {"tenantId", request.TenantId.ToString()},
                {"roleName", request.RoleName}
            };

            var result = await ExecuteQuery(match, parameters);
            if (result.IsSuccess)
                return result.Value == 1;
            return Result.Fail(result.Errors);
        }

        private async Task<Result<bool>> CheckExistenceInternal(LegacyTenantExistenceQuery request)
        {
            if (request.TenantLegacyIds.Count == 0)
                return true;

            var match = $"MATCH (x:Tenant:{request.TenantType}) WHERE x.LegacyId in $legacyIds RETURN count(*)";
            var parameters = new Dictionary<string, object>
            {
                {"legacyIds", request.TenantLegacyIds}
            };

            var result = await ExecuteQuery(match, parameters);
            if (result.IsSuccess)
                return result.Value == request.TenantLegacyIds.Count;
            return Result.Fail(result.Errors);
        }

        private async Task<Result<bool>> CheckExistenceInternal(NodeExistenceQuery request)
        {
            if (request.NodeDescriptors.Count == 0)
                return true;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("MATCH (x) WHERE ");
            for (var i = 0; i < request.NodeDescriptors.Count; ++i)
            {
                var node = request.NodeDescriptors[i];
                stringBuilder.Append($"(x:{node.Label}");

                if (node.Id != null)
                {
                    stringBuilder.Append($" AND x.Id = \"{node.Id.ToString()}\"");
                }
                if (node.UniqueName != null)
                    stringBuilder.Append($" AND x.Name = \"{node.UniqueName}\"");
                stringBuilder.Append(')');

                if (i != request.NodeDescriptors.Count - 1)
                {
                    stringBuilder.Append(" OR ");
                }
            }

            stringBuilder.Append(" RETURN count(*)");

            var result = await ExecuteQuery(stringBuilder.ToString());
            if (result.IsSuccess)
                return result.Value == request.NodeDescriptors.Count;
            return Result.Fail(result.Errors);

        }

        private async Task<Result<int>> ExecuteQuery(string query, Dictionary<string, object>? parameters = null)
        {
            var session = _driver.AsyncSession(o => o
                .WithDefaultAccessMode(AccessMode.Read));
            var result = -1;
            try
            {
                IResultCursor? reader;
                if (parameters != null)
                    reader = await session.RunAsync(query, parameters);
                else
                    reader = await session.RunAsync(query);
                result = await _cursorToResultConverter.ConvertToCountResult(reader);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e));
            }
            finally
            {
                await session.CloseAsync();
            }

            return result;
        }
    }
}
