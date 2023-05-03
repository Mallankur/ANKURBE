using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Moq;
using Neo4jClient.Extensions;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class AssignPermissionToFeatureCommandHandlerTests : IClassFixture<TestsFixture>
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());
        private readonly AssignPermissionToFeatureCommandHandler _handler;
        private readonly IAdminGraphRepository _repository;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> _visibilityProvider;

        public AssignPermissionToFeatureCommandHandlerTests(TestsFixture fixture)
        {
            var mediator = new Mock<IMediator>().Object;
            _handler = new AssignPermissionToFeatureCommandHandler(fixture.GraphRepository,
                fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Feature>(),
                mediator);
            _repository = fixture.GraphRepository;
            _visibilityProvider = fixture.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>();
        }

        [Fact]
        public async Task Assign_Permission_Which_Is_Already_Assigned_To_Other_Feature_Fails()
        {
            // Arrange
            var principal =
                Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant8), TenantName = Graph.Tenant8Name,
                    Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                    Permissions = new[] {""}
                });
            var features =
                (await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIds(), 0, 10)).Data
                .Select(y => y.Id);
            var permissions =
                await _repository.GetConnectedAsync<Feature, Permission>(x => x.Id.In(features),
                    Constants.ContainsLink);
            var permission = permissions[Random.Next(0, permissions.Count - 1)];
            var feature =
                (await _repository.GetConnectedAsync<Permission, Feature>(x => x.Id == permission.Id,
                    Constants.ContainsIncomingLink)).First();
            var request =
                new AssignPermissionToFeatureCommand(principal, feature.Id, permission.Id, LinkOperation.Assign);

            // Act
            // Assert
            await Assert.ThrowsAsync<BadRequestException>(async () =>
                await _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Assign_Permission_To_Feature_That_Principal_Doesnt_Have_Access_To_Fails()
        {
            // Arrange
            var principal =
                Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant8), TenantName = Graph.Tenant8, Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                });
            var features =
                (await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIds(), 0, 10)).Data
                .Select(y => y.Id);
            var permissions =
                await _repository.GetConnectedAsync<Feature, Permission>(x => x.Id.NotIn(features),
                    Constants.ContainsLink);
            var permission = permissions[Random.Next(0, permissions.Count - 1)];
            var feature =
                (await _repository.GetConnectedAsync<Permission, Feature>(x => x.Id == permission.Id,
                    Constants.ContainsIncomingLink)).First();
            var request =
                new AssignPermissionToFeatureCommand(principal, feature.Id, permission.Id, LinkOperation.Assign);

            // Act
            // Assert
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
                await _handler.Handle(request, CancellationToken.None));
        }
    }
}