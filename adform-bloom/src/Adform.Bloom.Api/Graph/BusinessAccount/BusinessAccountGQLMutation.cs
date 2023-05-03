using System;
using System.Collections.Generic;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Write.Commands;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.BusinessAccount;

public class BusinessAccountGQLMutation : ObjectTypeExtension
{
    private readonly IMediator _mediator;

    public BusinessAccountGQLMutation(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override void Configure(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name(OperationTypeNames.Mutation);
        descriptor
            .Field("updateBusinessAccountToLicensedFeaturesAssignments")
            .Description("Assign or unassign Business Account to LicensedFeatures by their identifiers.")
            .Argument(Constants.Parameters.BusinessAccountId, o => o.Type<NonNullType<IdType>>())
            .Argument(Constants.Parameters.AssignLicensedFeatureIds, o => o.Type<ListType<NonNullType<IdType>>>())
            .Argument(Constants.Parameters.UnassignLicensedFeatureIds, o => o.Type<ListType<NonNullType<IdType>>>())
            .Type<NonNullType<IdType>>()
            .Resolve(async (context, cancellationToken) =>
            {
                var userContext = context.ResolveUser();
                var tenantId = context.ArgumentValue<Guid>(Constants.Parameters.BusinessAccountId);
                var licensedFeatureAssignments =
                    context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.AssignLicensedFeatureIds);
                var licensedFeatureUnassignments =
                    context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.UnassignLicensedFeatureIds);
                await _mediator.Send(new UpdateLicensedFeatureToTenantAssignmentsCommand(userContext,
                    tenantId, licensedFeatureAssignments, licensedFeatureUnassignments), cancellationToken);
                return tenantId;
            }).Authorize(StartupOAuth.Scopes.Full);
    }
}