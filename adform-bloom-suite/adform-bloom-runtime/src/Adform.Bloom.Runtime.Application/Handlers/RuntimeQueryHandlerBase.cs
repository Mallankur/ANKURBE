using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Read.Entities;
using MediatR;

namespace Adform.Bloom.Application.Handlers
{
    public abstract class RuntimeQueryHandlerBase<T> : IRequestHandler<T, IEnumerable<RuntimeResult>> where T : SubjectQueryBase
    {
        protected readonly IAdformTenantProvider _tenantProvider;
        protected readonly IRuntimeProvider _runtimeProvider;

        public RuntimeQueryHandlerBase(
            IAdformTenantProvider tenantProvider,
            IRuntimeProvider runtimeProvider)
        {
            _tenantProvider = tenantProvider;
            _runtimeProvider = runtimeProvider;
        }

        public abstract Task<IEnumerable<RuntimeResult>> Handle(T request, CancellationToken cancellationToken);
    }
}