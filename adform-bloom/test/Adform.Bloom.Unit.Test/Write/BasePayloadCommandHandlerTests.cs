using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class BasePayloadCommandHandlerTests : BaseTests
    {
        private readonly BasePayloadValidationCommandHandler<TestCreateCommand, TestEntity> _handler;

        public BasePayloadCommandHandlerTests()
        {
            _handler = new Handler(
                _adminGraphRepositoryMock.Object, _mediatorMock.Object);
        }

        [Fact]
        public async Task BasePayloadCommandHandlerTests_With_Valid_Payload_Doesnt_Throw_Exception()
        {
            var (entityToBeCreated, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal);
            var res = await _handler.Handle(cmd, CancellationToken.None);
            
            Assert.Equal(res.Name, entityToBeCreated.Name);
        }

        [Theory]
        [MemberData(nameof(GenerateInvalidPayloads))]
        public async Task BasePayloadCommandHandlerTests_With_Invalid_Payload_Throws_Exception(TestEntity entity)
        {
            var (_, cmd) = _adminGraphRepositoryMock.SetupTestCreateCommand(_claimsPrincipal, initialEntity: entity);
            await Assert.ThrowsAsync<BadRequestException>(
                async () => await _handler.Handle(cmd, CancellationToken.None));
        }

        public static TheoryData<TestEntity> GenerateInvalidPayloads() =>
            new TheoryData<TestEntity>
            {
                new TestEntity(""),
                new TestEntity("<script"),
                new TestEntity("%20"),
                new TestEntity("name") {Description = "<script"},
                new TestEntity("name") {Description = "aaa%20"}
            };

        public class Handler : BasePayloadValidationCommandHandler<TestCreateCommand, TestEntity>
        {
            public Handler(IAdminGraphRepository repository, IMediator mediator) : base(repository, mediator)
            {
            }

            protected override Task<TestEntity> HandleInternal(TestCreateCommand request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new TestEntity { Name = request.Name });
            }
        }
    }
}