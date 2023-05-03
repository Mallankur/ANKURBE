using System.Threading;
using System.Threading.Tasks;
using ProtoBuf.Grpc;

namespace Adform.Bloom.DataAccess.Extensions
{
    public interface ICallContextEnhancer
    {
        Task<CallContext> Build(CancellationToken cancellation = default);
    }
}