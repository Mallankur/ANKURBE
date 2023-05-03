using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using MapsterMapper;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Read
{
    public class BaseAccessRangeQueryHandlerTests
    {
        private readonly Mock<IAccessProvider<TestDto, QueryParams, TestOutDto>> _accessRepositoryMock =
            new Mock<IAccessProvider<TestDto, QueryParams, TestOutDto>>();

        private readonly BaseAccessRangeQueryHandler<BaseAccessRangeQuery<TestDto, QueryParamsInput, TestOutDto>,
                TestDto, QueryParamsInput, QueryParams, TestOutDto>
            _handler;

        public BaseAccessRangeQueryHandlerTests()
        {
            var mapper = new Mapper();
            _handler =
                new BaseAccessRangeQueryHandler<BaseAccessRangeQuery<TestDto, QueryParamsInput, TestOutDto>,
                    TestDto, QueryParamsInput, QueryParams, TestOutDto>(
                    _accessRepositoryMock.Object, mapper);
        }

        private EntityPagination<TestOutDto> SetupSearchPaginationAsync(
            BaseAccessRangeQuery<TestDto, QueryParamsInput, TestOutDto> query)
        {
            var entities = new EntityPagination<TestOutDto>(query.Offset, query.Limit, 1510, new List<TestOutDto>
            {
                new TestOutDto
                {
                    Name = "aaa"
                },
                new TestOutDto
                {
                    Name = "bbb"
                }
            });

            _accessRepositoryMock.Setup(m => m.EvaluateAccessAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<TestDto>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<QueryParams>(),
                    It.IsAny<CancellationToken>()
                )
            ).ReturnsAsync(entities);

            return entities;
        }

        private void AssertSearchPaginationAsync(EntityPagination<TestOutDto> res,
            BaseAccessRangeQuery<TestDto, QueryParamsInput, TestOutDto> query,
            EntityPagination<TestOutDto> entities)
        {
            _accessRepositoryMock.Verify(m => m.EvaluateAccessAsync(
                It.IsAny<ClaimsPrincipal>(),
                query.Context,
                query.Offset,
                query.Limit,
                It.IsAny<QueryParams>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);

            Assert.Equal(entities.TotalItems, res.TotalItems);
            Assert.Equal(query.Limit, res.Limit);
            Assert.Equal(query.Offset, res.Offset);
            Assert.NotNull(res.Data);
            Assert.Equal(entities.Data.Count, res.Data.Count);
        }

        [Fact]
        public async Task Handle_Calls_EvaluateAsync_For_Query_And_Returns_Correct_Data()
        {
            var query = new BaseAccessRangeQuery<TestDto, QueryParamsInput, TestOutDto>(new ClaimsPrincipal(),
                new TestDto(),
                5, 10);

            var items = SetupSearchPaginationAsync(query);

            var res = await _handler.Handle(query, CancellationToken.None);

            AssertSearchPaginationAsync(res, query, items);
        }
    }
}