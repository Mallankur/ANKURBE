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
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using Bogus;
using MapsterMapper;
using Moq;
using Xunit;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.Unit.Test.Read
{
    public class BusinessAccountRangeQueryHandlerTests
    {
        private readonly BusinessAccountRangeQueryHandler _handler;
        private readonly Faker<Tenant> _tenantFaker;
        private readonly Faker<BusinessAccount> _businessAccountFaker;

        private readonly Mock<IVisibilityProvider<QueryParamsBusinessAccount, Tenant>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParamsBusinessAccount, Tenant>>();

        private readonly Mock<IBusinessAccountReadModelProvider> _readModelProviderMock =
            new Mock<IBusinessAccountReadModelProvider>();

        private readonly IMapper _mapper;

        public BusinessAccountRangeQueryHandlerTests()
        {
            _mapper = new Mapper();
            _handler = new BusinessAccountRangeQueryHandler(_accessRepositoryMock.Object,
                _readModelProviderMock.Object, _mapper);
            _tenantFaker = new Faker<Tenant>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Enabled, f => f.Random.Bool());
            _businessAccountFaker = new Faker<BusinessAccount>()
                .RuleFor(o => o.Id, f => Guid.NewGuid())
                .RuleFor(o => o.Name, f => f.Company.CompanyName())
                .RuleFor(o => o.Status, f => f.Random.Enum<BusinessAccountStatus>())
                .RuleFor(o => o.Type, f => f.Random.Enum<BusinessAccountType>());
        }

        [Theory]
        [ClassData(typeof(NotEligibleSearchTestData))]
        public async Task ReadModel_Is_Not_Invoked_When_Search_Not_Eligible(QueryParamsBusinessAccountInput filterInput)
        {
            // Arrange
            var tenant = _tenantFaker.Generate();
            var businessAccount = _businessAccountFaker.Generate();
            businessAccount.Id = tenant.Id;

            var query = new BusinessAccountsQuery(new ClaimsPrincipal(), filterInput, 0, 10);
            _accessRepositoryMock.Setup(m => m.EvaluateVisibilityAsync(
                    query.Principal, It.IsAny<QueryParamsBusinessAccount>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new EntityPagination<Tenant>(0, 10, 1, new[] {tenant}));
            _readModelProviderMock.Setup(m =>
                    m.SearchForResourcesAsync(
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<QueryParams?>(),
                        It.IsAny<IEnumerable<Guid>>(),
                        It.IsAny<BusinessAccountType?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityPagination<BusinessAccount>(0, 10, 1, new[] {businessAccount}));
            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _accessRepositoryMock.Verify(
                m => m.EvaluateVisibilityAsync(query.Principal,
                    It.IsAny<QueryParamsBusinessAccount>(), 0, int.MaxValue), Times.Once);
            _readModelProviderMock.Verify(
                x => x.SearchForResourcesAsync(query.Offset, query.Limit,
                    It.Is<QueryParams?>(o => o != null ? o.Search == null : o == null), It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<BusinessAccountType?>(),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [ClassData(typeof(SearchTestData))]
        public async Task ReadModel_Is_Invoked_When_Search_Eligible(QueryParamsBusinessAccountInput filterInput)
        {
            // Arrange
            var tenant = _tenantFaker.Generate();
            var businessAccount = _businessAccountFaker.Generate();
            businessAccount.Id = tenant.Id;
            var query = new BusinessAccountsQuery(new ClaimsPrincipal(), filterInput, 0, 10);
            _accessRepositoryMock.Setup(m => m.EvaluateVisibilityAsync(
                    query.Principal, It.IsAny<QueryParamsBusinessAccount>(), 0, int.MaxValue))
                .ReturnsAsync(new EntityPagination<Tenant>(0, 10, 1, new[] {tenant}));
            _readModelProviderMock.Setup(m =>
                    m.SearchForResourcesAsync(
                        query.Offset,
                        query.Limit,
                        It.IsAny<QueryParams?>(),
                        It.IsAny<IEnumerable<Guid>>(),
                        It.IsAny<BusinessAccountType?>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EntityPagination<BusinessAccount>(0, 10, 1, new[] {businessAccount}));

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _readModelProviderMock.Verify(
                x => x.SearchForResourcesAsync(
                    It.Is<int>(p => p == query.Offset), It.Is<int>(p => p == query.Limit),
                    It.Is<QueryParamsBusinessAccount>(o => o.Search == query.Filter.Search),
                    It.Is<IEnumerable<Guid>>(x => x.Contains(businessAccount.Id)),
                    It.Is<BusinessAccountType?>(x =>
                        x.HasValue ? x == (BusinessAccountType) query.Filter.BusinessAccountType : x == null),
                    It.IsAny<CancellationToken>()), Times.Once);
            _accessRepositoryMock.Verify(
                m => m.EvaluateVisibilityAsync(query.Principal,
                    It.Is<QueryParamsBusinessAccount>(o => 
                        o.Search == query.Filter.Search  &&
                        o.BusinessAccountType == query.Filter.BusinessAccountType &&
                        o.ContextId == query.Filter.ContextId
                        ), 0, int.MaxValue),
                Times.Once);
        }


        public class NotEligibleSearchTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput()
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = null
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "a"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
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
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "aaa"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "bbb"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ccc"
                    }
                };
                yield return new object[]
                {
                    new QueryParamsBusinessAccountInput
                    {
                        Search = "ddd"
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}