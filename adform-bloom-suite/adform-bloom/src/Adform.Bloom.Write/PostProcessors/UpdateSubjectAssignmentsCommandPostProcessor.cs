using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Services;
using MediatR.Pipeline;

namespace Adform.Bloom.Write.PostProcessors
{
    public class UpdateSubjectAssignmentsCommandPostProcessor : IRequestPostProcessor<UpdateSubjectAssignmentsCommand,
        IEnumerable<RuntimeResponse>>
    {
        private readonly INotificationService _notificationService;
        private readonly IBloomCacheManager _cache;
        private readonly IBloomRuntimeClient _bloomRuntimeClient;


        public UpdateSubjectAssignmentsCommandPostProcessor(INotificationService notificationService,
            IBloomCacheManager cache, IBloomRuntimeClient bloomRuntimeClient)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _bloomRuntimeClient = bloomRuntimeClient ?? throw new ArgumentNullException(nameof(bloomRuntimeClient));
        }

        public async Task Process(UpdateSubjectAssignmentsCommand request, IEnumerable<RuntimeResponse> originalState,
            CancellationToken cancellationToken)
        {
            var changedState = await RefreshCache(request, cancellationToken);
            await SendNotification(request, originalState, changedState.Direct, changedState.Full,
                cancellationToken);
        }

        private async Task SendNotification(UpdateSubjectAssignmentsCommand request,
            IEnumerable<RuntimeResponse> originalState,
            IEnumerable<RuntimeResponse> changedState,
            IEnumerable<RuntimeResponse> fullState, CancellationToken cancellationToken)
        {
            await _notificationService.SendEvents(request, originalState, changedState, cancellationToken);
            await _notificationService.SendNotifications(request, originalState, changedState, fullState,
                cancellationToken);
        }

        private async Task<(IEnumerable<RuntimeResponse> Direct, IEnumerable<RuntimeResponse> Full)> RefreshCache(
            UpdateSubjectAssignmentsCommand command,
            CancellationToken cancellationToken)
        {
            var request = new SubjectRuntimeRequest
            {
                SubjectId = command.SubjectId,
                InheritanceEnabled = false
            };
            await _cache.ForgetBySubjectAsync(request.SubjectId.ToString(), cancellationToken);
            var direct = await _bloomRuntimeClient.InvokeAsync(request, cancellationToken);
            request.InheritanceEnabled = true;
            var updatedAuthorization = await _bloomRuntimeClient.InvokeAsync(request, cancellationToken);
            return (direct, updatedAuthorization);
        }
    }
}