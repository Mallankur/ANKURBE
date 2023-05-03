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
    public class BaseAccessRangeQueryHandler<TQuery, TContext, TFilterInput, TFilter, TOutput> : IRequestHandler<TQuery, EntityPagination<TOutput>>
        where TQuery : IBaseAccessRangeQuery<TContext, TFilterInput, EntityPagination<TOutput>>
        where TFilterInput : QueryParamsInput
        where TFilter : QueryParams
        where TContext : BaseNodeDto
        where TOutput : BaseNodeDto
    {

        private readonly IAccessProvider<TContext, TFilter, TOutput> _accessProvider;
        private readonly IMapper _mapper;

        public BaseAccessRangeQueryHandler(IAccessProvider<TContext, TFilter, TOutput> accessProvider, IMapper mapper)
        {
            _accessProvider = accessProvider;
            _mapper = mapper;
        }
        public Task<EntityPagination<TOutput>> Handle(TQuery request, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<TFilterInput , TFilter>(request.Filter);
            return _accessProvider.EvaluateAccessAsync(request.Principal,
                request.Context,
                request.Offset,
                request.Limit,
                filter,
                cancellationToken
            );
        }
    }
}