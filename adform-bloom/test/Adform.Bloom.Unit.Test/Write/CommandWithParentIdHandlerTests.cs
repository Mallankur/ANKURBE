using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.OngDb.Core.Interfaces;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Write.Mappers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class CommandWithParentIdHandlerTests : BaseTests
    {
        public CommandWithParentIdHandlerTests()
        {
            _handler = new CreateWithParentIdCommandHandler<TestCreateCommand, TestEntity>(
                new NamedNodeMapper<TestCreateCommand, TestEntity>(), 
                _adminGraphRepositoryMock.Object, 
                _mediatorMock.Object);
        }

        private readonly CreateWithParentIdCommandHandler<TestCreateCommand, TestEntity> _handler;

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task Handle_Does_Not_Send_Audit_Event_In_Case_Of_Error(bool createItemThrowsException,
            bool createLinkThrowsException)
        {
            var (_, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(
                _claimsPrincipal, createItemThrowsException, createLinkThrowsException);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _mediatorMock.AssertPublishAuditEventWasNotPublished();
        }

        [Fact]
        public async Task Handle_Creates_Item()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal);

            var res = await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateItem(res, entityToBeCreated);
        }

        [Fact]
        public async Task Handle_Creates_Link_If_ParentId_Is_Present_And_Parent_Exist()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal);

            await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.AssertCreateLink(entityToBeCreated, new TestEntity { Id = cmd.ParentId.Value },
                Constants.ChildOfLink);
        }

        [Fact]
        public async Task Handle_Does_Not_Create_Link_If_ParentId_Is_Present_And_Parent_Doesnt_Exist()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal, parentItemExist: false);

            await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Does_Not_Create_Link_If_ParentId_Is_Null()
        {
            var (_, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal, withParentId: false);

            await _handler.Handle(cmd, CancellationToken.None);

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Does_Not_Create_Link_In_Case_Of_Error()
        {
            var (_, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal, true);

            await Assert.ThrowsAsync<Exception>(async () => await _handler.Handle(cmd, CancellationToken.None));

            _adminGraphRepositoryMock.Verify(r => r.CreateRelationshipAsync(
                It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<Expression<Func<TestEntity, bool>>>(),
                It.IsAny<ILink>()), Times.Never);
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