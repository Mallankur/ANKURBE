using MediatR;

namespace Adform.Bloom.Read.Interfaces
{
    public interface ISingleQuerHandlery<out TEntity> : IRequest<TEntity>
    {
    }
}