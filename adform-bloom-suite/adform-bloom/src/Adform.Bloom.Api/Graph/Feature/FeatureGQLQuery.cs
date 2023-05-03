using System;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class FeatureGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public FeatureGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("feature")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<FeatureType>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new FeatureQuery(userContext, id), cancellationToken);

                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("features")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<FeatureType, Contracts.Output.Feature>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    return await _mediator.Send(new FeaturesQuery(userContext, filter,
                        pagination.Offset, pagination.Limit), cancellationToken);

                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}