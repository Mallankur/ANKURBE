using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Read.Entities;
using Microsoft.Extensions.Logging;

namespace Adform.Bloom.Application.Handlers
{
    public class RuntimeQueryHandler : RuntimeQueryHandlerBase<SubjectRuntimeQuery>
    {
        private readonly IValidateQuery _validateQuery;
        private readonly ILogger<RuntimeQueryHandler> _logger;
        public RuntimeQueryHandler(ILogger<RuntimeQueryHandler> logger, IAdformTenantProvider tenantProvider,IRuntimeProvider runtimeProvider, IValidateQuery validateQuery)
            : base(tenantProvider, runtimeProvider)
        {
            _validateQuery = validateQuery;
            _logger = logger;
        }

        public override async Task<IEnumerable<RuntimeResult>> Handle(SubjectRuntimeQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"GET {request}");
            _validateQuery.Validate(request);
            var tenantId = await _tenantProvider.GetAdformTenant(request.SubjectId);
            if (!tenantId.Equals(Guid.Empty))
            {
                request.InheritanceEnabled = false;
                if (!request.TenantIds.Any())
                {
                    request.TenantIds = new[] {tenantId};
                    request.TenantLegacyIds = Enumerable.Empty<int>();
                    request.TenantType = null;
                }
            }

            var results = await _runtimeProvider.GetSubjectEvaluation(request);
            return results;
        }
    }
}