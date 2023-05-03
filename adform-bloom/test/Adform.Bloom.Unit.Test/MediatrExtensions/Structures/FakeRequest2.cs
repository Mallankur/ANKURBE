using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class FakeRequest2 : IRequest<FakeResponse2>
    {
    }
    
    public class FakeRequest2Handler : IRequestHandler<FakeRequest2, FakeResponse2>
    {
        public Task<FakeResponse2> Handle(FakeRequest2 request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}