using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.User;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using MapsterMapper;
using MediatR;
using Subject = Adform.Bloom.Contracts.Output.Subject;
using User = Adform.Bloom.Contracts.Output.User;

namespace Adform.Bloom.Read.Handlers
{
    public class UserRangeQueryHandler : IRequestHandler<UsersQuery, EntityPagination<User>>
    {
        private readonly IVisibilityProvider<QueryParamsTenantIds, Subject> _visibilityProvider;
        private readonly IUserReadModelProvider _readModelProvider;
        private readonly IMapper _mapper;

        public UserRangeQueryHandler(
            IVisibilityProvider<QueryParamsTenantIds, Subject> visibilityProvider,
            IUserReadModelProvider readModelProvider,
            IMapper mapper)
        {
            _visibilityProvider = visibilityProvider;
            _readModelProvider = readModelProvider;
            _mapper = mapper;
        }

        public async Task<EntityPagination<User>> Handle(UsersQuery request, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<QueryParamsTenantIdsInput, QueryParamsTenantIds>(request.Filter);
            var ids = await _visibilityProvider.EvaluateVisibilityAsync(request.Principal,filter, 0, int.MaxValue);
            var search = filter.Search;
            if (search?.Length < 3)
            {
                filter.Search = null;
            }

            if (ids.Data.Count < 1 || request.Limit == 0)
            {
                return new EntityPagination<User>(ids.Offset, ids.Limit, ids.TotalItems,
                    new List<User>(0));
            }

            return await _readModelProvider.SearchForResourcesAsync(
                request.Offset,
                request.Limit,
                filter,
                ids.Data.Select(x => x.Id),
                !request.Principal.IsAdformAdmin() ? UserType.MasterAccount : null,
                cancellationToken);
        }
    }
}