using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Ciam.SharedKernel.Entities;
using MapsterMapper;
using MediatR;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Read
{
    public class RangeQueryHandlerTests
    {
        private readonly Mock<IVisibilityProvider<QueryParams, TestDto>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParams, TestDto>>();

        private readonly Mock<IMediator> _mediatorMock = new Mock<IMediator>();
        private readonly BaseRangeQueryHandler<TestRangeQuery, QueryParamsInput, QueryParams, TestDto> _handler;

        public RangeQueryHandlerTests()
        {
            _handler = new BaseRangeQueryHandler<TestRangeQuery, QueryParamsInput, QueryParams, TestDto>(
                _accessRepositoryMock.Object,
                new Mapper());
        }

        private EntityPagination<TestDto> SetupSearchPaginationAsync(TestRangeQuery query)
        {
            var entities = new EntityPagination<TestDto>(query.Offset, query.Limit, 1510, new List<TestDto>
            {
                new TestDto
                {
                    Name = "aaa"
                },
                new TestDto
                {
                    Name = "bbb"
                }
            });

            _accessRepositoryMock.Setup(m => m.EvaluateVisibilityAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<QueryParams>(),
                It.IsAny<int>(),
                It.IsAny<int>())
            ).ReturnsAsync(entities);

            return entities;
        }

        private void AssertSearchPaginationAsync(EntityPagination<TestDto> res,
            TestRangeQuery query,
            EntityPagination<TestDto> entities)
        {
            _mediatorMock.Verify(m => m.Publish(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Never);

            _accessRepositoryMock.Verify(m => m.EvaluateVisibilityAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<QueryParams>(),
                query.Offset,
                query.Limit), Times.Once);

            Assert.Equal(entities.TotalItems, res.TotalItems);
            Assert.Equal(query.Limit, res.Limit);
            Assert.Equal(query.Offset, res.Offset);
            Assert.NotNull(res.Data);
            Assert.Equal(entities.Data.Count, res.Data.Count);
        }

        [Fact]
        public async Task Handle_Calls_EvaluateAsync_For_Query_And_Returns_Correct_Data()
        {
            var query = new TestRangeQuery(new ClaimsPrincipal(),new QueryParamsInput(), 5, 10);

            var items = SetupSearchPaginationAsync(query);

            var res = await _handler.Handle(query, CancellationToken.None);

            AssertSearchPaginationAsync(res, query, items);
        }
    }
}