using Adform.Bloom.DataAccess;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Xunit;

namespace Adform.Bloom.Unit.Test
{
    public class GuardTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task ThrowIfNotFound_Does_Not_Throws_NotFoundException_If_Node_Was_Found(int count)
        {
            var repository = new Mock<IAdminGraphRepository>();
            repository.Setup(r => r.GetCountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(count);

            await repository.Object.ThrowIfNotFound<TestEntity>(Guid.NewGuid());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ThrowIfNotFound_Throws_NotFoundException_If_Node_Was_Not_Found(int count)
        {
            var repository = new Mock<IAdminGraphRepository>();
            repository.Setup(r => r.GetCountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(count);

            await Assert.ThrowsAsync<NotFoundException>(async () =>
                await repository.Object.ThrowIfNotFound<TestEntity>(Guid.NewGuid()));
        }

        [Fact]
        public async Task ThrowIfNotFound_Sets_Parameter()
        {
            var repository = new Mock<IAdminGraphRepository>();
            repository.Setup(r => r.GetCountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<string>(),
                It.IsAny<string>())).ReturnsAsync(0);
            var id = Guid.NewGuid();

            var exception = await Assert.ThrowsAsync<NotFoundException>(async () =>
                await repository.Object.ThrowIfNotFound<TestEntity>(id));
            
            Assert.Equal(id.ToString(), exception.Params["testEntity"]);
        }
    }
}