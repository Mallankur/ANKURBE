using System;
using System.Security.Claims;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class PermissionGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public PermissionGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("permission")
                .Description("Get a Permission in the Policy tree by its identifier")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<PermissionType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new PermissionQuery(userContext ?? new ClaimsPrincipal(), id),
                        cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("permissions")
                .Description("Get a Permissions in the Policy tree")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<PermissionType, Contracts.Output.Permission>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    return await _mediator.Send(new PermissionsQuery(userContext, filter,
                        pagination.Offset, pagination.Limit), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}