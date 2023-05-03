using System;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Mediatr.Extensions;
using Adform.Bloom.Unit.Test.MediatrExtensions.Structures;
using MediatR;
using Moq;
using Neo4jClient.Transactions;
using Xunit;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Transactions
{
    public class UnitOfWorkBehaviorTests
    {
        [Fact]
        public async Task Error_Flow()
        {
            var uow = new Mock<ITransaction>();
            var client = new Mock<ITransactionalGraphClient>();
            client.Setup(x => x.BeginTransaction()).Returns(uow.Object);
            var next = new Mock<RequestHandlerDelegate<FakeResponse>>();
            next.Setup(n => n.Invoke()).ThrowsAsync(new Exception());
            var b = new UnitOfWorkBehavior<FakeRequest, FakeResponse>(client.Object);

            await Assert.ThrowsAsync<Exception>(
                async () => await b.Handle(new FakeRequest(), CancellationToken.None, next.Object));
            client.Verify(x => x.BeginTransaction());
            uow.Verify(u => u.CommitAsync(), Times.Never);
            uow.Verify(u => u.RollbackAsync(), Times.Once);
            next.Verify(n => n.Invoke(), Times.Once);
        }

        [Fact]
        public async Task Normal_Flow()
        {
            var uow = new Mock<ITransaction>();
            var client = new Mock<ITransactionalGraphClient>();
            client.Setup(x => x.BeginTransaction()).Returns(uow.Object);
            var response = new FakeResponse();
            var next = new Mock<RequestHandlerDelegate<FakeResponse>>();
            next.Setup(n => n.Invoke()).ReturnsAsync(response);
            var b = new UnitOfWorkBehavior<FakeRequest, FakeResponse>(client.Object);

            var res = await b.Handle(new FakeRequest(), CancellationToken.None, next.Object);

            Assert.NotNull(res);
            Assert.Same(response, res);
            client.Verify(x => x.BeginTransaction());
            uow.Verify(u => u.CommitAsync(), Times.Once);
            uow.Verify(u => u.RollbackAsync(), Times.Never);
            next.Verify(n => n.Invoke(), Times.Once);
        }

        [Fact]
        public async Task Request_Cancelled_Flow()
        {
            var uow = new Mock<ITransaction>();
            var client = new Mock<ITransactionalGraphClient>();
            client.Setup(x => x.BeginTransaction()).Returns(uow.Object);
            var response = new FakeResponse();
            var next = new Mock<RequestHandlerDelegate<FakeResponse>>();
            next.Setup(n => n.Invoke()).ReturnsAsync(response);
            var b = new UnitOfWorkBehavior<FakeRequest, FakeResponse>(client.Object);
            var cts = new CancellationTokenSource(0);

            var res = await b.Handle(new FakeRequest(), cts.Token, next.Object);

            Assert.NotNull(res);
            Assert.Same(response, res);
            client.Verify(x => x.BeginTransaction());
            uow.Verify(u => u.CommitAsync(), Times.Never);
            uow.Verify(u => u.RollbackAsync(), Times.Once);
            next.Verify(n => n.Invoke(), Times.Once);
        }
    }
}