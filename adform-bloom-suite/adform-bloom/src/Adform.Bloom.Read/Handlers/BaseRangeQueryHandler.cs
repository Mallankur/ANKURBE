using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Interfaces;
using Adform.Ciam.SharedKernel.Entities;
using MapsterMapper;
using MediatR;

namespace Adform.Bloom.Read.Handlers
{
    public class BaseRangeQueryHandler<TQuery, TFilterInput, TFilter, TOutput> : IRequestHandler<TQuery,
            EntityPagination<TOutput>>
        where TQuery : IRangeQuery<TFilterInput, EntityPagination<TOutput>>
        where TFilterInput : QueryParamsInput
        where TFilter : QueryParams
        where TOutput : BaseNodeDto
    {
        private readonly IVisibilityProvider<TFilter, TOutput> _visibilityProvider;
        private readonly IMapper _mapper;

        public BaseRangeQueryHandler(IVisibilityProvider<TFilter, TOutput> visibilityProvider, IMapper mapper)
        {
            _visibilityProvider = visibilityProvider;
            _mapper = mapper;
        }

        public async Task<EntityPagination<TOutput>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<TFilterInput, TFilter>(request.Filter);
            var result =
                await _visibilityProvider.EvaluateVisibilityAsync(request.Principal, filter, request.Offset, request.Limit);
            return result;
        }
    }
}