using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Contracts.Output;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.DataAccess.Extensions
{
    public static class QueryResultExtensions
    {
        public static EntityPagination<TOut> ToEntityPagination<TOut>(this IEnumerable<CypherPaginationResult<TOut>> result,
            int skip, int limit)
            where TOut : BaseNodeDto
        {
            var arrResult = result.ToArray();

            if (arrResult.Length == 0)
            {
                return new EntityPagination<TOut>(skip, limit, 0, new TOut[0]);
            }

            return new EntityPagination<TOut>(skip, limit,
                arrResult[^1].TotalCount,
                arrResult[..^1].Select(r => r.Node!).ToList());
        }
    }
}