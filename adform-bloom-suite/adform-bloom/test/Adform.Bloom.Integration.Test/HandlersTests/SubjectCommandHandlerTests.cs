using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Bloom.Write.Mappers;
using CorrelationId.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Bloom.Write.Services;
using Microsoft.Extensions.Options;
using Xunit;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class SubjectCommandHandlersTests : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        private readonly CreateSubjectCommandHandler _createHandler;
        private readonly UpdateSubjectAssignmentsCommandHandler _updateHandler;
        private readonly IOptions<ValidationConfiguration> _validationConfiguration;

        public SubjectCommandHandlersTests(TestsFixture fixture)
        {
            _fixture = fixture;
            _validationConfiguration = Options.Create(new ValidationConfiguration {RoleLimitPerSubject = int.MaxValue});
            var adapter = new ValidatorAdapter(
                _fixture.GraphRepository,
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsRoles,Contracts.Output.Role>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Subject>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Permission>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Contracts.Output.Feature>(),
                _validationConfiguration
            );
            var mediator = new Mock<IMediator>();
            var validator = new AccessValidator(adapter, adapter, adapter, adapter, adapter, adapter, adapter);
            var client = new Mock<IBloomRuntimeClient>().Object;
            var cacheManager = new Mock<IBloomCacheManager>().Object;
            var logger = new Mock<ILogger<UpdateSubjectAssignmentsCommandHandler>>().Object;
            var correlationId = new Mock<ICorrelationContextAccessor>().Object;
            var bus = new Mock<INotificationService>();

            _createHandler = new CreateSubjectCommandHandler(
                new SubjectMapper(),
                validator,
                _fixture.GraphRepository,
                mediator.Object,
                correlationId);
            
            _updateHandler = new UpdateSubjectAssignmentsCommandHandler(
                _fixture.GraphRepository,
                mediator.Object,
                validator,
                client,
                logger);

            mediator.Setup(m => m.Send(It.IsAny<UpdateSubjectAssignmentsCommand>(), It.IsAny<CancellationToken>()))
                .Returns(async (UpdateSubjectAssignmentsCommand q, CancellationToken token) => await _updateHandler.Handle(q, token));

        }

        [Fact]
        public async Task Create_Subject()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act
            var result = await _createHandler.Handle(
                new CreateSubjectCommand(_fixture.BloomApiPrincipal[Graph.Subject0], id, $"{id}@test", true),
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task Create_Subject_With_Assignments()
        {
            // Arrange
            var id = Guid.NewGuid();
            // Act
            var result = await _createHandler.Handle(
                new CreateSubjectCommand(_fixture.BloomApiPrincipal[Graph.Subject0], id, $"{id}@test", true,
                    roleTenantIds: new[]
                    {
                        new RoleTenant
                        {
                            RoleId = Guid.Parse(Graph.CustomRole10),
                            TenantId = Guid.Parse(Graph.Tenant9)
                        }
                    }),
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);

            var roles = await _fixture.GraphRepository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                x => x.Id == result.Id, Constants.MemberOfLink, Constants.AssignedLink);
            Assert.Single(roles);
            Assert.Equal(Graph.CustomRole10Name, roles.First().Name);
        }

        [Fact]
        public async Task Update_Subject_Assigns_Roles()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant8),
                Roles = new[] { Graph.OtherRole }
            });

            // Act
            await _updateHandler.Handle(
                new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject1), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole9),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    },
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole11),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    }
                }),
                CancellationToken.None);

            // Assert
            var roles = (await _fixture.GraphRepository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                    x => x.Id == Guid.Parse(Graph.Subject1), Constants.MemberOfLink, Constants.AssignedLink))
                .Select(x => x.Id);
            Assert.Contains(Guid.Parse(Graph.CustomRole0), roles);
        }

        [Fact]
        public async Task Update_Subject_Assigns_And_Unassigns_Roles()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant2),
                Roles = new[] { Graph.OtherRole }
            });
            await _updateHandler.Handle(
                new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole6),
                        TenantId = Guid.Parse(Graph.Tenant2)
                    }
                }),
                CancellationToken.None);

            // Act
            await _updateHandler.Handle(
                new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), null, new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole6),
                        TenantId = Guid.Parse(Graph.Tenant2)
                    }
                }),
                CancellationToken.None);

            // Assert
            var roles = (await _fixture.GraphRepository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                    x => x.Id == Guid.Parse(Graph.Subject0), Constants.MemberOfLink, Constants.AssignedLink))
                .Select(x => x.Id);
            Assert.DoesNotContain(roles,
                x => new[] { Guid.Parse(Graph.CustomRole0), Guid.Parse(Graph.CustomRole10) }.Contains(x));
        }

        [Fact]
        public async Task Update_Subject_AssignUnassing_With_Filtered_Assignments()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant8),
                Roles = new[] { Graph.OtherRole }
            });

            _validationConfiguration.Value.RoleLimitPerSubject = 1;

            await _updateHandler.Handle(
                new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole9),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    }
                }),
                CancellationToken.None);

            // Act
            await _updateHandler.Handle(
                new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole9),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    }
                }, new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole9),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    },
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole11),
                        TenantId = Guid.Parse(Graph.Tenant8)
                    }
                }),
                CancellationToken.None);

            // Assert
            var roles = (await _fixture.GraphRepository.GetConnectedWithIntermediateAsync<Subject, Group, Role>(
                    x => x.Id == Guid.Parse(Graph.Subject2), Constants.MemberOfLink, Constants.AssignedLink))
                .Select(x => x.Id);
            Assert.DoesNotContain(roles,
                x => new[] { Guid.Parse(Graph.CustomRole9), Guid.Parse(Graph.CustomRole11) }.Contains(x));
        }
    }
}