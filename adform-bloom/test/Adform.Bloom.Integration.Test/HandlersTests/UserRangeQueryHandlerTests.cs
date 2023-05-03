using System;
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
using Mapster;
using MapsterMapper;
using Xunit;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class UserRangeQueryHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly UserRangeQueryHandler _handler;
        private readonly TestsFixture _fixture;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Subject> _repository;
        private readonly Mapper _mapper;

        public UserRangeQueryHandlerTests(TestsFixture fixture)
        {
            _mapper = new Mapper();
            _repository = fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Subject>();
            _handler = new UserRangeQueryHandler(
                _repository,
                fixture.UserReadModel,
                _mapper
            );
            _fixture = fixture;
        }

        [Theory]
        [ClassData(typeof(NotEligibleSearchTestData))]
        public async Task UserRangeQueryHandler_Returns_Repository_Result_When_Not_Eligible_For_Search(
            QueryParamsTenantIdsInput filterInput)
        {
            var filter = filterInput.Adapt<QueryParamsTenantIds>();
            var principal = _fixture.BloomApiPrincipal[Graph.Subject0];
            var expectedResult =
                await _repository.EvaluateVisibilityAsync(principal, filter, 0, 10);
            var query = new UsersQuery(principal, filterInput, 0, 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(expectedResult.Data.OrderBy(x => x.Id).Select(o => o.Id)
                .SequenceEqual(result.Data.OrderBy(x => x.Id).Select(x => x.Id)));
        }

        [Theory]
        [ClassData(typeof(SearchTestData))]
        public async Task UserRangeQueryHandler_Returns_Repository_Result(QueryParamsTenantIdsInput filterInput)
        {
            var principal = _fixture.BloomApiPrincipal[Graph.Subject0];
            var filter = filterInput.Adapt<QueryParamsTenantIds>();
            var subjects = await _repository.EvaluateVisibilityAsync(principal, filter, 0, 10);
            var users = await _fixture.SQL.PsqlConnection.QueryAsync<User>(
                "select * from users where email ~* @Search OR name ~* @Search", new
                {
                    filter.Search
                });
            var expectedIds = subjects.Data.Select(x => x.Id).Intersect(users.Select(x => x.Id));
            var query = new UsersQuery(principal, filterInput, 0, 10);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.True(expectedIds.OrderBy(o => o).SequenceEqual(result.Data.OrderBy(o => o.Id).Select(x => x.Id)));
        }

        [Theory]
        [ClassData(typeof(FilterTestData))]
        public async Task UserRangeQueryHandler_Returns_Filtered_Result(Guid[] expectedUserIds, Guid[] subjectIds,
            Guid tenantId)
        {
            var principal = _fixture.BloomApiPrincipal[Graph.Subject0];
            var query = new UsersQuery(principal, new QueryParamsTenantIdsInput
            {
                ResourceIds = subjectIds,
                TenantIds = new[] {tenantId}
            }, 0, 10);
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.Data.Select(x => x.Id).OrderBy(x => x).SequenceEqual(expectedUserIds.OrderBy(x => x)));
        }

        [Theory]
        [ClassData(typeof(FilterBaseOnRoleTestData))]
        public async Task UserRangeQueryHandler_Returns_Filtered_BasedOn_Actor_Role(string caseName, string subject,
            Domain.Entities.Subject[] expectedUserIds)
        {
            var principal = _fixture.BloomApiPrincipal[subject];
            var query = new UsersQuery(principal,new QueryParamsTenantIdsInput(), 0, 100);
            var result = await _handler.Handle(query, CancellationToken.None);
            Assert.True(result.Data.Select(x => x.Id).OrderBy(x => x)
                .SequenceEqual(expectedUserIds.OrderBy(x => x.Id).Select(p => p.Id)));
        }

        public class NotEligibleSearchTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = null
                    }
                };
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "a"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "ab"
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class SearchTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "aaa"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "bbb"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "ccc"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsTenantIdsInput
                    {
                        Search = "ddd"
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class FilterTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new[] { Guid.Parse(Graph.Subject2),Guid.Parse(Graph.Subject3) },
                    null,
                    Guid.Parse(Graph.Tenant2)
                };
                yield return new object[]
                {
                    new[] {Guid.Parse(Graph.Subject6)},
                    null,
                    Guid.Parse(Graph.Tenant12)
                };
                yield return new object[]
                {
                    new[] {Guid.Parse(Graph.Subject10)},
                    null,
                    Guid.Parse(Graph.Tenant13)
                };
                
                yield return new object[]
                {
                    new[] { Guid.Parse(Graph.Subject2)},
                    new[] {Guid.Parse(Graph.Subject2)},
                    Guid.Parse(Graph.Tenant2)
                };
                yield return new object[]
                {
                    new[] { Guid.Parse(Graph.Subject3)},
                    new[] {Guid.Parse(Graph.Subject3)},
                    Guid.Parse(Graph.Tenant2)
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public class FilterBaseOnRoleTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var subjects = Graph.GetSubjects();
                yield return new object[]
                {
                    "Case 0",
                    Graph.Subject0,
                    subjects.Take(11),
                };
                yield return new object[]
                {
                    "Case 1",
                    Graph.Subject2,
                    new[]
                    {
                        subjects[2], subjects[3]
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}