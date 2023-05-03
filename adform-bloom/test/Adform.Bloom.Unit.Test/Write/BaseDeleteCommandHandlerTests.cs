using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.SharedKernel.Extensions;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class BaseDeleteCommandHandlerTests : BaseTests
    {
        public BaseDeleteCommandHandlerTests()
        {
            _handler = new BaseDeleteCommandHandler<TestEntity>(_adminGraphRepositoryMock.Object, _mediatorMock.Object);
        }

        private readonly BaseDeleteCommandHandler<TestEntity> _handler;

        private void AssertAuditEvent(Guid idOfEntityToBeDeleted)
        {
            _mediatorMock.Verify(m => m.Publish(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _mediatorMock.Verify(m => m.Publish(
                    It.Is<AuditEvent>(e =>
                        e.EntityId == idOfEntityToBeDeleted.ToString() &&
                        e.EntityType == typeof(TestEntity).Name &&
                        e.Operation == AuditOperation.Delete.ToString() &&
                        e.Subject == _claimsPrincipal.GetSubId()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private void AssertDeleteNodeAsync(TestEntity entityToDelete)
        {
            _adminGraphRepositoryMock.Verify(
                r => r.DeleteNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), true), Times.Once);
            _adminGraphRepositoryMock.Verify(r => r.DeleteNodeAsync(
                It.Is<Expression<Func<TestEntity, bool>>>(
                    expression => expression.Compile()(entityToDelete)), true), Times.Once);
        }

        [Fact]
        public async Task Handle_Deletes_Link_And_Item()
        {
            var id = Guid.NewGuid();
            var entityToDelete = new TestEntity("aaa") {Id = id};
            var cmd = new TestDeleteCommand(_claimsPrincipal, id);

            await _handler.Handle(cmd, CancellationToken.None);

            AssertDeleteNodeAsync(entityToDelete);
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_If_It_Was_Not_Possible_To_Delete_Item()
        {
            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            _adminGraphRepositoryMock
                .Setup(r => r.DeleteNodeAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), true))
                .ThrowsAsync(new Exception());

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.Verify(
                m => m.Publish(It.IsAny<AuditEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_Send_Audit_Event()
        {
            var cmd = new TestDeleteCommand(_claimsPrincipal, Guid.NewGuid());

            await _handler.Handle(cmd, CancellationToken.None);

            AssertAuditEvent(cmd.IdOfEntityToDeleted);
        }
    }
}