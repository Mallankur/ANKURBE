using System;
using System.Collections.Generic;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Policy;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.LicensedFeature
{
    public class LicensedFeatureGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public LicensedFeatureGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("licensedFeature")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<LicensedFeatureType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    var tenantIds =
                        context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.BusinessAccountIds);
                    return await _mediator.Send(new LicensedFeatureQuery(userContext, id,
                        new QueryParamsTenantIdsAndPolicyTypesInput
                        {
                            TenantIds = tenantIds
                        }), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("licensedFeatures")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Argument(Constants.Parameters.ProductNames, o => o.Type<ListType<NonNullType<PolicyTypeInputEnum>>>())
                .Type<PaginationType<LicensedFeatureType, Contracts.Output.LicensedFeature>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) =
                        context.ResolveQueryParameters<QueryParamsTenantIdsAndPolicyTypesInput>();
                    var result = await _mediator.Send(new LicensedFeaturesQuery(userContext, filter,
                        pagination.Offset, pagination.Limit), cancellationToken);
                    return result;
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}