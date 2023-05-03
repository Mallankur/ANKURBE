using System.Text;
using System.Text.Json;
using Adform.Bloom.Application.Abstractions.Cache;
using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Runtime.Infrastructure.Cache
{
    public class KeyGenerator : IKeyGenerator<SubjectQueryBase>
    {
        public string GenerateKey(SubjectQueryBase query)
        {
            query.TenantIds = query.TenantIds.Distinct().OrderBy(p => p);
            query.TenantLegacyIds = query.TenantLegacyIds.Distinct().OrderBy(p => p);
            query.PolicyNames = query.PolicyNames.Distinct().OrderBy(p => p);
            var sb = new StringBuilder();
            sb.Append(query.SubjectId);
            sb.Append(":");
            sb.Append(nameof(SubjectQueryBase));
            sb.Append(":");
            var json = JsonSerializer.Serialize(query);
            var inputBytes = Encoding.UTF8.GetBytes(json);
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hashBytes = md5.ComputeHash(inputBytes);
            foreach (var t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}