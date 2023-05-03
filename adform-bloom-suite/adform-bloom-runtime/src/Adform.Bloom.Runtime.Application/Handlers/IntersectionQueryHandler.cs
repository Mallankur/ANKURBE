using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Read.Entities;

namespace Adform.Bloom.Application.Handlers
{
    public class IntersectionQueryHandler : RuntimeQueryHandlerBase<SubjectIntersectionQuery>
    {
        public IntersectionQueryHandler(IAdformTenantProvider tenantProvider,
            IRuntimeProvider runtimeProvider)
            : base(tenantProvider, runtimeProvider)
        {
        }

        public override async Task<IEnumerable<RuntimeResult>> Handle(SubjectIntersectionQuery request,
            CancellationToken cancellationToken)
        {
            var tenantId = await _tenantProvider.GetAdformTenant(request.ActorId);
            if (!tenantId.Equals(Guid.Empty))
            {
                request.InheritanceEnabled = false;
                request.SubjectId = request.ActorId;
                if (!request.TenantIds.Any())
                {
                    request.TenantIds = new[] {tenantId};
                    request.TenantLegacyIds = Enumerable.Empty<int>();
                    request.TenantType = null;
                }
            }

            var results = await _runtimeProvider.GetSubjectIntersection(request);
            return results;
        }
    }
}