using Adform.Bloom.DataAccess;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Mappers;

namespace Adform.Bloom.Write.Handlers
{
    public class CreateWithParentIdCommandHandler<TCommand, TEntity> : BaseCreateCommandHandler<TCommand, TEntity>
        where TCommand : BaseCommandWithParentId<TEntity>
        where TEntity : BaseNode
    {
        public CreateWithParentIdCommandHandler(
            IRequestToEntityMapper<TCommand, TEntity> mapper, 
            IAdminGraphRepository repository, 
            IMediator mediator)
            : base(mapper, repository, mediator)
        {
        }

        protected override async Task PreCreateNodeAsync(TCommand request, CancellationToken cancellationToken)
        {
            if (request.ParentId == null)
                return;

            await AdminGraphRepository.ThrowIfNotFound<TEntity>(request.ParentId.Value);
        }

        protected override async Task PostCreateNodeAsync(TCommand request, TEntity createdEntity,
            CancellationToken cancellationToken)
        {
            if (request.ParentId == null)
                return;

            await AdminGraphRepository.CreateRelationshipAsync<TEntity, TEntity>(o => o.Id == createdEntity.Id,
                    p => p.Id == request.ParentId, Constants.ChildOfLink);
        }
    }
}