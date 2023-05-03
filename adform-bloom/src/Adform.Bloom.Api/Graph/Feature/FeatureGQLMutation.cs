using System;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Feature
{
    public class FeatureGQLMutation : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public FeatureGQLMutation(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Mutation);

            descriptor
                .Field("createFeature")
                .Argument(Constants.Parameters.Feature, o => o.Type<NonNullType<FeatureInputType>>())
                .Type<FeatureType>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();                 
                    var feature = context.ArgumentValue<Contracts.Output.Feature>(Constants.Parameters.Feature);
                    var result = await _mediator.Send(new CreateFeatureCommand(userContext ,feature.Name,
                        feature.Description,
                        feature.Enabled), cancellationToken);
                    return result.MapFromDomain<Domain.Entities.Feature, Contracts.Output.Feature>();
                }).Authorize(StartupOAuth.Scopes.Full);
            
            descriptor
                .Field("deleteFeature")
                .Description("Delete a Feature by its identifier")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();                    
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    await _mediator.Send(new DeleteFeatureCommand(userContext ,id), cancellationToken);
                    return id;
                }).Authorize(StartupOAuth.Scopes.Full);


            descriptor
                .Field("updatePermissionToFeatureAssignments")
                .Description("Assign or unassign a Permission to a Feature by their identifiers")             
                .Deprecated()
                .Argument(Constants.Parameters.Assignment, o => o.Type<NonNullType<AssignPermissionToFeatureInputType>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var assignment = context.ArgumentValue<AssignPermissionToFeature>(Constants.Parameters.Assignment);
                    await _mediator.Send(new AssignPermissionToFeatureCommand(userContext, assignment.FeatureId,
                        assignment.PermissionId, assignment.Operation.MapToDomain()), cancellationToken);
                    return assignment.FeatureId;
                }).Authorize(StartupOAuth.Scopes.Full);

            descriptor
                .Field("updateFeatureCoDependency")
                .Description("Create or destroy Co-Dependency between features")
                .Argument(Constants.Parameters.FeatureId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.DependentOnId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.Operation, o => o.Type<NonNullType<LinkOperationTypeEnum>>())
                .Type<NonNullType<IdType>>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();                    
                    var featureId = context.ArgumentValue<Guid>(Constants.Parameters.FeatureId);
                    var dependentOnId = context.ArgumentValue<Guid>(Constants.Parameters.DependentOnId);
                    var operation = context.ArgumentValue<LinkOperation>(Constants.Parameters.Operation);
                    await _mediator.Send(new AssignFeatureCoDependencyCommand(userContext ,featureId,
                        dependentOnId, operation.MapToDomain()), cancellationToken);
                    return featureId;
                }).Authorize(StartupOAuth.Scopes.Full);
        }
    }
}