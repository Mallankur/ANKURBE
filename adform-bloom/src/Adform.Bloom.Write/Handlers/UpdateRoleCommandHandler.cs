using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Infrastructure.Cache;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Write.Commands;
using Adform.Bloom.Write.Mappers;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using MediatR;
using static Adform.Bloom.DataAccess.Guard;

namespace Adform.Bloom.Write.Handlers
{
    public class UpdateRoleCommandHandler : BasePayloadValidationCommandHandler<UpdateRoleCommand, Role>
    {
        private readonly IRequestToEntityMapper<UpdateRoleCommand, Role> _mapper;
        private readonly IAccessValidator _accessValidator;
        private readonly IBloomCacheManager _cache;

        public UpdateRoleCommandHandler(
            IAdminGraphRepository repository,
            IMediator mediator,
            IAccessValidator accessValidator,
            IRequestToEntityMapper<UpdateRoleCommand, Role> mapper, IBloomCacheManager cache)
            : base(repository, mediator)
        {
            _accessValidator = accessValidator;
            _mapper = mapper;
            _cache = cache;
        }

        protected override async Task<Role> HandleInternal(UpdateRoleCommand request,
            CancellationToken cancellationToken)
        {
            var principal = request.Principal;
            var roleId = request.RoleId;

            await Validate(principal, roleId);
            var node = (await AdminGraphRepository.GetNodeAsync<Role>(o => o.Id == roleId))!;

            var version = request.UpdatedAt == 0 ? node.UpdatedAt : request.UpdatedAt;
            var item = _mapper.Map(request);
            item.Id = roleId;
            item.CreatedAt = node.CreatedAt;
            item.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var result =
                await AdminGraphRepository.UpdateNodeAsync(o => o.Id == roleId && o.UpdatedAt == version, item);
            if (result == null)
            {
                throw new ConflictException(ErrorReasons.ConcurrencyFailedReason,
                    ErrorMessages.ConcurrencyCannotUpdateEntity,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Role).ToLowerFirstCharacter(), ErrorMessages.ConcurrencyCannotUpdateEntity}
                    });
            }

            await _cache.ForgetByRoleAsync(item.Name, cancellationToken);
            await Mediator.Publish(new AuditEvent(principal, result.Id, nameof(Role), AuditOperation.Update),
                cancellationToken);

            return result;
        }

        private async Task Validate(
            ClaimsPrincipal principal,
            Guid roleId)
        {
            var res = await _accessValidator.CanUpdateRole(principal, roleId);

            if (res.HasError(ErrorCodes.RoleDoesNotExist))
                ThrowNotFound<Role>(roleId);

            if (res.HasError(ErrorCodes.SubjectCannotAccessRole))
            {
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessRole,
                    parameters: new Dictionary<string, object>
                    {
                        {nameof(Role).ToLowerFirstCharacter(), ErrorMessages.SubjectCannotAccessRole}
                    });
            }
        }
    }
}