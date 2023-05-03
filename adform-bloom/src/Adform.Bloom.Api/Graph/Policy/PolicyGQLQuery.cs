using System;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.Policy
{
    public class PolicyGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public PolicyGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("policy")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<PolicyType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new PolicyQuery(userContext, id), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("policies")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<PolicyType, Contracts.Output.Policy>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    return await _mediator.Send(
                        new PoliciesQuery(userContext, filter, pagination.Offset, pagination.Limit), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}