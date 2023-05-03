using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.LicensedFeature;

public class LicensedFeatureGQLMutation : ObjectTypeExtension
{
    private readonly IMediator _mediator;

    public LicensedFeatureGQLMutation(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(OperationTypeNames.Mutation);
    }
}