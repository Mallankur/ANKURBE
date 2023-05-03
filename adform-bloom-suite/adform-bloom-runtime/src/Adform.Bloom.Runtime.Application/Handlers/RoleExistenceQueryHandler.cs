using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Application.Handlers
{
    public class RoleExistenceQueryHandler : ExistenceQueryHandlerBase<RoleExistenceQuery>
    {
        public RoleExistenceQueryHandler(IExistenceProvider existenceProvider) : base(existenceProvider)
        {
        }
    }
}
