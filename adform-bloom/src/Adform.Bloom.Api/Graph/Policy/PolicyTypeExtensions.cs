using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Role;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class PolicyTypeExtensions : ObjectTypeExtension<Contracts.Output.Policy>
    {
        private readonly IMediator _mediator;
        
        public PolicyTypeExtensions(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Policy> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.Policy));

            descriptor
                .Field("roles")
                .Description("Policy Roles.")
                .Type<NonNullType<ListType<NonNullType<RoleType>>>>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();
                    var parent = context.Parent<Contracts.Output.Policy>();
                    var dataLoader = context.DataLoader<RolesByPolicyIdBatchDataLoader>();
                    return await dataLoader.LoadAsync(parent.Id, cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Full);
        }
    }
}