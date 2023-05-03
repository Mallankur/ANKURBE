using System.Text;
using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Ciam.OngDb.Core.Extensions;
using Neo4j.Driver;

namespace Adform.Bloom.Runtime.Infrastructure.Persistence
{
    public class AdformTenantProvider : IAdformTenantProvider
    {

        private const string ResultWithTenant =
            " RETURN t.Id as TenantId LIMIT 1";

        private static readonly string SubjectToGroup =
            $"{Constants.MemberOfLink.ToCypher()}(g:Group)";

        private static readonly string GroupToTenant =
            $"{Constants.BelongsLink.ToCypher()}(t:Tenant:{Constants.AdformLabel})";

        private readonly IDriver _driver;
        private readonly ICursorToResultConverter _cursorToResultConverter;

        public AdformTenantProvider(IDriver driver, ICursorToResultConverter cursorToResultConverter)
        {
            _driver = driver;
            _cursorToResultConverter = cursorToResultConverter;
        }

        public async Task<Guid> GetAdformTenant(Guid SubjectId, CancellationToken cancellationToken = default)
        {
            var match = new StringBuilder();
            var parameters = new Dictionary<string, object> { { "subjectId", SubjectId.ToString() } };
            match.Append($"MATCH (s:Subject {{Id: $subjectId}}){SubjectToGroup}{GroupToTenant}");
            match.Append(ResultWithTenant);
            var session = _driver.AsyncSession(o => o
                .WithDefaultAccessMode(AccessMode.Read));
            Guid result;
            try
            {
                var reader = await session.RunAsync(match.ToString(), parameters);
                result = await _cursorToResultConverter.ConvertToTenantIdAsync(reader);
            }
            finally
            {
                await session.CloseAsync();
            }

            return result;
        }
    }
}