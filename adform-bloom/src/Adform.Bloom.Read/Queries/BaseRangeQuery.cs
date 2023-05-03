using System.Security.Claims;
using Adform.Bloom.Read.Interfaces;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.Read.Queries
{
    public abstract class BaseRangeQuery<TFilterInput, TEntity> : IRangeQuery<TFilterInput, EntityPagination<TEntity>>
    {
        public ClaimsPrincipal Principal { get; }
        public int Offset { get; }
        public int Limit { get; }
        public TFilterInput Filter { get; }

        protected BaseRangeQuery(ClaimsPrincipal principal, TFilterInput filter, int offset = 0, int limit = 100)
        {
            Principal = principal;
            Offset = offset;
            Limit = limit;
            Filter = filter;
        }
    }
}