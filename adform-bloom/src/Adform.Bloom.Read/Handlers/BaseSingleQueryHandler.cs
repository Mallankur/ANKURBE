using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Interfaces;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Ciam.SharedKernel.Extensions;
using MapsterMapper;
using MediatR;

namespace Adform.Bloom.Read.Handlers
{
    public class BaseSingleQueryHandler<TQuery, TFilterInput, TFilter, TEntity, TOutput> : IRequestHandler<TQuery, TOutput>
        where TQuery : ISingleQuery<TFilterInput, TOutput>
        where TFilterInput : QueryParamsInput
        where TFilter : QueryParams
        where TEntity : NamedNode
        where TOutput : NamedNodeDto, new()
    {
        protected readonly IAdminGraphRepository _repository;
        protected readonly IVisibilityProvider<TFilter, TOutput> VisibilityProvider;
        protected readonly IMapper _mapper;

        public BaseSingleQueryHandler(
            IAdminGraphRepository repository,
            IVisibilityProvider<TFilter, TOutput> visibilityProvider, 
            IMapper mapper)
        {
            VisibilityProvider = visibilityProvider;
            _mapper = mapper;
            _repository = repository.ThrowExceptionIfNull(nameof(repository));
        }


        public async Task<TOutput> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var res = await _repository.GetNodeAsync<TEntity>(entity => entity.Id == id);

            if (res is null) throw new NotFoundException();
            var filter = _mapper.Map<TFilterInput, TFilter>(request.Filter);
            var hasAccess = await HasAccessAsync(request);

            if (!hasAccess)
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);

            return new TOutput
            {
                Id = res.Id,
                Name = res.Name,
                Description = res.Description,
                Enabled = res.IsEnabled,
                CreatedAt = res.CreatedAt,
                UpdatedAt = res.UpdatedAt
            };
        }

        protected virtual async Task<bool> HasAccessAsync(TQuery request)
        {
            return await VisibilityProvider.HasItemVisibilityAsync(request.Principal, request.Id);
        }
    }
}