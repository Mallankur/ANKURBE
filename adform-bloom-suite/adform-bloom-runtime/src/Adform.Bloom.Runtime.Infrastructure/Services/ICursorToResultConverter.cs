using Adform.Bloom.Runtime.Read.Entities;
using Neo4j.Driver;

namespace Adform.Bloom.Runtime.Infrastructure.Services
{
    public interface ICursorToResultConverter
    {
        Task<Guid> ConvertToTenantIdAsync(IResultCursor reader);
        Task<IEnumerable<RuntimeResult>> ConvertToRuntimeResultAsync(IResultCursor reader);
        Task<int> ConvertToCountResult(IResultCursor reader);
    }
}