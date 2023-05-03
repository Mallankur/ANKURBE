using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MapsterMapper;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Read
{
    public class SingleQueryHandlerTests
    {
        private readonly Mock<IAdminGraphRepository> _repositoryMock = new Mock<IAdminGraphRepository>();

        private readonly Mock<IVisibilityProvider<QueryParams, TestDto>> _accessRepositoryMock =
            new Mock<IVisibilityProvider<QueryParams, TestDto>>();

        private readonly BaseSingleQueryHandler<TestSingleQuery, QueryParamsInput, QueryParams, TestEntity, TestDto>
            _handler;

        public SingleQueryHandlerTests()
        {
            _handler = new BaseSingleQueryHandler<TestSingleQuery, QueryParamsInput, QueryParams, TestEntity, TestDto>(
                _repositoryMock.Object, _accessRepositoryMock.Object, new Mapper());
        }

        private void AssertGetNodeAsync(TestDto queryResult, TestEntity expectedEntity, TestDto expectedDto)
        {
            _repositoryMock.Verify(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()), Times.Once);
            _repositoryMock.Verify(r => r.GetNodeAsync(
                It.Is<Expression<Func<TestEntity, bool>>>(
                    expression => expression.Compile()(expectedEntity))), Times.Once);

            Assert.NotNull(queryResult);
            Assert.Equal(expectedDto.Id, queryResult.Id);
            Assert.Equal(expectedDto.Name, queryResult.Name);
        }

        [Fact]
        public async Task Handle_Calls_GetNode_For_Query_And_Returns_Correct_Data()
        {
            var query = new TestSingleQuery(new ClaimsPrincipal(), Guid.NewGuid());

            var entity = new TestEntity
            {
                Id = query.Id,
                Name = "aaa"
            };
            var dto = new TestDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
            _repositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(entity);
            _accessRepositoryMock
                .Setup(m => m.HasItemVisibilityAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), null))
                .ReturnsAsync(true);

            var res = await _handler.Handle(query, CancellationToken.None);

            AssertGetNodeAsync(res, entity, dto);
        }

        [Fact]
        public async Task Handle_Calls_HasAccess_For_Query()
        {
            var query = new TestSingleQuery(new ClaimsPrincipal(), Guid.NewGuid());

            var entity = new TestEntity
            {
                Id = query.Id,
                Name = "aaa"
            };
            _repositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(entity);
            _accessRepositoryMock
                .Setup(m => m.HasItemVisibilityAsync(
                    It.IsAny<ClaimsPrincipal>(), It.IsAny<Guid>(), null))
                .ReturnsAsync(true);

            await _handler.Handle(query, CancellationToken.None);

            _accessRepositoryMock.Verify(
                m => m.HasItemVisibilityAsync(query.Principal, It.Is<Guid>(p=>p==query.Id), null), Times.Once);
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_For_TenantQuery_In_The_Case_Of_Error()
        {
            var query = new TestSingleQuery(
                new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, DateTime.Now.ToString(CultureInfo.InvariantCulture))
                        })
                ),
                Guid.NewGuid()
            );

            _repositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Does_Not_Call_HasAccess_For_Query_When_Resource_NotFound()
        {
            var query = new TestSingleQuery(new ClaimsPrincipal(), Guid.NewGuid());

            _repositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync((TestEntity) null);
            _accessRepositoryMock
                .Setup(m => m.HasVisibilityAsync(It.IsAny<ClaimsPrincipal>(), 
                    It.IsAny<QueryParams>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<NotFoundException>(
                async () => await _handler.Handle(query, CancellationToken.None));

            _accessRepositoryMock.Verify(
                m => m.HasVisibilityAsync(query.Principal, 
                    It.IsAny<QueryParams>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Throws_NotFoundException_For_Query_If_Entity_Was_Not_Found()
        {
            var query = new TestSingleQuery(new ClaimsPrincipal(), Guid.NewGuid());

            await Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(
                query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_When_HasAccess_Returns_False_ForbiddenException_Is_Thrown()
        {
            var query = new TestSingleQuery(new ClaimsPrincipal(), Guid.NewGuid());

            var entity = new TestEntity
            {
                Id = query.Id,
                Name = "aaa"
            };
            _repositoryMock.Setup(r => r.GetNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>()))
                .ReturnsAsync(entity);
            _accessRepositoryMock
                .Setup(m => m.HasVisibilityAsync(It.IsAny<ClaimsPrincipal>(), 
                    It.IsAny<QueryParams>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(query, CancellationToken.None));

            _accessRepositoryMock.Verify(
                m => m.HasItemVisibilityAsync(query.Principal, It.IsAny<Guid>(), null), Times.Once);
        }
    }
}