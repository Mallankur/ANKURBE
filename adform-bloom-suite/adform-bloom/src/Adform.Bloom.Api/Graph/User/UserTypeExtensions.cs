using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Role;
using Adform.Bloom.Api.Services;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.User
{
    public class UserTypeExtensions : ObjectTypeExtension<Contracts.Output.User>
    {
        private readonly IMediator _mediator;
        private readonly IClaimPrincipalGenerator _claimPrincipalGenerator;

        public UserTypeExtensions(IMediator mediator, IClaimPrincipalGenerator claimPrincipalGenerator)
        {
            _mediator = mediator;
            _claimPrincipalGenerator = claimPrincipalGenerator;
        }

        protected override void Configure(IObjectTypeDescriptor<Contracts.Output.User> descriptor)
        {
            base.Configure(descriptor);

            descriptor.Name(nameof(Contracts.Output.User));

            descriptor
                .Field("businessAccounts")
                .Description("Works for Business Accounts.")
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.BusinessAccountType, o => o.Type<BusinessAccountTypeEnum>())
                .Type<PaginationType<BusinessAccountType, Contracts.Output.BusinessAccount>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    //TODO: REMOVE THE CLAIMGENERATOR AND IMPLEMENT AN ACCESSPROVIDER
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsBusinessAccountInput>();
                    var parent = context.Parent<Contracts.Output.User>();
                    var subjectClaims = await _claimPrincipalGenerator.GenerateAsync(parent.Id, userContext, cancellationToken);
                    return await _mediator.Send(new BusinessAccountsQuery(subjectClaims, filter, pagination.Offset,
                        pagination.Limit), cancellationToken);
                });

            descriptor
                .Field("roles")
                .Description("Assigned Roles.")
                .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
                .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
                .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<PaginationType<RoleType, Contracts.Output.Role>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var (pagination, filter) = context.ResolveQueryParameters<QueryParamsTenantIdsInput>();
                    var parent = context.Parent<Contracts.Output.User>();
                    return await _mediator.Send(new BaseAccessRangeQuery<Subject, QueryParamsTenantIdsInput, Contracts.Output.Role>(
                            userContext, new Subject() {Id = parent.Id}, pagination.Offset, pagination.Limit, filter), cancellationToken);
                });
        }
    }
}