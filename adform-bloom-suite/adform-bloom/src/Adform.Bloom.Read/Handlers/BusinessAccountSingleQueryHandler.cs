using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MapsterMapper;
using MediatR;

namespace Adform.Bloom.Read.Handlers
{
    public class BusinessAccountSingleQueryHandler : IRequestHandler<BusinessAccountQuery, BusinessAccount>
    {
        private readonly IAdminGraphRepository _repository;
        private readonly IMapper _mapper;
        private readonly IBusinessAccountReadModelProvider _readModelProvider;
        private readonly IVisibilityProvider<QueryParamsBusinessAccount, Adform.Bloom.Contracts.Output.Tenant> _visibilityProvider;

        public BusinessAccountSingleQueryHandler(
            IAdminGraphRepository repository,
            IVisibilityProvider<QueryParamsBusinessAccount, Adform.Bloom.Contracts.Output.Tenant> visibilityProvider,
            IBusinessAccountReadModelProvider readModelProvider, IMapper mapper)
        {
            _repository = repository;
            _visibilityProvider = visibilityProvider;
            _readModelProvider = readModelProvider;
            _mapper = mapper;
        }

        public async Task<BusinessAccount> Handle(BusinessAccountQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var res = await _repository.GetNodeAsync<Bloom.Domain.Entities.Tenant>(entity => entity.Id == id);
            var filter = _mapper.Map<QueryParamsBusinessAccountInput?, QueryParamsBusinessAccount?>(request.Filter);

            if (res is null) throw new NotFoundException();

            var hasAccess = await HasAccessAsync(request, filter);

            if (!hasAccess)
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);

            var result = await _readModelProvider.SearchForResourceAsync(id);
            if (result is null) throw new NotFoundException();
            return result;
        }
        protected async Task<bool> HasAccessAsync(BusinessAccountQuery request, QueryParamsBusinessAccount? filter)
        {
            var tenantIds = filter?.TenantIds;
            return await _visibilityProvider.HasVisibilityAsync(request.Principal, new QueryParamsBusinessAccount
                {
                    ResourceIds = new[] { request.Id },
                    TenantIds = tenantIds
                });
        }
    }
}