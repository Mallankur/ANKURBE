using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;

namespace Adform.Bloom.Write.Handlers
{
#warning To Remove once the flow is fully defined, in the meantime this can be used to maintain the graph manually.
    public class DeleteSubjectCommandHandler : BaseDeleteCommandHandler<Subject>
    {
        private readonly IAccessValidator _accessValidator;

        public DeleteSubjectCommandHandler(
            IAdminGraphRepository repository,
            IAccessValidator accessValidator,
            IMediator mediator)
            : base(repository, mediator)
        {
            _accessValidator = accessValidator;
        }

        protected override async Task PreDeleteValidation(BaseDeleteEntityCommand request, CancellationToken cancellationToken)
        {
            var res = await _accessValidator.CanDeleteSubjectAsync(request.Principal, request.IdOfEntityToDeleted);

            if (!res)
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotDeleteEntity);
        }
    }
}