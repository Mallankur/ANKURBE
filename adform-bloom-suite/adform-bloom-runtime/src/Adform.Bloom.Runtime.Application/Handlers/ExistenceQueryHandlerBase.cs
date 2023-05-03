using Adform.Bloom.Application.Abstractions.Persistence;
using FluentResults;
using MediatR;

namespace Adform.Bloom.Application.Handlers
{
    public abstract class ExistenceQueryHandlerBase<TQuery> :
        IRequestHandler<TQuery, Result<bool>>
        where TQuery : IRequest<Result<bool>>
    {
        private readonly IExistenceProvider _existenceProvider;

        protected ExistenceQueryHandlerBase(IExistenceProvider existenceProvider)
        {
            _existenceProvider = existenceProvider;
        }

        public async Task<Result<bool>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            return await _existenceProvider.CheckExistence(request);
        }
    }
}
