using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Write.Commands;

namespace Adform.Bloom.Write.Services
{
    public interface INotificationService
    {
        Task SendEvents(UpdateSubjectAssignmentsCommand message, IEnumerable<RuntimeResponse> originalState,
            IEnumerable<RuntimeResponse> newState, CancellationToken cancellationToken = default);
        Task SendNotifications(UpdateSubjectAssignmentsCommand message, IEnumerable<RuntimeResponse> originalState,
            IEnumerable<RuntimeResponse> newState, IEnumerable<RuntimeResponse> fullState, CancellationToken cancellationToken = default);
    }
}