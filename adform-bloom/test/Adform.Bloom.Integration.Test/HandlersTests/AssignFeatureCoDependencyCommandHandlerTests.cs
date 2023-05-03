using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class AssignFeatureCoDependencyCommandHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly AssignFeatureCoDependencyCommandHandler _handler;
        private readonly IAdminGraphRepository _repository;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> _visibilityRepository;

        public AssignFeatureCoDependencyCommandHandlerTests(TestsFixture fixture)
        {
            var mediator = new Mock<IMediator>().Object;
            var adapter = new ValidatorAdapter(
                fixture.GraphRepository,
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsRoles, Contracts.Output.Role>(),
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Subject>(),
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Permission>(),
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Feature>(),
                new Mock<IOptions<ValidationConfiguration>>().Object
            );
            var validator = new AccessValidator(
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter);
            _handler = new AssignFeatureCoDependencyCommandHandler(fixture.GraphRepository, mediator, validator);
            _repository = fixture.GraphRepository;
            _visibilityRepository = fixture.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>();
        }

        [Fact, Order(0)]
        public async Task Assign_Features_Succeeds()
        {
            // Arrange
            var principal =
                Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant0), TenantName = Graph.Tenant0Name, Roles = new[] {Graph.AdformAdmin},
                    Permissions = new[] {""}
                });
            var filter = new QueryParamsTenantIds();
            var features =
                (await _visibilityRepository.EvaluateVisibilityAsync(principal, filter, 0, 2)).Data
                .ToArray();
            var featureId = features[0].Id;
            var dependentOnId = features[1].Id;
            var request =
                new AssignFeatureCoDependencyCommand(principal, featureId, dependentOnId, LinkOperation.Assign);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            var result = await _repository.HasRelationshipAsync<Feature, Feature>(f => f.Id == featureId,
                d => d.Id == dependentOnId, Constants.DependsOnLink);
            Assert.True(result);
        }

        [Fact, Order(1)]
        public async Task Assign_Unassign_Features_Succeeds()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name,
                    Roles = new[] {Graph.AdformAdminRoleName},
                    Permissions = new[] {""}
                });
            var filter = new QueryParamsTenantIds();
            var features = (await _visibilityRepository.EvaluateVisibilityAsync(principal, filter, 0, 2)).Data
                .ToArray();
            var featureId = features[0].Id;
            var dependentOnId = features[1].Id;
            var request =
                new AssignFeatureCoDependencyCommand(principal, featureId, dependentOnId, LinkOperation.Assign);
            await _handler.Handle(request, CancellationToken.None);
            var result = await _repository.HasRelationshipAsync<Feature, Feature>(f => f.Id == featureId,
                d => d.Id == dependentOnId, Constants.DependsOnLink);
            Assert.True(result);
            request = new AssignFeatureCoDependencyCommand(principal, featureId, dependentOnId, LinkOperation.Unassign);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            result = await _repository.HasRelationshipAsync<Feature, Feature>(f => f.Id == featureId,
                d => d.Id == dependentOnId, Constants.DependsOnLink);
            Assert.False(result);
        }

        [Fact, Order(2)]
        public async Task Assign_Features_With_Circular_Dependency_Fails()
        {
            // Arrange
            var principal =
                Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4,
                    Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                    Permissions = new[] {""}
                });
            var filter = new QueryParamsTenantIds();
            var features = (await _visibilityRepository.EvaluateVisibilityAsync(principal, filter, 0, 3)).Data
                .ToArray();
            var featureId = features[0].Id;
            var dependentOnId = features[1].Id;
            var lastDependentOnId = features[2].Id;
            await _repository.CreateRelationshipAsync<Feature, Feature>(f => f.Id == featureId,
                d => d.Id == dependentOnId, Constants.DependsOnLink);
            await _repository.CreateRelationshipAsync<Feature, Feature>(f => f.Id == dependentOnId,
                d => d.Id == lastDependentOnId, Constants.DependsOnLink);

            var request =
                new AssignFeatureCoDependencyCommand(principal, lastDependentOnId, featureId, LinkOperation.Assign);

            // Act
            // Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _handler.Handle(request, CancellationToken.None));
        }
    }
}