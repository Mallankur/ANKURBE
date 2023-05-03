using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using Role = Adform.Bloom.Contracts.Output.Role;

namespace Adform.Bloom.Read.Handlers
{
    public class RoleSingleQueryHandler : IRequestHandler<RoleQuery, Role>
    {
        private readonly IAdminGraphRepository _repository;
        private readonly IVisibilityProvider<QueryParamsRoles, Role> _visibilityProvider;

        public RoleSingleQueryHandler(
            IAdminGraphRepository repository,
            IVisibilityProvider<QueryParamsRoles, Role> visibilityProvider)
        {
            _repository = repository;
            _visibilityProvider = visibilityProvider;
        }

        public async Task<Role> Handle(RoleQuery request, CancellationToken cancellationToken)
        {
            var id = request.Id;
            var res = await _repository.GetNodeAsync<Domain.Entities.Role>(entity => entity.Id == id);

            if (res is null) throw new NotFoundException();
            var hasAccess = await HasAccessAsync(request);

            if (!hasAccess)
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessEntity);

            var labels = await _repository.GetLabelsAsync<Domain.Entities.Role>(entity => entity.Id == id);
            return new Role
            {
                Id = res.Id,
                Name = res.Name,
                Description = res.Description,
                Enabled = res.IsEnabled,
                Type = labels.Contains(RoleCategory.Template.ToString()) ? RoleCategory.Template : RoleCategory.Custom,
                CreatedAt = res.CreatedAt,
                UpdatedAt = res.UpdatedAt
            };
        }

        protected async Task<bool> HasAccessAsync(RoleQuery request)
        {
            return await _visibilityProvider.HasItemVisibilityAsync(request.Principal, request.Id);
        }
    }
}