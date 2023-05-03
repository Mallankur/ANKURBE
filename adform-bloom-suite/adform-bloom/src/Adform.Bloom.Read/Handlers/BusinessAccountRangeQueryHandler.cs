using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using MapsterMapper;
using MediatR;
using Tenant = Adform.Bloom.Contracts.Output.Tenant;

namespace Adform.Bloom.Read.Handlers
{
    public class
        BusinessAccountRangeQueryHandler : IRequestHandler<BusinessAccountsQuery, EntityPagination<BusinessAccount>>
    {
        private readonly IVisibilityProvider<QueryParamsBusinessAccount, Tenant> _visibilityProvider;
        private readonly IBusinessAccountReadModelProvider _readModelProvider;
        private readonly IMapper _mapper;

        public BusinessAccountRangeQueryHandler(
            IVisibilityProvider<QueryParamsBusinessAccount, Tenant> visibilityProvider,
            IBusinessAccountReadModelProvider readModelProvider, IMapper mapper)
        {
            _visibilityProvider = visibilityProvider;
            _readModelProvider = readModelProvider;
            _mapper = mapper;
        }

        public async Task<EntityPagination<BusinessAccount>> Handle(BusinessAccountsQuery request,
            CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<QueryParamsBusinessAccountInput, QueryParamsBusinessAccount>(request.Filter);
            var ids = await _visibilityProvider.EvaluateVisibilityAsync(request.Principal, filter, 0, int.MaxValue);

            if (filter.Search?.Length < 3)
            {
                filter.Search = null;
            }

            if (ids.Data.Count < 1 || request.Limit == 0)
            {
                return new EntityPagination<BusinessAccount>(ids.Offset, ids.Limit, ids.TotalItems,
                    new List<BusinessAccount>(0));
            }

            return await _readModelProvider.SearchForResourcesAsync(
                request.Offset,
                request.Limit,
                filter,
                ids.Data.Select(x => x.Id),
                (BusinessAccountType?) filter.BusinessAccountType,
                cancellationToken);
        }
    }
}