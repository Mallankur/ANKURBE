using System;
using System.Collections.Generic;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.User
{
    public class UserGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public UserGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("user")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<UserType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new UserQuery(userContext, id), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("users")
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.UserIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<PaginationType<UserType, Contracts.Output.User>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    var userIds = context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.UserIds);
                    filter.ResourceIds = userIds;
                    return await _mediator.Send(new UsersQuery(userContext, filter, pagination.Offset, pagination.Limit), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}