using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class FakeRequest3 : IRequest<MediatR.Unit>
    {
    }
    
    public class FakeRequest3Handler : IRequestHandler<FakeRequest3, MediatR.Unit>
    {
        public Task<MediatR.Unit> Handle(FakeRequest3 request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}