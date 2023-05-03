using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Role
{
    public class RoleGQLMutation : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public RoleGQLMutation(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor
                .Field("createRole")
                .Description(
                    "Create a Role in the context of a Policy, if the TenantId is provided then a Custom Role will be created")
                .Argument(Constants.Parameters.PolicyId, o => o.Type<IdType>())
                .Argument(Constants.Parameters.BusinessAccountId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.Role, o => o.Type<NonNullType<RoleInputType>>())
                .Argument(Constants.Parameters.IsTemplateRole, o => o.Type<BooleanType>().DefaultValue(false))
                .Argument(Constants.Parameters.FeatureIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<NonNullType<RoleType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var policyId = context.ArgumentValue<Guid?>(Constants.Parameters.PolicyId);
                    var tenantId = context.ArgumentValue<Guid>(Constants.Parameters.BusinessAccountId);
                    var role = context.ArgumentValue<Contracts.Output.Role>(Constants.Parameters.Role);
                    var featureIds = context.ArgumentValue<IReadOnlyCollection<Guid>?>(Constants.Parameters.FeatureIds);
                    var isTemplateRole = context.ArgumentValue<bool>(Constants.Parameters.IsTemplateRole);

                    var result = await _mediator.Send(new CreateRoleCommand(userContext,
                        policyId,
                        tenantId,
                        role.Name,
                        role.Description,
                        role.Enabled,
                        featureIds,
                        isTemplateRole), cancellationToken);
                    var roleResult = result.MapFromDomain<Domain.Entities.Role, Contracts.Output.Role>();
                    roleResult.Type = isTemplateRole ? RoleCategory.Template : RoleCategory.Custom;
                    return roleResult;
                }).Authorize(StartupOAuth.Scopes.Full).Authorize(new[] { ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin });

            descriptor
                .Field("updatePermissionToRoleAssignments")
                .Description("Assign or unassign a Permission to a Role by its identifiers")  
                .Deprecated()
                .Argument(Constants.Parameters.Assignment, o => o.Type<NonNullType<AssignPermissionToRoleInputType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var assignment = context.ArgumentValue<AssignPermissionToRole>(Constants.Parameters.Assignment);
                    await _mediator.Send(new AssignPermissionToRoleCommand(userContext ?? new ClaimsPrincipal(),
                        assignment.PermissionId, assignment.RoleId, assignment.Operation.MapToDomain()), cancellationToken);
                    return assignment.RoleId;
                }).Authorize(StartupOAuth.Scopes.Full).Authorize(new[] { ClaimPrincipalExtensions.AdformAdmin});

            descriptor
                .Field("deleteRole")
                .Description("Delete a Role from the Policy tree by its identifier.")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    await _mediator.Send(new DeleteRoleCommand(userContext, id), cancellationToken);
                    return id;
                }).Authorize(StartupOAuth.Scopes.Full).Authorize(new[] { ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin });

            descriptor
                .Field("updateRole")
                .Description("Update a Role from the Policy tree by its identifier")
                .Argument(Constants.Parameters.RoleId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.Role, o => o.Type<NonNullType<RoleInputType>>())
                .Type<NonNullType<RoleType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var roleId = context.ArgumentValue<Guid>(Constants.Parameters.RoleId);
                    var role = context.ArgumentValue<Contracts.Output.Role>(Constants.Parameters.Role);
                    var result = await _mediator.Send(new UpdateRoleCommand(
                        userContext,
                        roleId,
                        role.Name,
                        role.Description,
                        role.Enabled
                    ), cancellationToken);
                    var roleResult = result.MapFromDomain<Domain.Entities.Role, Contracts.Output.Role>();
                    roleResult.Type = RoleCategory.Custom;
                    return roleResult;
                }).Authorize(StartupOAuth.Scopes.Full).Authorize(new[] { ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin });

            descriptor
                .Field("updateRoleToFeatureAssignments")
                .Description("Assign or Unassign a Role to/from a Features by their identifiers")
                .Argument(Constants.Parameters.RoleId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.AssignFeatureIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Argument(Constants.Parameters.UnassignFeatureIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var roleId = context.ArgumentValue<Guid>(Constants.Parameters.RoleId);
                    var featureAssignments =
                        context.ArgumentValue<IReadOnlyCollection<Guid>?>(Constants.Parameters.AssignFeatureIds);
                    var featureUnassignments =
                        context.ArgumentValue<IReadOnlyCollection<Guid>?>(Constants.Parameters.UnassignFeatureIds);

                    await _mediator.Send(new UpdateRoleToFeatureAssignmentsCommand(userContext, roleId,
                        featureAssignments, featureUnassignments), cancellationToken);
                    return roleId;
                }).Authorize(StartupOAuth.Scopes.Full).Authorize(new[] { ClaimPrincipalExtensions.AdformAdmin, ClaimPrincipalExtensions.LocalAdmin });
        }
    }
}