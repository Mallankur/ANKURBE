using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Read.Queries;

namespace Adform.Bloom.Unit.Test.Read
{
    public class TestRangeQuery : BaseRangeQuery<QueryParamsInput, TestDto>
    {
        public TestRangeQuery(ClaimsPrincipal principal, QueryParamsInput filter, int offset = 0, int limit = 100) :
            base(principal, filter, offset, limit)
        {
        }
    }
}