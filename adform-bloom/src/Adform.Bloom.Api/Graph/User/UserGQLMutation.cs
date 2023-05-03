using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.User
{
    public class UserGQLMutation : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public UserGQLMutation(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor
                .Field("updateUserAssignments")
                .Description("Assign or Unassign a User to/from a Roles by their identifiers")
                .Argument(Constants.Parameters.UserId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.AssignRoleBusinessAccountIds,
                    o => o.Type<ListType<RoleBusinessAccountInputType>>())
                .Argument(Constants.Parameters.UnassignRoleBusinessAccountIds,
                    o => o.Type<ListType<RoleBusinessAccountInputType>>())
                .Argument(Constants.Parameters.AssetsReassignments,
                    o => o.Type<ListType<UserBusinessAccountAssetsReassignmentInputType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var subjectId = context.ArgumentValue<Guid>(Constants.Parameters.UserId);
                    var assignRoleBusinessAccountIds =
                        context.ArgumentValue<IReadOnlyCollection<RoleBusinessAccount>?>(
                            Constants.Parameters.AssignRoleBusinessAccountIds);
                    var unassignRoleBusinessAccountIds =
                        context.ArgumentValue<IReadOnlyCollection<RoleBusinessAccount>?>(
                            Constants.Parameters.UnassignRoleBusinessAccountIds);
                    var assetsReassignments = context.ArgumentValue<IReadOnlyCollection<AssetsReassignment>?>(
                        Constants.Parameters.AssetsReassignments);
                    var assignRoleTenantIds = assignRoleBusinessAccountIds?.Select(o => new RoleTenant
                    {
                        RoleId = o.RoleId,
                        TenantId = o.BusinessAccountId
                    }).ToList();
                    var unassignRoleTenantIds = unassignRoleBusinessAccountIds?.Select(o => new RoleTenant
                    {
                        RoleId = o.RoleId,
                        TenantId = o.BusinessAccountId
                    }).ToList();
                    await _mediator.Send(new UpdateSubjectAssignmentsCommand(userContext, subjectId,
                        assignRoleTenantIds, unassignRoleTenantIds, assetsReassignments), cancellationToken);
                    return subjectId;
                }).Authorize(StartupOAuth.Scopes.Full);
        }
    }
}