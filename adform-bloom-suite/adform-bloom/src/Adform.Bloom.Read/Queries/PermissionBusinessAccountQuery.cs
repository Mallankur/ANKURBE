using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using MediatR;

namespace Adform.Bloom.Read.Queries
{
    public class PermissionBusinessAccountsQuery : IRequest<IReadOnlyCollection<BusinessAccount>>
    {
        public ClaimsPrincipal Principal { get; }
        public Guid SubjectId { get; }
        public IReadOnlyCollection<string> PermissionNames { get; }
        public EvaluationParameter EvaluationParameter { get; }
        public IReadOnlyCollection<Guid>? TenantIds { get; }

        public PermissionBusinessAccountsQuery(ClaimsPrincipal principal, Guid subjectId, IReadOnlyCollection<string> permissionNames,
            EvaluationParameter evaluationParameter, IReadOnlyCollection<Guid>? tenantIds = null)
        {
            Principal = principal;
            SubjectId = subjectId;
            PermissionNames = permissionNames;
            EvaluationParameter = evaluationParameter;
            TenantIds = tenantIds;
        }
    }
}