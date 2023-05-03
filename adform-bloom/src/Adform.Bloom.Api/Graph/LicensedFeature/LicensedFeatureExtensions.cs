using System;
using System.Collections.Generic;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Feature;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.LicensedFeature
{
    public class LicensedFeatureExtensions : ObjectTypeExtension<Contracts.Output.LicensedFeature>
    {
        private readonly IMediator _mediator;

        public LicensedFeatureExtensions(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.LicensedFeature> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.LicensedFeature));

            descriptor
                .Field("features")
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<PaginationType<FeatureType, Contracts.Output.Feature>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.LicensedFeature>();
                    var tenantIds = context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.BusinessAccountIds);
                    var filter = new QueryParamsTenantIdsInput
                    {
                        ContextId = parent.Id,
                        TenantIds = tenantIds
                    };
                    return await _mediator.Send(new FeaturesQuery(userContext, filter: filter), cancellationToken);
                });

        }
    }
}