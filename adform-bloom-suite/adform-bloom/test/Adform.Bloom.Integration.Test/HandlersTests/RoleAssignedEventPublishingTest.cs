using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Adapters;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Domain.Validations;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Handlers;
using Adform.Bloom.Write.PostProcessors;
using Adform.Bloom.Write.Services;
using Adform.Ciam.Kafka.Producer;
using Adform.Ciam.RabbitMQ;
using Adform.Ciam.RabbitMQ.Configuration;
using Adform.Ciam.SharedKernel.Extensions;
using Adform.Ciam.SharedKernel.Services;
using CorrelationId;
using CorrelationId.Abstractions;
using EasyNetQ;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AssetsReassignment = Adform.Bloom.Contracts.Input.AssetsReassignment;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using Permission = Adform.Bloom.Contracts.Output.Permission;
using Role = Adform.Bloom.Contracts.Output.Role;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [Collection(nameof(HandlersCollection))]
    public class RoleAssignedEventPublishingTest : IClassFixture<TestsFixture>
    {
        private readonly TestsFixture _fixture;
        private readonly UpdateSubjectAssignmentsCommandHandler _updateHandler;
        private readonly UpdateSubjectAssignmentsCommandPostProcessor _postProcessor;
        private readonly IBus _bus;

        public RoleAssignedEventPublishingTest(TestsFixture fixture)
        {
            _fixture = fixture;
            var adapter = new ValidatorAdapter(
                _fixture.GraphRepository,
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsRoles, Role>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Subject>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Permission>(),
                _fixture.VisibilityRepositoriesContainer.Get<QueryParamsTenantIds, Feature>(),
                Options.Create(new ValidationConfiguration { RoleLimitPerSubject = int.MaxValue })
            );
            var mediator = new Mock<IMediator>().Object;
            var cacheManager = _fixture.CacheManager;
            var validator = new AccessValidator(adapter, adapter, adapter, adapter, adapter, adapter, adapter);
            var client = _fixture.Identities.GetBloomRuntimeClient();

            var producer = new Mock<IKafkaProducer<string, SubjectAuthorizationResultChangedEvent>>().Object;
            var notificationProducer = new Mock<IKafkaProducer<string, SubjectAssignmentsNotification>>().Object;

            var rabbitConfig = fixture.Configuration.GetSection(Paths.Configuration)
                .Get<RabbitMQConfiguration>();

            var timestamp = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();
            var dateTimeProvider = new Mock<IDateTimeProvider>();
            var correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
            dateTimeProvider
                .Setup(p => p.UtcNowWithOffset)
                .Returns(timestamp);
            correlationContextAccessor
                .Setup(p => p.CorrelationContext)
                .Returns(new CorrelationContext(correlationId.ToString(), "CorrelationId"));
            _bus = RabbitHutch.CreateBus(rabbitConfig.ConnectionString);
            var generator = new EventsGenerator(dateTimeProvider.Object, correlationContextAccessor.Object);
            var notificationService = new NotificationService(producer, notificationProducer, generator, _bus,
                NullLogger<NotificationService>.Instance);
            _updateHandler = new UpdateSubjectAssignmentsCommandHandler(_fixture.GraphRepository,
                mediator,
                validator,
                client,
                NullLogger<UpdateSubjectAssignmentsCommandHandler>.Instance);
            _postProcessor = new UpdateSubjectAssignmentsCommandPostProcessor(notificationService, cacheManager, client);
        }


        [Fact]
        public async Task Validate_If_RoleAssignedEvent_Is_Published_And_Can_Be_Consumed()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant0),
                Roles = new[] { Graph.AdformAdminRoleName }
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var consumer = new TestSubjectAssignedEventConsumer(1);
            // Act
            using (await _bus.PubSub.SubscribeAsync<SubjectAssignmentEvent>(Guid.NewGuid().ToString(), async ev =>
                       await consumer.ConsumeAsync(ev, cts.Token), cts.Token))
            {
                var request = new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject10), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole19),
                        TenantId = Guid.Parse(Graph.Tenant15)
                    },
                });
                var originalState = await _updateHandler.Handle(request, cts.Token);
                await _postProcessor.Process(request, originalState, cts.Token);
                
                await consumer.MessageSink.WaitAllReceivedAsync(cts.Token);

                // Assert
                await Task.Delay(1000);

                var message = consumer.MessageSink.ReceivedMessages.FirstOrDefault();
                Assert.NotNull(message);
                Assert.Contains(Graph.Permission13Name, message.Permissions);
                Assert.Contains(Graph.Permission14Name, message.Permissions);
                Assert.Equal(Guid.Parse(Graph.Subject10), message.SubjectId);
                Assert.Equal(Guid.Parse(Graph.Tenant15), message.TenantId);
                Assert.Equal(Graph.Tenant15Name, message.TenantName);
                Assert.Equal(Guid.Parse(principal.GetActorId()), message.ActorId);
                Assert.Equal(Guid.Parse(principal.GetActorId()), message.ActorId);
                Assert.Equal(15, message.TenantLegacyId);
                Assert.Equal(BusinessAccountType.DataProvider.ToString(), message.TenantType);
            }
        }

        [Fact]
        public async Task Validate_If_Two_RoleAssignedEvents_Are_Published_And_Can_Be_Consumed()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant0),
                Roles = new[] { Graph.AdformAdminRoleName }
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var consumer = new TestSubjectAssignedEventConsumer(2);
            // Act
            using (await _bus.PubSub.SubscribeAsync<SubjectAssignmentEvent>(Guid.NewGuid().ToString(), async ev =>
                       await consumer.ConsumeAsync(ev, cts.Token), cts.Token))
            {
                var request = new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject10), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole7),
                        TenantId = Guid.Parse(Graph.Tenant6)
                    },
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole8),
                        TenantId = Guid.Parse(Graph.Tenant7)
                    }
                });
                var originalState = await _updateHandler.Handle(request, cts.Token);
                await _postProcessor.Process(request, originalState, cts.Token);

                await consumer.MessageSink.WaitAllReceivedAsync(cts.Token);

                // Assert
                await Task.Delay(1000);

                var message = consumer.MessageSink.ReceivedMessages.OrderBy(p => p.TenantLegacyId).ToArray();
                Assert.Equal(2, message.Count());
                Assert.Equal(Guid.Parse(Graph.Subject10), message[0].SubjectId);
                Assert.Equal(Guid.Parse(Graph.Subject10), message[1].SubjectId);
                Assert.Contains(Graph.Permission5Name, message[0].Permissions);
                Assert.Contains(Graph.Permission6Name, message[0].Permissions);
                Assert.Contains(Graph.Permission7Name, message[1].Permissions);
                Assert.Contains(Graph.Permission8Name, message[1].Permissions);
                Assert.Equal(new[] { Guid.Parse(Graph.Tenant6), Guid.Parse(Graph.Tenant7) }.OrderBy(x => x),
                    message.Select(x => x.TenantId).OrderBy(x => x));
                Assert.Equal(Guid.Parse(principal.GetActorId()), message[0].ActorId);
                Assert.Equal(Guid.Parse(principal.GetActorId()), message[1].ActorId);
                Assert.Equal(new[] { 6, 7 }.OrderBy(x => x),
                    message.Select(x => x.TenantLegacyId).OrderBy(x => x));
                Assert.Equal(BusinessAccountType.DataProvider.ToString(), message[0].TenantType);
                Assert.Equal(BusinessAccountType.Agency.ToString(), message[1].TenantType);
            }
        }

        [Fact]
        public async Task Validate_If_ReassignUserAssetsCommand_Is_Published_And_Can_Be_Consumed()
        {
            // Arrange
            var principal = Common.Test.Common.BuildUser(new RuntimeResponse
            {
                TenantId = Guid.Parse(Graph.Tenant0),
                Roles = new[] { ClaimPrincipalExtensions.AdformAdmin }
            });

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            var commandConsumer = new TestReassignUserAssetsCommandConsumer(2);
            var eventConsumer = new TestSubjectAssignedEventConsumer(2);
            using (await _bus.PubSub.SubscribeAsync<SubjectAssignmentEvent>(Guid.NewGuid().ToString(), async ev =>
                       await eventConsumer.ConsumeAsync(ev, cts.Token), cts.Token))
            {
                var arrangeRequest = new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole12),
                        TenantId = Guid.Parse(Graph.Tenant11)
                    },
                    new RoleTenant
                    {
                        RoleId = Guid.Parse(Graph.CustomRole13),
                        TenantId = Guid.Parse(Graph.Tenant4)
                    }
                });
                var arrangeOriginalState = await _updateHandler.Handle(arrangeRequest, cts.Token);
                await _postProcessor.Process(arrangeRequest, arrangeOriginalState, cts.Token);
                
                await eventConsumer.MessageSink.WaitAllReceivedAsync(cts.Token);
                // Act
                using (await _bus.PubSub.SubscribeAsync<ReassignUserAssetsCommand>(Guid.NewGuid().ToString(),
                           async ev =>
                               await commandConsumer.ConsumeAsync(ev, cts.Token), cts.Token))
                {
                    var actRequest = new UpdateSubjectAssignmentsCommand(principal, Guid.Parse(Graph.Subject2), null,
                        new[]
                        {
                            new RoleTenant
                            {
                                RoleId = Guid.Parse(Graph.CustomRole12),
                                TenantId = Guid.Parse(Graph.Tenant11)
                            },
                            new RoleTenant
                            {
                                RoleId = Guid.Parse(Graph.CustomRole6),
                                TenantId = Guid.Parse(Graph.Tenant2)
                            }
                        }, new[]
                        {
                            new AssetsReassignment
                            {
                                BusinessAccountType = BusinessAccountType.Publisher,
                                LegacyBusinessAccountId = 11,
                                NewUserId = Guid.Parse(Graph.Subject1)
                            },
                            new AssetsReassignment
                            {
                                BusinessAccountType = BusinessAccountType.Agency,
                                LegacyBusinessAccountId = 2,
                                NewUserId = Guid.Parse(Graph.Subject1)
                            }
                        });
                    var actOriginalState = await _updateHandler.Handle(actRequest,cts.Token);
                    await _postProcessor.Process(actRequest, actOriginalState, cts.Token);
                    
                    await commandConsumer.MessageSink.WaitAllReceivedAsync(cts.Token);

                    // Assert
                    await Task.Delay(1000);

                    var message = commandConsumer.MessageSink.ReceivedMessages;
                    Assert.Equal(2, message.Count);
                    Assert.Equal(Guid.Parse(Graph.Subject2), message[0].CurrentUserId);
                    Assert.Equal(Guid.Parse(Graph.Subject2), message[1].CurrentUserId);
                    Assert.Equal(new[] { 2, 11 }.OrderBy(x => x),
                        message.SelectMany(x => x.AssetsReassignments.Select(y => y.LegacyBusinessAccountId))
                            .OrderBy(x => x));
                    Assert.Equal(new[] { Guid.Parse(Graph.Subject1), Guid.Parse(Graph.Subject1) }.OrderBy(x => x),
                        message.SelectMany(x => x.AssetsReassignments.Select(y => y.NewUserId))
                            .OrderBy(x => x));
                    Assert.True(message.OfType<ReassignUserAgenciesAssetsCommand>().Any());
                    Assert.True(message.OfType<ReassignUserPublishersAssetsCommand>().Any());
                }
            }
        }
    }
}