using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Mappers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using CorrelationId.Abstractions;
using MediatR;

namespace Adform.Bloom.Write.Handlers
{
    public class CreateSubjectCommandHandler : BasePayloadValidationCommandHandler<CreateSubjectCommand, Subject>
    {
        private readonly IRequestToEntityMapper<CreateSubjectCommand, Subject> _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly ICorrelationContextAccessor _correlationContext;

        public CreateSubjectCommandHandler(IRequestToEntityMapper<CreateSubjectCommand, Subject> mapper,
            IAccessValidator accessValidator,
            IAdminGraphRepository repository,
            IMediator mediator,
            ICorrelationContextAccessor correlationContext) : base(repository, mediator)
        {
            _mapper = mapper;
            _accessValidator = accessValidator;
            _correlationContext = correlationContext;
        }

        protected override async Task<Subject> HandleInternal(CreateSubjectCommand request,
            CancellationToken cancellationToken)
        {
            var subject = _mapper.Map(request);
            if (!await _accessValidator.CanCreateSubjectAsync(request.Principal, request.Id))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);
            }

            await AdminGraphRepository.CreateNodeAsync(subject);

            if (request.RoleTenantIds?.Count > 0)
            {
                var command = new UpdateSubjectAssignmentsCommand(request.Principal, subject.Id,
                    request.RoleTenantIds,
                    null);
                await Mediator.Send(command, cancellationToken);
            }

            return subject;
        }

    }
}