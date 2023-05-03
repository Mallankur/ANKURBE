using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Application.Handlers
{
    public class LegacyTenantExistenceQueryHandler : ExistenceQueryHandlerBase<LegacyTenantExistenceQuery>
    {
        public LegacyTenantExistenceQueryHandler(IExistenceProvider existenceProvider) : base(existenceProvider)
        {
        }
    }
}
