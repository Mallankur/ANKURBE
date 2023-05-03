using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class Fake2CommandHandler :
        IRequestHandler<FakeRequest, FakeResponse>,
        IRequestHandler<FakeRequest2, FakeResponse2>
    {
        public Task<FakeResponse> Handle(FakeRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<FakeResponse2> Handle(FakeRequest2 request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}