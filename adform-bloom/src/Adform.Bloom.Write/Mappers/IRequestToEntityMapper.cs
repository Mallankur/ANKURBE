using Adform.Bloom.Domain.Entities;

namespace Adform.Bloom.Write.Mappers
{
    public interface IRequestToEntityMapper<in TCommand, out TEntity>
        where TEntity: BaseNode
    {
        TEntity Map(TCommand cmd);
    }
}
