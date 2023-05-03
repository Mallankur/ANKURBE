using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class BaseCreateCommandHandlerTests : BaseTests
    {
        public BaseCreateCommandHandlerTests()
        {
            _handler = new BaseCreateCommandHandler<TestCreateCommand, TestEntity>(
                new NamedNodeMapper<TestCreateCommand, TestEntity>(), 
                _adminGraphRepositoryMock.Object,
                _mediatorMock.Object);
        }

        private readonly BaseCreateCommandHandler<TestCreateCommand, TestEntity> _handler;

        [Fact]
        public async Task Handle_Creates_Item()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal);

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateItem(res, entityToBeCreated);
        }

        [Fact]
        public async Task Handle_Does_Not_Send_Audit_Event_In_Case_Of_Error()
        {
            var (_, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal, true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Send_Audit_Event()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal);

            await _handler.Handle(cmd, CancellationToken.None);

            _mediatorMock.AssertAuditEventWasPublished<TestEntity>(entityToBeCreated.Id, _claimsPrincipal,
                AuditOperation.Create);
        }
    }
}