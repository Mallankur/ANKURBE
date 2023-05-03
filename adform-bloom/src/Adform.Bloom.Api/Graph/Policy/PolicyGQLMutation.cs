using System;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class PolicyGQLMutation : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public PolicyGQLMutation(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor
                .Field("createPolicy")
                .Description(  "Create a Policy in the Policy tree and define it's level on the hierarchy by providing its parentId")
                .Argument(Constants.Parameters.ParentId, o => o.Type<IdType>())
                .Argument(Constants.Parameters.Policy, o => o.Type<NonNullType<PolicyInputType>>())
                .Type<PolicyType>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();
                    var parentId = context.ArgumentValue<Guid?>(Constants.Parameters.ParentId);
                    var policy = context.ArgumentValue<Contracts.Output.Policy>(Constants.Parameters.Policy);
                    var result =  await _mediator.Send(new CreatePolicyCommand(userContext,
                        parentId,
                        policy.Name,
                        policy.Description,
                        policy.Enabled), cancellationToken);
                    return result.MapFromDomain<Domain.Entities.Policy, Contracts.Output.Policy>();
                }).Authorize(StartupOAuth.Scopes.Full);
            
            descriptor
                .Field("deletePolicy")
                .Description(  "Delete a Policy in the Policy tree by its identifier")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    await _mediator.Send(new DeletePolicyCommand(userContext, id), cancellationToken);
                    return id;
                }).Authorize(StartupOAuth.Scopes.Full);
        }
    }
}