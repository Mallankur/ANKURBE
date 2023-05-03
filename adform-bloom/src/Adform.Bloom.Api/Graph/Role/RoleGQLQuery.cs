using System;
using System.Security.Claims;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Role
{
    public class RoleGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public RoleGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("role")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<RoleType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new RoleQuery(userContext ?? new ClaimsPrincipal(), id),
                        cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("roles")
                .Argument(Constants.Parameters.PrioritizeTemplateRoles, o => o.Type<BooleanType>())
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<RoleType, Contracts.Output.Role>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsRolesInput>();
                    return await _mediator.Send(new RolesQuery(userContext,filter,
                        pagination.Offset, pagination.Limit), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}