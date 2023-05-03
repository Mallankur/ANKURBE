using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Neo4jClient.Transactions;

namespace Adform.Bloom.Mediatr.Extensions
{
    public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ITransactionalGraphClient _client;

        public UnitOfWorkBehavior(ITransactionalGraphClient client)
        {
            _client = client;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            using var uow = _client.BeginTransaction();
            try
            {
                var response = await next();
                if (cancellationToken.IsCancellationRequested)
                {
                    await uow.RollbackAsync();
                }
                else
                {
                    await uow.CommitAsync();
                }
                return response;
            }
            catch
            {
                await uow.RollbackAsync();
                throw;
            }
        }
    }
}