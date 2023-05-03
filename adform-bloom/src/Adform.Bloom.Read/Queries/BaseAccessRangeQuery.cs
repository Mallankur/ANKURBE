using System.Security.Claims;
using Adform.Bloom.Read.Interfaces;
using Adform.Ciam.SharedKernel.Entities;

namespace Adform.Bloom.Read.Queries
{
    public class
        BaseAccessRangeQuery<TContextDto, TFilterInput , TOutputDto> : IBaseAccessRangeQuery<TContextDto, TFilterInput,
            EntityPagination<TOutputDto>>
    {
        public ClaimsPrincipal Principal { get; }
        public int Offset { get; }
        public int Limit { get; }
        public TContextDto Context { get; }
        public TFilterInput? Filter { get; }

        public BaseAccessRangeQuery(ClaimsPrincipal principal, TContextDto context,
            int offset = 0, int limit = 100,
            TFilterInput? filter = default)
        {
            Principal = principal;
            Context = context;
            Offset = offset;
            Limit = limit;
            Filter = filter;
        }
    }
}