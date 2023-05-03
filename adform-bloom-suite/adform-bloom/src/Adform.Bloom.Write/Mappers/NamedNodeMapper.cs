using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Commands;

namespace Adform.Bloom.Write.Mappers
{
    public class NamedNodeMapper<TCommand, TEntity> : IRequestToEntityMapper<TCommand, TEntity>
        where TCommand: NamedCreateCommand<TEntity>
        where TEntity: NamedNode, new()
    {
        public TEntity Map(TCommand cmd)
        {
            return new TEntity
            {
                Name = cmd.Name,
                Description = cmd.Description,
                IsEnabled = cmd.IsEnabled,
                CreatedAt = cmd.CreatedAt,
                UpdatedAt = cmd.UpdatedAt
            };
        }
    }
}
