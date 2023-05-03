using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Queries;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Services;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Read.Handlers
{
    public class PermissionBusinessAccountRangeQueryHandler : IRequestHandler<PermissionBusinessAccountsQuery,
        IReadOnlyCollection<BusinessAccount>>
    {
        private readonly IBloomRuntimeClient _bloomRuntimeClient;
        private readonly IBusinessAccountReadModelProvider _readModelProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Subject> _visibilityProvider;
        private readonly IAdminGraphRepository _repository;

        public PermissionBusinessAccountRangeQueryHandler(IBloomRuntimeClient bloomRuntimeClient,
            IBusinessAccountReadModelProvider readModelProvider,
            IVisibilityProvider<QueryParamsTenantIds, Subject> visibilityProvider,
            IAdminGraphRepository repository)
        {
            _bloomRuntimeClient = bloomRuntimeClient;
            _readModelProvider = readModelProvider;
            _visibilityProvider = visibilityProvider;
            _repository = repository;
        }

        public async Task<IReadOnlyCollection<BusinessAccount>> Handle(PermissionBusinessAccountsQuery request,
            CancellationToken cancellationToken)
        {
            await Validate(request.Principal, request.SubjectId);

            var ids = (await BusinessAccountIds(request, cancellationToken)).ToList();
            if (!ids.Any()) return new List<BusinessAccount>(0);

            var result =
                (await _readModelProvider.SearchForResourcesAsync(0, ids.Count, new QueryParams(), ids, ct: cancellationToken));
            if (result?.Data == null || !result.Data.Any()) throw new NotFoundException();
            return result.Data;
        }

        private async Task Validate(ClaimsPrincipal userContext, Guid subjectId)
        {
            var subject =
                await _repository.GetNodeAsync<Domain.Entities.Subject>(entity => entity.Id == subjectId);
            if (subject == null) throw new NotFoundException();

            if (!await _visibilityProvider.HasItemVisibilityAsync(userContext, subjectId))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);
        }

        private async Task<IEnumerable<Guid>> BusinessAccountIds(PermissionBusinessAccountsQuery request,
            CancellationToken cancellationToken)
        {
            var actorTenants = request.Principal.GetTenants(limitTo: request.TenantIds).Select(Guid.Parse);
            var runtimeResult = await _bloomRuntimeClient.InvokeAsync(
                new SubjectRuntimeRequest
                {
                    SubjectId = request.SubjectId,
                    TenantIds = actorTenants
                }, cancellationToken);

            return runtimeResult
                .Where(r => Contains(r.Permissions, request.PermissionNames, request.EvaluationParameter))
                .Select(r => r.TenantId);
        }

        private static bool Contains(IEnumerable<string> allPermissions, IEnumerable<string> wantedPermissions,
            EvaluationParameter evaluationParameter)
        {
            switch (evaluationParameter)
            {
                case EvaluationParameter.Any:
                    return wantedPermissions.Any(allPermissions.Contains);
                case EvaluationParameter.All:
                    return wantedPermissions.Any() && wantedPermissions.All(allPermissions.Contains);
                default:
                    throw new ArgumentOutOfRangeException(nameof(evaluationParameter), evaluationParameter, null);
            }
        }
    }
}