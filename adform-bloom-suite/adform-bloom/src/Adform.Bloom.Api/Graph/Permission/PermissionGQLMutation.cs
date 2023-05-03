using System;
using System.Security.Claims;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class PermissionGQLMutation : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public PermissionGQLMutation(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor
                .Field("createPermission")
                .Description("Create a Permission in the Policy tree")
                .Argument(Constants.Parameters.Permission, o => o.Type<NonNullType<PermissionInputType>>())
                .Type<PermissionType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var permissionInput = context.ArgumentValue<Contracts.Output.Permission>(Constants.Parameters.Permission);
                    var result = await _mediator.Send(new CreatePermissionCommand(userContext ?? new ClaimsPrincipal(), 
                        permissionInput.Name, permissionInput.Description, permissionInput.Enabled), cancellationToken);
                    return result.MapFromDomain<Domain.Entities.Permission, Contracts.Output.Permission>();
                }).Authorize(StartupOAuth.Scopes.Full);

            descriptor
                .Field("deletePermission")
                .Description("Delete a Permission in the Policy tree by its identifier")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    await _mediator.Send(new DeletePermissionCommand(userContext ?? new ClaimsPrincipal(), id), cancellationToken);
                    return id;
                }).Authorize(StartupOAuth.Scopes.Full);
            

        }
    }
}