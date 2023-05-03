using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class FakeRequest : IRequest<FakeResponse>
    {
    }

    public class FakeRequestHandler : IRequestHandler<FakeRequest, FakeResponse>
    {
        public Task<FakeResponse> Handle(FakeRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}