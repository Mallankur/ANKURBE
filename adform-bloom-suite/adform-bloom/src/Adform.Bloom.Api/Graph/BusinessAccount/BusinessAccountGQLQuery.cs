using System;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.BusinessAccount
{
    public class BusinessAccountGQLQuery: ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public BusinessAccountGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("businessAccount")
                .Argument(Constants.Parameters.Id, o => o.Type<NonNullType<IdType>>())
                .Type<BusinessAccountType>()
                .Resolve(async (context, cancellationToken)  =>
                {
                    var userContext = context.ResolveUser();
                    var id = context.ArgumentValue<Guid>(Constants.Parameters.Id);
                    return await _mediator.Send(new BusinessAccountQuery(userContext, id), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("businessAccounts")
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.BusinessAccountType, o => o.Type<BusinessAccountTypeEnum>())
                .Argument(Constants.Parameters.Pagination, o=>o.Type<NonNullType<PaginationInputType>>())
                .Type<PaginationType<BusinessAccountType, Contracts.Output.BusinessAccount>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsBusinessAccountInput>();
                    return await _mediator.Send(new BusinessAccountsQuery(userContext, filter,
                        pagination.Offset, pagination.Limit), cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}