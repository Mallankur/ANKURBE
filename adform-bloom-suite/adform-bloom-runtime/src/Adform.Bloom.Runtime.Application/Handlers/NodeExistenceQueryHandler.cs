using Adform.Bloom.Application.Abstractions.Persistence;
using Adform.Bloom.Application.Queries;

namespace Adform.Bloom.Application.Handlers
{
    public class NodeExistenceQueryHandler : ExistenceQueryHandlerBase<NodeExistenceQuery>
    {
        public NodeExistenceQueryHandler(IExistenceProvider existenceProvider) : base(existenceProvider)
        {
        }
    }
}
