using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.LicensedFeature;
using Adform.Bloom.Api.Graph.Role;
using Adform.Bloom.Api.Graph.User;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.BusinessAccount
{
    public class BusinessAccountTypeExtensions : ObjectTypeExtension<Contracts.Output.BusinessAccount>
    {
        private readonly IMediator _mediator;

        public BusinessAccountTypeExtensions(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.BusinessAccount> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.BusinessAccount));

            descriptor
                .Field("roles")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<RoleType, Contracts.Output.Role>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.BusinessAccount>();
                    var (pagination, queryparams) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                   
                    return await _mediator.Send(new BaseAccessRangeQuery<Contracts.Output.BusinessAccount,
                        QueryParamsTenantIdsInput, Contracts.Output.Role>(userContext, parent, pagination.Offset,
                        pagination.Limit, queryparams), cancellationToken);
                });

            descriptor
                .Field("users")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<UserType, Contracts.Output.User>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.BusinessAccount>();
                    var (pagination, queryparams) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    return await _mediator.Send(new BaseAccessRangeQuery<Contracts.Output.BusinessAccount,
                        QueryParamsTenantIdsInput, Contracts.Output.User>(userContext, parent, pagination.Offset,
                        pagination.Limit, queryparams), cancellationToken);
                });

            descriptor
                .Field("licensedFeatures")
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<LicensedFeatureType, Contracts.Output.LicensedFeature>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.BusinessAccount>();
                    var pagination = context.ArgumentValue<PaginationInput>(Constants.Parameters.Pagination);
                    return await _mediator.Send(new LicensedFeaturesQuery(userContext, new QueryParamsTenantIdsAndPolicyTypesInput
                    {
                        TenantIds = new[] {parent.Id}
                    }, pagination.Offset, pagination.Limit), cancellationToken);
                });
        }
    }
}