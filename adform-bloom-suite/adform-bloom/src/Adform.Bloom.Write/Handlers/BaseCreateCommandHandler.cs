using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Mappers;

namespace Adform.Bloom.Write.Handlers
{
    public class BaseCreateCommandHandler<TCommand, TEntity> : BaseCommandHandler, IRequestHandler<TCommand, TEntity>
        where TCommand : NamedCreateCommand<TEntity>
        where TEntity : BaseNode
    {
        private readonly IRequestToEntityMapper<TCommand, TEntity> _mapper;

        public BaseCreateCommandHandler(
            IRequestToEntityMapper<TCommand, TEntity> mapper,
            IAdminGraphRepository repository,
            IMediator mediator)
            : base(repository, mediator)
        {
            _mapper = mapper;
        }

        public async Task<TEntity> Handle(TCommand request, CancellationToken cancellationToken)
        {
            await PreCreateNodeAsync(request, cancellationToken);
            var result = await AdminGraphRepository.CreateNodeAsync(_mapper.Map(request));
            await PostCreateNodeAsync(request, result, cancellationToken);
            await Mediator.Publish(
                new AuditEvent(request.Principal, result.Id, typeof(TEntity).Name, AuditOperation.Create),
                cancellationToken);
            return result;
        }

        protected virtual Task PreCreateNodeAsync(TCommand request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected virtual Task PostCreateNodeAsync(TCommand request, TEntity createdEntity,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}