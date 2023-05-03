using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Infrastructure;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Runtime.Host.Graph.SubjectEvaluation
{
    public class SubjectEvaluationGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public SubjectEvaluationGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("subjectEvaluation")
                .Description("Evaluates subject assignment on global level or scoped by Tenant and/or Policy identifiers.")
                .Argument(Constants.Parameters.SubjectId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.TenantIds, o => o.Type<ListType<IdType>>())
                .Argument(Constants.Parameters.PolicyNames, o => o.Type<ListType<StringType>>())
                .Argument(Constants.Parameters.TenantLegacyIds, o => o.Type<ListType<IntType>>())
                .Argument(Constants.Parameters.TenantType, o => o.Type<StringType>())
                .Argument(Constants.Parameters.InheritanceEnabled, o => o.Type<NonNullType<BooleanType>>().DefaultValue(true))
                .Type<ListType<RuntimeResultType>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.GetUser() ?? new ClaimsPrincipal();
                    var query = new SubjectRuntimeQuery
                    {
                        SubjectId = context.ArgumentValue<Guid>(Constants.Parameters.SubjectId),
                        TenantIds =  context.ArgumentValue<IReadOnlyCollection<Guid>>(Constants.Parameters.TenantIds) ?? new List<Guid>(),
                        TenantType =  context.ArgumentValue<string?>(Constants.Parameters.TenantType),
                        TenantLegacyIds = context.ArgumentValue<IReadOnlyCollection<int>>(Constants.Parameters.TenantLegacyIds) ?? new List<int>(),
                        PolicyNames = context.ArgumentValue<IReadOnlyCollection<string>>(Constants.Parameters.PolicyNames) ?? new List<string>(),
                        InheritanceEnabled = context.ArgumentValue<bool>(Constants.Parameters.InheritanceEnabled)
                    };
                    return await _mediator.Send(query, cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}