using System;
using System.Collections.Generic;
using System.Security.Claims;
using Adform.Bloom.Application.Extensions;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Application.Validators;
using Adform.Bloom.Runtime.Host.Capabilities;
using Adform.Bloom.Runtime.Infrastructure;
using Adform.Bloom.Runtime.Read.Entities;
using Adform.Ciam.ExceptionHandling.Abstractions.Exceptions;
using HotChocolate;
using HotChocolate.Types;
using MediatR;

namespace Adform.Bloom.Runtime.Host.Graph.ExistenceCheck
{
    public class ExistenceCheckGQLQuery : ObjectTypeExtension
    {
        private readonly IMediator _mediator;
        private readonly IExistenceQueryValidator _validator;

        public ExistenceCheckGQLQuery(IMediator mediator, IExistenceQueryValidator validator)
        {
            _mediator = mediator;
            _validator = validator;
        }

        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(OperationTypeNames.Query);

            descriptor
                .Field("nodesExistCheck")
                .Description("Checks if provided nodes exist in graph.")
                .Argument(Constants.Parameters.Nodes, o => o.Type<NonNullType<ListType<NodeDescriptorInputType>>>())
                .Type<ExistenceResultType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.GetUser() ?? new ClaimsPrincipal();
                    var nodes = context.ArgumentValue<List<NodeDescriptor>>(Constants.Parameters.Nodes);
                    var query = new NodeExistenceQuery
                    {
                        NodeDescriptors = nodes
                    };
                    var validationResult = _validator.Validate(query);
                    if (!validationResult.IsValid)
                        throw new BadRequestException(message: validationResult.ToString());

                    var result = await _mediator.Send(query, cancellationToken);
                    return new ExistenceResult { Exists = result.ThrowIfException() };
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("legacyTenantsExistCheck")
                .Description("Checks if provided tenants exist.")
                .Argument(Constants.Parameters.TenantType, o => o.Type<NonNullType<StringType>>())
                .Argument(Constants.Parameters.TenantLegacyIds, o => o.Type<NonNullType<ListType<IntType>>>())
                .Type<ExistenceResultType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.GetUser() ?? new ClaimsPrincipal();
                    var tenantLegacyIds = context.ArgumentValue<List<int>>(Constants.Parameters.TenantLegacyIds);
                    var tenantType = context.ArgumentValue<string>(Constants.Parameters.TenantType);
                    
                    var query = new LegacyTenantExistenceQuery
                    {
                        TenantType = tenantType,
                        TenantLegacyIds = tenantLegacyIds
                    };
                    var validationResult = _validator.Validate(query);
                    if (!validationResult.IsValid)
                        throw new BadRequestException(message: validationResult.ToString());

                    var result = await _mediator.Send(query, cancellationToken);
                    return new ExistenceResult { Exists = result.ThrowIfException() };
                }).Authorize(StartupOAuth.Scopes.Readonly);

            descriptor
                .Field("roleExistsCheck")
                .Description("Checks if provided role exists under tenant.")
                .Argument(Constants.Parameters.TenantId, o => o.Type<NonNullType<IdType>>())
                .Argument(Constants.Parameters.RoleName, o => o.Type<NonNullType<StringType>>())
                .Type<ExistenceResultType>()
                .Resolve(async (context, cancellationToken) =>
                {
                    var userContext = context.GetUser() ?? new ClaimsPrincipal();
                    var tenantId = context.ArgumentValue<Guid>(Constants.Parameters.TenantId);
                    var roleName = context.ArgumentValue<string>(Constants.Parameters.RoleName);

                    var query = new RoleExistenceQuery
                    {
                        TenantId = tenantId,
                        RoleName = roleName
                    };
                    var validationResult = _validator.Validate(query);
                    if (!validationResult.IsValid)
                        throw new BadRequestException(message: validationResult.ToString());

                    var result = await _mediator.Send(query, cancellationToken);

                    return new ExistenceResult { Exists = result.ThrowIfException() };

                }).Authorize(StartupOAuth.Scopes.Readonly);
        }
    }
}