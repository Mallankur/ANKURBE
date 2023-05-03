using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using Bogus;
using Mapster;
using MapsterMapper;
using Moq;
using Xunit;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Unit.Test.Read
{
    public class UserRangeQueryHandlerTests
    {
        private readonly UserRangeQueryHandler _handler;
        private readonly Faker<Subject> _subjectFaker;
        private readonly Faker<User> _userFaker;

        private readonly Mock<IVisibilityProvider<QueryParamsTenantIds, Subject>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParamsTenantIds, Subject>>();

        private readonly Mock<IUserReadModelProvider> _readModelProviderMock = new Mock<IUserReadModelProvider>();
        private readonly Mock<IMapper> _mapperMock = new Mock<IMapper>();
        private readonly Mapper _mapper;

        public UserRangeQueryHandlerTests()
        {
            _mapper = new Mapper();
            _handler = new UserRangeQueryHandler(_accessRepositoryMock.Object, _readModelProviderMock.Object,
                _mapperMock.Object);
            _subjectFaker = new Faker<Subject>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Enabled, f => f.Random.Bool());
            _userFaker = new Faker<User>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Email, f => f.Person.Email)
                .RuleFor(o => o.Username, f => f.Person.UserName);
        }

        [Theory]
        [ClassData(typeof(NotEligibleSearchTestData))]
        public async Task ReadModel_Is_Not_Invoked_When_Search_Not_Eligible(QueryParamsTenantIdsInput filterInput)
        {
            // Arrange
            var subject = _subjectFaker.Generate();
            var user = _userFaker.Generate();
            user.Id = subject.Id;
            var query = new UsersQuery(new ClaimsPrincipal(), filterInput, 0, 10);

            var filter = _mapper.Adapt<QueryParamsTenantIds>(filterInput);
            _accessRepositoryMock.Setup(m => m.EvaluateVisibilityAsync(
                    query.Principal, filter, It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new EntityPagination<Subject>(0, 10, 1, new[] {subject}));
            _readModelProviderMock.Setup(m =>
                    m.SearchForResourcesAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<QueryParams?>(),
                        It.IsAny<IEnumerable<Guid>>(),
                        It.IsAny<UserType>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityPagination<User>(0, 10, 1, new[] {user}));
            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _accessRepositoryMock.Verify(
                m => m.EvaluateVisibilityAsync(query.Principal, 0, int.MaxValue,
                    query.TenantIds, query.ContextId, null, It.IsAny<QueryParams?>()), Times.Once);
            _readModelProviderMock.Verify(
                x => x.SearchForResourcesAsync(query.Offset, query.Limit,
                    It.Is<QueryParams?>(o => o.Search == null),
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<UserType>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [ClassData(typeof(SearchTestData))]
        public async Task ReadModel_Is_Invoked_When_Search_Eligible(QueryParamsInput queryParams)
        {
            // Arrange
            var subject = _subjectFaker.Generate();
            var user = _userFaker.Generate();
            user.Id = subject.Id;
            var query = new UsersQuery(new ClaimsPrincipal(), 0, 10, queryParams: queryParams);
            _accessRepositoryMock.Setup(m => m.EvaluateVisibilityAsync(
                    query.Principal, 0, int.MaxValue,
                    query.TenantIds, query.ContextId, It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<QueryParams?>()))
                .ReturnsAsync(new EntityPagination<Subject>(0, 10, 1, new[] {subject}));
            _readModelProviderMock.Setup(m =>
                    m.SearchForResourcesAsync(
                        query.Offset,
                        query.Limit,
                        It.IsAny<QueryParams?>(),
                        It.IsAny<IEnumerable<Guid>>(),
                        It.IsAny<UserType>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityPagination<User>(0, 10, 1, new[] {user}));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _readModelProviderMock.Verify(
                x => x.SearchForResourcesAsync(
                    It.Is<int>(p => p == query.Offset), It.Is<int>(p => p == query.Limit),
                    It.Is<QueryParams?>(o => o.Search == query.QueryParams.Search),
                    It.Is<IEnumerable<Guid>>(x => x.Contains(user.Id)),
                    It.Is<UserType>(p => p == UserType.MasterAccount),
                    It.IsAny<CancellationToken>()), Times.Once);
            _accessRepositoryMock.Verify(
                m => m.EvaluateVisibilityAsync(query.Principal, 0, int.MaxValue,
                    query.TenantIds, query.ContextId, null, null),
                Times.Once);
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
    }
}