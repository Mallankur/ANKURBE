using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Dapper;
using MapsterMapper;
using Xunit;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class BusinessAccountRangeQueryHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly BusinessAccountRangeQueryHandler _handler;
        private readonly TestsFixture _fixture;
        private readonly IVisibilityProvider<QueryParamsBusinessAccount, Tenant> _repository;

        public BusinessAccountRangeQueryHandlerTests(TestsFixture fixture)
        {
            _repository = fixture.VisibilityRepositoriesContainer.Get<QueryParamsBusinessAccount, Tenant>();
            _handler = new BusinessAccountRangeQueryHandler(
                _repository,
                fixture.BusinessAccountReadModel,
                new Mapper()
            );
            _fixture = fixture;
        }

        [Fact]
        public async Task BusinessAccountRangeQueryHandler_Returns_Repository_Result()
        {
            var principal = _fixture.BloomApiPrincipal[Graph.Subject0];
            var expectedResult =
                await _repository.EvaluateVisibilityAsync(principal, new QueryParamsBusinessAccount(), 0, 20);
            var query = new BusinessAccountsQuery(principal, new QueryParamsBusinessAccountInput(), 0, 20);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(expectedResult.Data.OrderBy(x => x.Id).Select(o => o.Id)
                .SequenceEqual(result.Data.OrderBy(x => x.Id).Select(x => x.Id)));
        }

        [Theory]
        [ClassData(typeof(SearchTestData))]
        public async Task BusinessAccountRangeQueryHandler_Returns_Searched_Result(
            QueryParamsBusinessAccountInput filter)
        {
            var principal = _fixture.BloomApiPrincipal[Graph.Subject0];
            var subjects =
                await _repository.EvaluateVisibilityAsync(principal, new QueryParamsBusinessAccount(), 0, 10);
            var users = await _fixture.SQL.PsqlConnection.QueryAsync<BusinessAccount>(
                "select * from business_accounts where (name ~* @Search) and type = @Type",
                new
                {
                    filter.Search,
                    Type = filter.BusinessAccountType
                });
            var expectedIds = subjects.Data.Select(x => x.Id).Intersect(users.Select(x => x.Id));
            var query = new BusinessAccountsQuery(principal, filter, 0, 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(expectedIds.OrderBy(o => o).SequenceEqual(result.Data.OrderBy(o => o.Id).Select(x => x.Id)));
        }

        public class SearchTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "aaa",
                        BusinessAccountType = 0
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "bbb",
                        BusinessAccountType = 0
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ccc",
                        BusinessAccountType = 0
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ddd",
                        BusinessAccountType = 0
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "aaa",
                        BusinessAccountType = 1
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "bbb",
                        BusinessAccountType = 1
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ccc",
                        BusinessAccountType = 1
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ddd",
                        BusinessAccountType = 1
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}