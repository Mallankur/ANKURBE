using System;
using System.Collections.Generic;
using Adform.Bloom.Api.Capabilities;
using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Api.Graph.PermissionBusinessAccount
{
    public class PermissionBusinessAccountGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;

        public PermissionBusinessAccountGQLQuery(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("permissionBusinessAccounts")
                .Argument(Constants.Parameters.UserId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.PermissionNames, o => o.Type<NonNullType<ListType<StringType>>>())
                .Argument(Constants.Parameters.PermissionBusinessAccountEvaluationParameter, o => o.Type<NonNullType<PermissionBusinessAccountEvaluationParameterEnum>>())
                .Argument(Constants.Parameters.BusinessAccountIds, o => o.Type<ListType<NonNullType<IdType>>>())
                .Type<NonNullType<ListType<BusinessAccountType>>>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.ResolveUser();
                    var subjectId = context.ArgumentValue<Guid>(Constants.Parameters.UserId);
                    var permissionNames =
                        context.ArgumentValue<IReadOnlyCollection<string>>(Constants.Parameters.PermissionNames);
                    var evaluationParameter =
                        context.ArgumentValue<EvaluationParameter>(Constants.Parameters.PermissionBusinessAccountEvaluationParameter);
                    var tenantIds =
                        context.ArgumentValue<IReadOnlyCollection<Guid>?>(Constants.Parameters.BusinessAccountIds);

                    return await _mediator.Send(new PermissionBusinessAccountsQuery(userContext, subjectId, permissionNames,
                            evaluationParameter, tenantIds),
                        cancellationToken);
                }).Authorize(StartupOAuth.Scopes.Readonly).Authorize(ClaimPrincipalExtensions.AdformAdmin,
                    ClaimPrincipalExtensions.LocalAdmin);
        }
    }
}