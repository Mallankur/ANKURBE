using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Mappers;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class PolicyCommandHandlersTests : IClassFixture<TestsFixture>
    {
        private readonly CreateWithParentIdCommandHandler<CreatePolicyCommand, Policy> _createHandler;
        private readonly TestsFixture _fixture;

        public PolicyCommandHandlersTests(TestsFixture fixture)
        {
            _fixture = fixture;
            var mediator = new Mock<IMediator>().Object;
            _createHandler = new CreateWithParentIdCommandHandler<CreatePolicyCommand, Policy>(
                new NamedNodeMapper<CreatePolicyCommand, Policy>(), 
                fixture.GraphRepository, 
                mediator);
        }

        [Theory]
        [InlineData(Graph.Policy1)]
        [InlineData(Graph.Policy2)]
        [InlineData(Graph.Policy3)]
        public async Task CreatePolicyWithParentIdSucceedsWhenParentExist(Guid parentId)
        {
            // Arrange
            const string policyName = "PolicyX";

            // Act
            var created = await _createHandler.Handle(
                new CreatePolicyCommand(_fixture.BloomApiPrincipal[Graph.Subject0], parentId,
                    policyName), CancellationToken.None);
            var policyId = created.Id;

            // Assert
            var policy = await _fixture.GraphRepository.GetNodeAsync<Policy>(r => r.Id == policyId);
            Assert.StartsWith(policyName, policy.Name);

            var policies =
                await _fixture.GraphRepository.GetConnectedAsync<Policy, Policy>(r => r.Id == policyId,
                    Constants.ChildOfLink);
            Assert.Equal(parentId, policies.First().Id);
        }

        [Fact]
        public async Task CreatePolicyWithParentIdThrowsExceptionWhenParentDoesntExist()
        {
            // Arrange
            const string policyName = "PolicyY";
            var parentId = Guid.Empty;

            // Act
            await Assert.ThrowsAsync<NotFoundException>(() => _createHandler.Handle(
                new CreatePolicyCommand(_fixture.BloomApiPrincipal[Graph.Subject0], parentId,
                    policyName), CancellationToken.None));

            // Assert
            var policy = await _fixture.GraphRepository.GetNodeAsync<Policy>(r => r.Name == policyName);
            Assert.Null(policy);
        }

    }
}