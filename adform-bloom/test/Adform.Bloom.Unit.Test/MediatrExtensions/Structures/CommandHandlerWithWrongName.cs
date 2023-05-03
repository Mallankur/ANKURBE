using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Adform.Bloom.Unit.Test.MediatrExtensions.Structures
{
    public class CommandHandlerWithWrongName : IRequestHandler<FakeRequest4, FakeResponse3>
    {
        public Task<FakeResponse3> Handle(FakeRequest4 request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}