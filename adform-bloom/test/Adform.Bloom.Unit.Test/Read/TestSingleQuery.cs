using System;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Read.Queries;

namespace Adform.Bloom.Unit.Test.Read
{
    public class TestSingleQuery: BaseSingleQuery<QueryParamsInput, TestDto>
    {
        public TestSingleQuery(ClaimsPrincipal principal, Guid id) : base(principal, id, new QueryParamsInput())
        {
        }
    }
}