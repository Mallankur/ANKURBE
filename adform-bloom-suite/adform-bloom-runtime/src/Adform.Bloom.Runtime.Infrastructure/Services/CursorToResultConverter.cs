using Adform.Bloom.Runtime.Read.Entities;
using Neo4j.Driver;

namespace Adform.Bloom.Runtime.Infrastructure.Services
{
    public class CursorToResultConverter : ICursorToResultConverter
    {
        public async Task<Guid> ConvertToTenantIdAsync(IResultCursor reader)
        {
            var result = await reader.ToListAsync(record =>
                 Guid.Parse(record.Values.ContainsKey("TenantId") ? record.Values["TenantId"].As<string>() : Guid.Empty.ToString()));
            return result.FirstOrDefault();
        }

        public async Task<IEnumerable<RuntimeResult>> ConvertToRuntimeResultAsync(IResultCursor reader)
        {
            return await reader.ToListAsync(item => new RuntimeResult
            {
                Permissions = item.Values["Permissions"].As<IEnumerable<string>>(),
                Roles = item.Values["Roles"].As<IEnumerable<string>>(),
                TenantId = Guid.Parse(item.Values.ContainsKey("TenantId") ? item.Values["TenantId"].As<string>() : Guid.Empty.ToString()),
                TenantName = item.Values.ContainsKey("TenantName") ? item.Values["TenantName"].As<string>() : "all",
                TenantLegacyId = item.Values.ContainsKey("TenantLegacyId") && item.Values["TenantLegacyId"] != null ? item.Values["TenantLegacyId"].As<int>() : 0,
                TenantType = item.Values.ContainsKey("TenantType") && item.Values["TenantType"] != null ? item.Values["TenantType"].As<string>() : "Tenant",
            });
        }

        public async Task<int> ConvertToCountResult(IResultCursor reader)
        {
            return (await reader.SingleAsync())[0].As<int>();
        }
    }
}
