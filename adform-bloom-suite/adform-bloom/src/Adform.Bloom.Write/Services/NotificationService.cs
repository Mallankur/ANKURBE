using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Messages.Commands.AssetsReassignment;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.Kafka.Producer;
using Adform.Ciam.SharedKernel.Extensions;
using EasyNetQ;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Write.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IKafkaProducer<string, SubjectAuthorizationResultChangedEvent> _authorizationProducer;
        private readonly IKafkaProducer<string, SubjectAssignmentsNotification> _notificationProducer;
        private readonly IEventsGenerator _eventsGenerator;
        private readonly IBus _bus;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IKafkaProducer<string, SubjectAuthorizationResultChangedEvent> authorizationProducer,
            IKafkaProducer<string, SubjectAssignmentsNotification> notificationProducer,
            IEventsGenerator eventsGenerator,
            IBus bus,
            ILogger<NotificationService> logger)
        {
            _authorizationProducer = authorizationProducer;
            _notificationProducer = notificationProducer;
            _eventsGenerator = eventsGenerator;
            _bus = bus;
            _logger = logger;
        }

        public async Task SendEvents(UpdateSubjectAssignmentsCommand message,
            IEnumerable<RuntimeResponse> originalState, IEnumerable<RuntimeResponse> newState,
            CancellationToken cancellationToken = default)
        {
            var actorId = Guid.Parse(message.Principal.GetActorId()!);
            var events = _eventsGenerator.GenerateSubjectAssignmentEvents(message.SubjectId, actorId, originalState, newState, message.AssetsReassignments);

            await SendSubjectDisabledEvent(events.DisabledEvent, cancellationToken);
            await SendSubjectAssignedEvents(events.AssignedEvents, cancellationToken);
            await SendSubjectUnassignedEvents(events.UnassignedEvents, cancellationToken);
            await SendReassignUserAssetsCommands(events.ReassignUserAssetsCommands, cancellationToken);
        }

        public async Task SendNotifications(UpdateSubjectAssignmentsCommand message, IEnumerable<RuntimeResponse> originalState, 
            IEnumerable<RuntimeResponse> newState, 
            IEnumerable<RuntimeResponse> fullState,
            CancellationToken cancellationToken = default)
        {            
            var actorId = Guid.Parse(message.Principal.GetActorId()!);
            var notification = _eventsGenerator.GenerateSubjectAssignmentsNotifications(message.SubjectId, actorId, originalState, newState);
            await SendSubjectAssignmentsNotifications(notification, cancellationToken);
            var authorizationChangedNotification = _eventsGenerator.GenerateSubjectAuthorizationChangedEvent(message.SubjectId, actorId, fullState);
            await SendSubjectAuthorizationResultChangedEvent(authorizationChangedNotification, cancellationToken);
        }

        private async Task SendSubjectAssignmentsNotifications(IEnumerable<SubjectAssignmentsNotification> newStates, CancellationToken cancellationToken)
        {
            foreach (var newState in newStates)
            {
                await _notificationProducer.ProduceAsync(newState.EventId.ToString(), newState);
            }
        }
        
        private async Task SendSubjectAuthorizationResultChangedEvent(SubjectAuthorizationResultChangedEvent newState, CancellationToken cancellationToken)
        {
            await _authorizationProducer.ProduceAsync(newState.EventId.ToString(), newState);
        }

        private async Task SendSubjectDisabledEvent(SubjectDisabledEvent? message,
            CancellationToken cancellationToken = default)
        {
            if (message != null)
            {
                await _bus.PubSub.PublishAsync(message, cancellationToken);
                _logger.LogInformation("[{CorrelationId}] Event {EventName} has been published.",
                    message.CorrelationId, nameof(SubjectDisabledEvent));
            }
        }
        
        private async Task SendSubjectAssignedEvents(IEnumerable<SubjectAssignmentEvent> messages,
            CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
            {
                await _bus.PubSub.PublishAsync(message, cancellationToken);
                _logger.LogInformation("[{CorrelationId}] Event {EventName} has been published.",
                    message.CorrelationId, nameof(SubjectAssignmentEvent));
            }
        }
        
        private async Task SendSubjectUnassignedEvents(IEnumerable<SubjectUnassignedEvent> messages,
            CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
            {
                await _bus.PubSub.PublishAsync(message, cancellationToken);
                _logger.LogInformation("[{CorrelationId}] Event {EventName} has been published.",
                    message.CorrelationId, nameof(SubjectUnassignedEvent));
            }
        }

        private async Task SendReassignUserAssetsCommands(IEnumerable<ReassignUserAssetsCommand> messages,
            CancellationToken cancellationToken = default)
        {
            foreach (var message in messages)
            {
                await _bus.PubSub.PublishAsync(message, cancellationToken);
                _logger.LogInformation("[{CorrelationId}] Event {EventName} has been published.",
                    message.CorrelationId, nameof(ReassignUserAssetsCommand));
            }
        }
    }
}