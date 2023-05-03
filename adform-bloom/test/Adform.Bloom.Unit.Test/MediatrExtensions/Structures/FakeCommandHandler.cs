using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class FakeCommandHandler :
        IRequestHandler<FakeRequest, FakeResponse>,
        IRequestHandler<FakeRequest3>
    {
        public Task<FakeResponse> Handle(FakeRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MediatR.Unit> Handle(FakeRequest3 request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}