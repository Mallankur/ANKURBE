using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Services;
using Adform.Ciam.Kafka.Producer;
using Adform.Ciam.SharedKernel.Services;
using CorrelationId;
using CorrelationId.Abstractions;
using EasyNetQ;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Adform.Bloom.Unit.Test.Write
{
    public class NotificationServiceTests
    {
        private readonly Mock<IKafkaProducer<string, SubjectAssignmentsNotification>> _producerNotification;

        private readonly Mock<IKafkaProducer<string, SubjectAuthorizationResultChangedEvent>>
            _producerAuthorizationChange;

        private readonly Mock<ICorrelationContextAccessor> _correlationContextAccessor;
        private readonly Mock<IDateTimeProvider> _dateTimeProvider;
        private readonly Mock<IEventsGenerator> _eventsGenerator;
        private readonly Mock<IBus> _bus;
        private readonly Mock<IPubSub> _pubSub;

        public NotificationServiceTests()
        {
            _bus = new Mock<IBus>();
            _pubSub = new Mock<IPubSub>();
            _bus.SetupGet(b => b.PubSub).Returns(_pubSub.Object);
            _producerNotification = new Mock<IKafkaProducer<string, SubjectAssignmentsNotification>>();
            _eventsGenerator = new Mock<IEventsGenerator>();
            _producerAuthorizationChange = new Mock<IKafkaProducer<string, SubjectAuthorizationResultChangedEvent>>();
            _dateTimeProvider = new Mock<IDateTimeProvider>();
            _correlationContextAccessor = new Mock<ICorrelationContextAccessor>();
            _dateTimeProvider
                .Setup(p => p.UtcNowWithOffset)
                .Returns(DateTimeOffset.UtcNow);
            _correlationContextAccessor
                .Setup(p => p.CorrelationContext)
                .Returns(new CorrelationContext(Guid.NewGuid().ToString(), "CorrelationId"));
        }

        [Fact]
        public async Task
            SendNotification_Produces_SubjectAuthorizationResultChangedEvent_And_SubjectAssignmentsNotification()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var tenantLegacyId = 0;
            var tenantName = "tenant";
            var tenantType = BusinessAccountType.Adform;
            var role = "role";
            var permission = "permission";

            var originalState = new List<RuntimeResponse>();
            var newState = new List<RuntimeResponse>
            {
                new RuntimeResponse
                {
                    TenantId = tenantId,
                    TenantName = tenantName,
                    TenantLegacyId = tenantLegacyId,
                    TenantType = tenantType.ToString(),
                    Permissions = new List<string> {role},
                    Roles = new List<string> {permission},
                }
            };

            var command = new UpdateSubjectAssignmentsCommand(Common.BuildActorPrincipal(actorId), subjectId,
                new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Empty,
                        TenantId = tenantId
                    }
                }, null);
            var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);
            var notificationService = new NotificationService(_producerAuthorizationChange.Object,
                _producerNotification.Object, eventsGenerator, _bus.Object,
                NullLogger<NotificationService>.Instance);

            // Act
            await notificationService.SendNotifications(command, originalState, newState, newState);

            // Assert
            _producerNotification.Verify(
                p =>
                    p.ProduceAsync(It.IsAny<string>(),
                        It.IsAny<SubjectAssignmentsNotification>()), Times.Once);
            _producerAuthorizationChange.Verify(
                p =>
                    p.ProduceAsync(It.IsAny<string>(),
                        It.IsAny<SubjectAuthorizationResultChangedEvent>()), Times.Once);
        }


        [Fact]
        public async Task SendEvents_Produces_AssignEvent()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var tenantLegacyId = 0;
            var tenantName = "tenant";
            var tenantType = BusinessAccountType.Adform;
            var role = "role";
            var permission = "permission";

            var originalState = new List<RuntimeResponse>();
            var newState = new List<RuntimeResponse>
            {
                new RuntimeResponse
                {
                    TenantId = tenantId,
                    TenantName = tenantName,
                    TenantLegacyId = tenantLegacyId,
                    TenantType = tenantType.ToString(),
                    Permissions = new List<string> {role},
                    Roles = new List<string> {permission},
                }
            };

            var command = new UpdateSubjectAssignmentsCommand(Common.BuildActorPrincipal(actorId), subjectId,
                new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Empty,
                        TenantId = tenantId
                    }
                }, null);
            var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);
            var notificationService = new NotificationService(_producerAuthorizationChange.Object,
                _producerNotification.Object, eventsGenerator, _bus.Object,
                NullLogger<NotificationService>.Instance);

            // Act
            await notificationService.SendEvents(command, originalState, newState);

            // Assert
            _pubSub.Verify(
                p => p.PublishAsync(It.Is((SubjectAssignmentEvent e) => e.Permissions.All(p => newState.First().Permissions.Contains(p))), It.IsAny<Action<IPublishConfiguration>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendEvents_Produces_UnassignEvent()
        {
            // Arrange
            var actorId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var tenantId0 = Guid.NewGuid();
            var tenantId1 = Guid.NewGuid();
            var tenantLegacyId0 = 0;
            var tenantLegacyId1 = 1;
            var tenantName0 = "tenant0";
            var tenantName1 = "tenant1";
            var tenantType0 = BusinessAccountType.Adform;
            var tenantType1 = BusinessAccountType.Agency;
            var role = "role";
            var permission = "permission";

            var originalState = new List<RuntimeResponse>
            {
                new RuntimeResponse
                {
                    TenantId = tenantId0,
                    TenantName = tenantName0,
                    TenantLegacyId = tenantLegacyId0,
                    TenantType = tenantType0.ToString(),
                    Permissions = new List<string> {role},
                    Roles = new List<string> {permission},
                },
                new RuntimeResponse
                {
                    TenantId = tenantId1,
                    TenantName = tenantName1,
                    TenantLegacyId = tenantLegacyId1,
                    TenantType = tenantType1.ToString(),
                    Permissions = new List<string> {role},
                    Roles = new List<string> {permission},
                }
            };
            var newState = new List<RuntimeResponse>
            {
                new RuntimeResponse
                {
                    TenantId = tenantId0,
                    TenantName = tenantName0,
                    TenantLegacyId = tenantLegacyId0,
                    TenantType = tenantType0.ToString(),
                    Permissions = new List<string> {role},
                    Roles = new List<string> {permission},
                }
            };

            var command = new UpdateSubjectAssignmentsCommand(Common.BuildActorPrincipal(actorId), subjectId, null,
                new[]
                {
                    new RoleTenant
                    {
                        RoleId = Guid.Empty,
                        TenantId = tenantId1
                    }
                });
            var eventsGenerator = new EventsGenerator(_dateTimeProvider.Object, _correlationContextAccessor.Object);
            var notificationService = new NotificationService(_producerAuthorizationChange.Object,
                _producerNotification.Object, eventsGenerator, _bus.Object,
                NullLogger<NotificationService>.Instance);

            // Act
            await notificationService.SendEvents(command, originalState, newState);

            // Assert
            _pubSub.Verify(
                p => p.PublishAsync(It.Is((SubjectUnassignedEvent e) => e.Permissions.All(p => newState.First().Permissions.Contains(p))), It.IsAny<Action<IPublishConfiguration>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
            _pubSub.Verify(
                p => p.PublishAsync(It.IsAny<ReassignUserAssetsCommand>(), It.IsAny<Action<IPublishConfiguration>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}