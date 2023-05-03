using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.DataAccess;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.Domain.Interfaces;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using Adform.Bloom.Infrastructure.Audit;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Constants = Adform.Bloom.Infrastructure.Constants;

namespace Adform.Bloom.Write.Handlers
{
    public class UpdateSubjectAssignmentsCommandHandler : BaseCommandHandler,
        IRequestHandler<UpdateSubjectAssignmentsCommand, IEnumerable<RuntimeResponse>>
    {
        private readonly IAccessValidator _accessValidator;
        private readonly IBloomRuntimeClient _bloomRuntimeClient;
        private readonly ILogger<UpdateSubjectAssignmentsCommandHandler> _logger;

        public UpdateSubjectAssignmentsCommandHandler(
            IAdminGraphRepository repository,
            IMediator mediator,
            IAccessValidator accessValidator,
            IBloomRuntimeClient bloomRuntimeClient,
            ILogger<UpdateSubjectAssignmentsCommandHandler> logger)
            : base(repository, mediator)
        {
            _accessValidator = accessValidator ?? throw new ArgumentNullException(nameof(accessValidator));
            _bloomRuntimeClient = bloomRuntimeClient ?? throw new ArgumentNullException(nameof(bloomRuntimeClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));  
        }

        public async Task<IEnumerable<RuntimeResponse>> Handle(UpdateSubjectAssignmentsCommand request, CancellationToken cancellationToken)
        {
            var originalState = await AssignUnassign(request, cancellationToken);
            await AuditingAsync(request, cancellationToken);
            return originalState;
        }

        private async Task<IEnumerable<RuntimeResponse>> AssignUnassign(UpdateSubjectAssignmentsCommand request,
            CancellationToken cancellationToken)
        {
            await ValidateAsync(request.Principal, request.AssignRoleTenantIds, request.UnassignRoleTenantIds,
                request.SubjectId);

            var result = await _bloomRuntimeClient.InvokeAsync(new SubjectRuntimeRequest
            {
                SubjectId = request.SubjectId,
                InheritanceEnabled = false
            },
                cancellationToken);

            var assignTask = request.AssignRoleTenantIds != null && request.AssignRoleTenantIds.Any()
                ? AdminGraphRepository.BulkLazyCreateGroupAsync(request.SubjectId, request.AssignRoleTenantIds)
                : Task.CompletedTask;
            var unassignTask = request.UnassignRoleTenantIds != null && request.UnassignRoleTenantIds.Any()
                ? AdminGraphRepository.BulkUnassignSubjectFromRolesAsync(request.SubjectId,
                    request.UnassignRoleTenantIds)
                : Task.CompletedTask;


            await Task.WhenAll(assignTask, unassignTask);
            return result;
        }

        private async Task AuditingAsync(UpdateSubjectAssignmentsCommand request, CancellationToken cancellationToken)
        {
            if (request.AssignRoleTenantIds != null)
                foreach (var assign in request.AssignRoleTenantIds)
                {
                    var oldEntity = new ConnectedNode(request.SubjectId, assign.TenantId, assign.RoleId, null);
                    var newEntity = new ConnectedNode(request.SubjectId, assign.TenantId, assign.RoleId,
                        Constants.Relationship.MEMBER_OF);
                    await Mediator.Publish(
                        new AuditChange(request.Principal, oldEntity, newEntity, AuditOperation.Update),
                        cancellationToken);
                }

            if (request.UnassignRoleTenantIds != null)
                foreach (var assign in request.UnassignRoleTenantIds)
                {
                    var oldEntity = new ConnectedNode(request.SubjectId, assign.TenantId, assign.RoleId,
                        Constants.Relationship.MEMBER_OF);
                    var newEntity = new ConnectedNode(request.SubjectId, assign.TenantId, assign.RoleId, null);
                    await Mediator.Publish(
                        new AuditChange(request.Principal, oldEntity, newEntity, AuditOperation.Update),
                        cancellationToken);
                }
        }

        private async Task<(IReadOnlyCollection<RoleTenant>?, IReadOnlyCollection<RoleTenant>?)> FilterAsync(IEnumerable<RoleTenant>? assignRoleTenantIds,
            IEnumerable<RoleTenant>? unassignRoleTenantIds, Guid subjectId)
        {

            var subjectGroups =
                (await AdminGraphRepository.GetConnectedAsync<Subject,Group>(s=>s.Id == subjectId, Constants.MemberOfLink)).Select(g=>g.Id);

            var currentRoleTenants = subjectGroups.Select(async gId =>
            {
                var tId =
                    (await AdminGraphRepository.GetConnectedAsync<Group, Tenant>(g => g.Id==gId, Constants.BelongsLink)).Select(t =>
                        t.Id).FirstOrDefault();
                var rId = (await AdminGraphRepository.GetConnectedAsync<Group, Role>(g => g.Id==gId, Constants.AssignedLink)).Select(r => r.Id).FirstOrDefault();

                return new RoleTenant {RoleId = rId, TenantId = tId};
            }).Select(t=>t.Result);

            var actualAssign = assignRoleTenantIds?.Where(x => !currentRoleTenants.Any(y => y.TenantId.Equals(x.TenantId) && y.RoleId.Equals(x.RoleId))).ToList().AsReadOnly();
            var actualUnassign = unassignRoleTenantIds?.Where(x => currentRoleTenants.Any(y => y.TenantId.Equals(x.TenantId) && y.RoleId.Equals(x.RoleId))).ToList().AsReadOnly();
            return (actualAssign, actualUnassign);
        }

        private async Task ValidateAsync(ClaimsPrincipal principal, IEnumerable<RoleTenant>? assignRoleTenantIds,
            IEnumerable<RoleTenant>? unassignRoleTenantIds, Guid subjectId)
        {
            var assignRoleTenantIdsEnumerated = assignRoleTenantIds?.ToList();
            var unassignRoleTenantIdsEnumerated = unassignRoleTenantIds?.ToList();
            var (filteredAssignments, filteredUnassignments) = await FilterAsync(assignRoleTenantIdsEnumerated, unassignRoleTenantIdsEnumerated, subjectId);
            var res = new ValidationResult();
            if (assignRoleTenantIdsEnumerated != null && assignRoleTenantIdsEnumerated.Any())
            {
                var result = await _accessValidator.CanAssignSubjectToRolesAsync(principal,
                    assignRoleTenantIdsEnumerated, filteredAssignments, filteredUnassignments, subjectId);
                res.SetError(result.Error);
            }

            if (unassignRoleTenantIdsEnumerated != null && unassignRoleTenantIdsEnumerated.Any())
            {
                var result =
                    await _accessValidator.CanUnassignSubjectFromRolesAsync(principal,
                        unassignRoleTenantIdsEnumerated, subjectId);
                res.SetError(result.Error);
            }

            if (res.HasError(ErrorCodes.TenantDoesNotExist))
                Guard.ThrowNotFound<Tenant>();

            if (res.HasError(ErrorCodes.SubjectDoesNotExist))
                Guard.ThrowNotFound<Subject>();

            if (res.HasError(ErrorCodes.RoleDoesNotExist))
                Guard.ThrowNotFound<Role>();

            if (res.HasError(ErrorCodes.SubjectCannotAccessTenant))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessTenant);

            if (res.HasError(ErrorCodes.SubjectCannotAccessSubject))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessSubject);

            if (res.HasError(ErrorCodes.SubjectCannotModifyAssignmentsForHimself))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotModifyAssignmentsForHimself);

            if (res.HasError(ErrorCodes.SubjectCannotAccessRole))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotAccessRole);

            if (res.HasError(ErrorCodes.SubjectCannotExceedRoleAssignmentLimit))
                throw new ForbiddenException(
                    ErrorReasons.AccessControlValidationFailedReason,
                    ErrorMessages.SubjectCannotExceedRoleAssignmentLimit);
        }

    }
}