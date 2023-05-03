using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;

namespace Adform.Bloom.Write.Handlers
{
    public abstract class BasePayloadValidationCommandHandler<TCommand, TEntity> : BaseCommandHandler,
        IRequestHandler<TCommand, TEntity>
        where TCommand : BaseCreateCommand<TEntity>
    {
        protected BasePayloadValidationCommandHandler(IAdminGraphRepository repository, IMediator mediator) : base(repository,
            mediator)
        {
        }

        public Task<TEntity> Handle(TCommand request, CancellationToken cancellationToken)
        {
            ValidatePayload(request);
            return HandleInternal(request, cancellationToken);
        }

        protected abstract Task<TEntity> HandleInternal(TCommand request, CancellationToken cancellationToken);

        private static void ValidatePayload(TCommand request)
        {
            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(request, new ValidationContext(request),
                validationResults, true))
            {
                return;
            }

            throw new BadRequestException(parameters: new Dictionary<string, object>(
                validationResults.Select(r =>
                    new KeyValuePair<string, object>(r.MemberNames.First().ToLowerFirstCharacter(),
                        r.ErrorMessage!))));
        }
    }
}