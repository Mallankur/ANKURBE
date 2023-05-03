using MediatR;

namespace Adform.Bloom.Read.Interfaces
{
    public interface IRangeQueryHandler<TFilter,  TEntity> : IRequest<TEntity>
    {
    }
}