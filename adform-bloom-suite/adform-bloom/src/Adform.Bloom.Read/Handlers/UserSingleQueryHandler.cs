using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;

namespace Adform.Bloom.Read.Handlers
{
    public class UserSingleQueryHandler : IRequestHandler<UserQuery, User>
    {
        private readonly IAdminGraphRepository _repository;
        private readonly IUserReadModelProvider _readModelProvider;
        private readonly IVisibilityProvider<QueryParamsTenantIds, Adform.Bloom.Contracts.Output.Subject> _visibilityProvider;

        public UserSingleQueryHandler(
            IAdminGraphRepository repository,
            IVisibilityProvider<QueryParamsTenantIds, Adform.Bloom.Contracts.Output.Subject> visibilityProvider, 
            IUserReadModelProvider readModelProvider)
        {
            _repository = repository;
            _visibilityProvider = visibilityProvider;
            _readModelProvider = readModelProvider;
        }

        public async Task<User> Handle(UserQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var res = (await _repository.GetNodesAsync<Domain.Entities.Subject>(entity => entity.Id == id,
                notLabel: Constants.Label.TRAFFICKER, limit:1)).FirstOrDefault();

            if (res is null) throw new NotFoundException();

            var hasAccess = await HasAccessAsync(request);

            if (!hasAccess)
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);

            var result = await _readModelProvider.SearchForResourceAsync(id);
            if (result is null) throw new NotFoundException();
            return result;
        }
        protected async Task<bool> HasAccessAsync(UserQuery request)
        {
            return await _visibilityProvider.HasVisibilityAsync(
                request.Principal, new QueryParamsTenantIds
                {
                    ResourceIds = new []{request.Id},
                    TenantIds = request.Filter?.TenantIds
                });
        }
    }
}