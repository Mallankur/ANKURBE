using Adform.Bloom.DataAccess.Interfaces;
using MediatR;

namespace Adform.Bloom.Write.Handlers
{
    public abstract class BaseCommandHandler
    {
        protected readonly IAdminGraphRepository AdminGraphRepository;
        protected readonly IMediator Mediator;

        protected BaseCommandHandler(IAdminGraphRepository repository, IMediator mediator)
        {
            AdminGraphRepository = repository;
            Mediator = mediator;
        }
    }
}