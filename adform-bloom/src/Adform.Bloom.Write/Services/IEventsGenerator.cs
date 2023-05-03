using System;
using System.Collections.Generic;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Runtime.Contracts.Response;

namespace Adform.Bloom.Write.Services
{
    public interface IEventsGenerator
    {
        EventTuple GenerateSubjectAssignmentEvents(Guid subjectId, Guid actorId,
            IEnumerable<RuntimeResponse> originalState, IEnumerable<RuntimeResponse> newState,
            IReadOnlyCollection<AssetsReassignment>? assetsReassignments = null);

        IEnumerable<SubjectAssignmentsNotification> GenerateSubjectAssignmentsNotifications(Guid subjectId,
            Guid actorId, IEnumerable<RuntimeResponse> originalState, IEnumerable<RuntimeResponse> newState);

        SubjectAuthorizationResultChangedEvent GenerateSubjectAuthorizationChangedEvent(Guid subjectId, Guid actorId,
            IEnumerable<RuntimeResponse> newState);
    }
}