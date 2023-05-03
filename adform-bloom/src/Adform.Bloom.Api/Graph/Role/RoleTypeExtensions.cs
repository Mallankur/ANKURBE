using System.Linq;
using System;
using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Feature;
using Adform.Bloom.Api.Graph.User;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Role
{
    public class RoleTypeExtensions : ObjectTypeExtension<Contracts.Output.Role>
    {
        private readonly IMediator _mediator;

        public RoleTypeExtensions(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Role> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Role));

            descriptor
                .Field("features")
                .Description("Role Features.")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Argument(Constants.Parameters.BusinessAccountId, o => o.Type<IdType>())
                .Type<PaginationType<FeatureType, Contracts.Output.Feature>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    var tenantId = context.ArgumentValue<Guid?>(Constants.Parameters.BusinessAccountId);
                    if (tenantId.HasValue)
                    {
                        filter.TenantIds = new[] { tenantId.Value };
                    }

                    var parent = context.Parent<Contracts.Output.Role>();

                    return await _mediator.Send(new BaseAccessRangeQuery<Contracts.Output.Role, QueryParamsTenantIdsInput, Contracts.Output.Feature>(
                        userContext, new Contracts.Output.Role() { Id = parent.Id }, pagination.Offset, pagination.Limit, filter), cancellationToken);
                });

            descriptor
                .Field("users")
                .Description("Assigned Users.")
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<UserType, Contracts.Output.User>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var pagination = context.ArgumentValue<PaginationInput>(Constants.Parameters.Pagination);
                    var parent = context.Parent<Contracts.Output.Role>();
                    return await _mediator.Send(new UsersQuery(userContext, new QueryParamsTenantIdsInput
                        {
                            ContextId = parent.Id
                        }, pagination.Offset,
                        pagination.Limit), cancellationToken);
                });

            descriptor
                .Field("businessAccount")
                .Description("Business Account.")
                .Type<BusinessAccountType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.Role>();
                    var dataLoader = context.DataLoader<BusinessAccountByRoleIdBatchDataLoader>();
                    return await dataLoader.LoadAsync(parent.Id, cancellationToken);
                });
        }
    }
}