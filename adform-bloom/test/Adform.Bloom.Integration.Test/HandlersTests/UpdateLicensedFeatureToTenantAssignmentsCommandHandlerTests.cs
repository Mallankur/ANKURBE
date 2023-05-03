using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.ExceptionHandling.Abstractions.Extensions;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests : IClassFixture<TestsFixture>
    {
        private readonly UpdateLicensedFeatureToTenantAssignmentsCommandHandler _handler;
        private readonly IAdminGraphRepository _repository;
        private readonly IAccessValidator _accessValidator;

        private readonly IVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>
            _visibilityProvider;

        public UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests(TestsFixture fixture)
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
            _accessValidator = new AccessValidator(
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter,
                adapter);
            _handler = new UpdateLicensedFeatureToTenantAssignmentsCommandHandler(
                fixture.GraphRepository,
                _accessValidator,
                mediator);
            _repository = fixture.GraphRepository;
            _visibilityProvider = fixture.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>();
        }

        [Fact, Order(1)]
        public async Task Handle_Assign_Returns_Expected_Result()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse[]
            {
                new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant0),
                    Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                },
                new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant3),
                    Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                },
            });

            var licensedFeatures =
                await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIdsAndPolicyTypes(),
                    0, 10);

            var licensedFeaturesIds = licensedFeatures.Data
                .Where(o => o.Id == Guid.Parse(Graph.LicensedFeature1))
                .Select(o => o.Id).ToList();

            var licensedFeaturesBeforeAssignment = await _repository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(Graph.Tenant3), Constants.AssignedLink);

            var traffickerRoleId = Guid.Parse(Graph.Trafficker3Role);

            var traffickerPermissionsBefore = (await _repository.GetConnectedAsync<Role, Permission>(o => o.Id == traffickerRoleId, Constants.ContainsLink)).Select(p => p.Id);

            var permissionIds = new List<Guid>
            {
                Guid.Parse(Graph.Permission1),
                Guid.Parse(Graph.Permission2),
                Guid.Parse(Graph.Permission3),
                Guid.Parse(Graph.Permission4)
            };

            // Act
            var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(principal,
                Guid.Parse(Graph.Tenant3),
                licensedFeaturesIds,
                null);
            await _handler.Handle(command, CancellationToken.None);


            //Assert
            var licensedFeaturesAfterAssignment = await _repository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(Graph.Tenant3), Constants.AssignedLink);
            Assert.Equal(licensedFeaturesIds.Count + licensedFeaturesBeforeAssignment.Count, licensedFeaturesAfterAssignment.Count);
            Assert.True(licensedFeaturesAfterAssignment.Select(p => p.Id).Except(licensedFeaturesBeforeAssignment.Select(p => p.Id))
                .OrderBy(o => o).SequenceEqual(licensedFeaturesIds.OrderBy(o => o)));
            
            var traffickerPermissionsAfter = (await _repository.GetConnectedAsync<Role, Permission>(o => o.Id == traffickerRoleId, Constants.ContainsLink)).Select(p => p.Id);
            var permissionDiff = traffickerPermissionsAfter.Except(traffickerPermissionsBefore).OrderBy(p=>p).ToList();
            Assert.Equal(permissionIds.Count, permissionDiff.Count);
            Assert.True(permissionDiff.SequenceEqual(permissionIds.OrderBy(p=>p)));
        }


        [Fact, Order(2)]
        public async Task Handle_Unassign_Returns_Expected_Result()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse[]
            {
                new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant0),
                    Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                },
                new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant1),
                    Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                },
            });

            var licensedFeatures =
                await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIdsAndPolicyTypes(),
                    0, 10);
            var licensedFeaturesIds = licensedFeatures.Data
                .Where(o => o.Id == Guid.Parse(Graph.LicensedFeature2))
                .Select(o => o.Id).ToList();

            var licensedFeaturesBeforeAssignment = await _repository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(Graph.Tenant1), Constants.AssignedLink);


            var traffickerRoleId = Guid.Parse(Graph.Trafficker1Role);

            var traffickerPermissionsBefore = (await _repository.GetConnectedAsync<Role, Permission>(o => o.Id == traffickerRoleId, Constants.ContainsLink)).Select(p => p.Id);

            var permissionIds = new List<Guid>
            {
                Guid.Parse(Graph.Permission5),
                Guid.Parse(Graph.Permission6),
                Guid.Parse(Graph.Permission7),
                Guid.Parse(Graph.Permission8)
            };

            // Act
            var command = new UpdateLicensedFeatureToTenantAssignmentsCommand(principal,
                Guid.Parse(Graph.Tenant1),
                null,
                licensedFeaturesIds);
            await _handler.Handle(command, CancellationToken.None);

            // Assert

            var licensedFeaturesAfterAssignment = await _repository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(Graph.Tenant1), Constants.AssignedLink);


            Assert.Equal(Math.Clamp(licensedFeaturesBeforeAssignment.Count - licensedFeaturesIds.Count, 0, 100),
                licensedFeaturesAfterAssignment.Count);
            Assert.True(licensedFeaturesBeforeAssignment.Except(licensedFeaturesAfterAssignment).OrderBy(o => o.Id).Select(i => i.Id)
                .SequenceEqual(licensedFeaturesIds.OrderBy(o => o)));

            var traffickerPermissionsAfter = (await _repository.GetConnectedAsync<Role, Permission>(o => o.Id == traffickerRoleId, Constants.ContainsLink)).Select(p => p.Id).ToList();
            var permissionsDiff = traffickerPermissionsBefore.Except(traffickerPermissionsAfter).OrderBy(p => p).ToList();
            Assert.Empty(traffickerPermissionsAfter);
            Assert.Equal(permissionIds.Count, permissionsDiff.Count);
            Assert.True(permissionsDiff.SequenceEqual(permissionIds.OrderBy(p=>p)));
        }


        [Theory, Order(0)]
        [MemberData(nameof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests.Data),
            MemberType = typeof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests))]
        public async Task Handle_Unassign_Throws_Exception_When_Doesnt_Have_Access(ErrorCodes errorCodes,
            Exception exception)
        {
            // Arrange
            var tenantId = Guid.Parse(Graph.Tenant8);
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = tenantId,
                Roles = new[] {Graph.OtherRole},
                Permissions = new[] {""}
            });

            var licensedFeatures =
                await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIdsAndPolicyTypes(),
                    0, 10);

            var licensedFeaturesIds = licensedFeatures.Data.Select(o => o.Id).ToList();

            switch (errorCodes)
            {
                case ErrorCodes.TenantDoesNotExist:
                    tenantId = Guid.NewGuid();
                    break;
                case ErrorCodes.LicensedFeaturesDoNotExist:
                    licensedFeaturesIds = new List<Guid>() {Guid.NewGuid()};
                    break;
                case ErrorCodes.SubjectCannotAccessTenant:
                    tenantId = Guid.Parse(Graph.Tenant0);
                    break;
            }

            // Act
            var command =
                new UpdateLicensedFeatureToTenantAssignmentsCommand(principal, tenantId, null, licensedFeaturesIds);
            var exceptionResult = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal(exceptionResult.GetReason(), exception.GetReason());
            Assert.Equal(exceptionResult.Message, exception.Message);
        }

        [Theory, Order(0)]
        [MemberData(nameof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests.Data),
            MemberType = typeof(UpdateLicensedFeatureToTenantAssignmentsCommandHandlerTests))]
        public async Task Handle_Assign_Throws_Exception_When_Doesnt_Have_Access(ErrorCodes errorCodes,
            Exception exception)
        {
            // Arrange
            var tenantId = Guid.Parse(Graph.Tenant8);
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = tenantId,
                Roles = new[] {Graph.OtherRole},
                Permissions = new[] {""}
            });

            var licensedFeatures =
                await _visibilityProvider.EvaluateVisibilityAsync(principal, new QueryParamsTenantIdsAndPolicyTypes(),
                    0, 10);

            var licensedFeaturesIds = licensedFeatures.Data.Select(o => o.Id).ToList();

            switch (errorCodes)
            {
                case ErrorCodes.TenantDoesNotExist:
                    tenantId = Guid.NewGuid();
                    break;
                case ErrorCodes.LicensedFeaturesDoNotExist:
                    licensedFeaturesIds = new List<Guid>() {Guid.NewGuid()};
                    break;
                case ErrorCodes.SubjectCannotAccessTenant:
                    tenantId = Guid.Parse(Graph.Tenant0);
                    break;
            }

            // Act
            var command =
                new UpdateLicensedFeatureToTenantAssignmentsCommand(principal, tenantId, licensedFeaturesIds, null);
            var exceptionResult = await Record.ExceptionAsync(() => _handler.Handle(command, CancellationToken.None));

            Assert.Equal(exceptionResult.GetReason(), exception.GetReason());
            Assert.Equal(exceptionResult.Message, exception.Message);
        }

        public static TheoryData<ErrorCodes, Exception> Data()
        {
            var data = new TheoryData<ErrorCodes, Exception>()
            {
                {
                    ErrorCodes.SubjectCannotAccessTenant,
                    new ForbiddenException(ErrorReasons.AccessControlValidationFailedReason,
                        ErrorMessages.SubjectCannotAccessTenant)
                },
                {
                    ErrorCodes.TenantDoesNotExist,
                    new NotFoundException(message: $"{typeof(Tenant).Name} not found.")
                },
                {
                    ErrorCodes.LicensedFeaturesDoNotExist,
                    new NotFoundException(message: $"{typeof(LicensedFeature).Name} not found.")
                }
            };

            return data;
        }
    }
}