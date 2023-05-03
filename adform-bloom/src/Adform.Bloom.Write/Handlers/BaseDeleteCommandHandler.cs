using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Write.Handlers
{
    public class BaseDeleteCommandHandler<TEntity> : BaseCommandHandler, IRequestHandler<BaseDeleteEntityCommand, Unit>
        where TEntity : BaseNode
    {
        public BaseDeleteCommandHandler(IAdminGraphRepository repository, IMediator mediator)
            : base(repository, mediator)
        {
        }

        public async Task<Unit> Handle(BaseDeleteEntityCommand request, CancellationToken cancellationToken)
        {
            await PreDeleteValidation(request, cancellationToken);
            await PreDeleteAction(request, cancellationToken);

            await AdminGraphRepository.DeleteNodeAsync<TEntity>(o => o.Id == request.IdOfEntityToDeleted, true);

            await Mediator.Publish(
                new AuditEvent(request.Principal, request.IdOfEntityToDeleted, typeof(TEntity).Name,
                    AuditOperation.Delete), cancellationToken);

            return Unit.Value;
        }

        protected virtual Task PreDeleteValidation(BaseDeleteEntityCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task PreDeleteAction(BaseDeleteEntityCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}