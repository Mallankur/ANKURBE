using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Feature;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Permission
{
    public class PermissionTypeExtensions : ObjectTypeExtension<Contracts.Output.Permission>
    {
        private readonly IMediator _mediator;

        public PermissionTypeExtensions(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Permission> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Permission));

            descriptor
                .Field("feature")
                .Type<NonNullType<FeatureType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var dataLoader = context.DataLoader<FeatureByPermissionIdBatchDataLoader>();
                    var parent = context.Parent<Contracts.Output.Permission>();
                    return await dataLoader.LoadAsync(parent.Id, cancellationToken);
                });
        }
    }
}