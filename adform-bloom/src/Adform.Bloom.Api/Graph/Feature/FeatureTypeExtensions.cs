using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.LicensedFeature;
using Adform.Bloom.Api.Graph.Permission;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.ReadModel;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Read.Queries;
using HotChocolate.Types;
using MapsterMapper;
using MediatR;

namespace Adform.Bloom.Api.Graph.Feature;

public class FeatureTypeExtensions : ObjectTypeExtension<Contracts.Output.Feature>
{
    private readonly IAdminGraphRepository _repository;
    private readonly IBusinessAccountReadModelProvider _businessProvider;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public FeatureTypeExtensions(
        IAdminGraphRepository repository,
        IBusinessAccountReadModelProvider businessProvider, IMediator mediator, IMapper mapper)
    {
        _repository = repository;
        _businessProvider = businessProvider;
        _mediator = mediator;
        _mapper = mapper;
    }

    protected override void Configure(IObjectTypeDescriptor<Contracts.Output.Feature> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Name(nameof(Contracts.Output.Feature));

        descriptor
            .Field("permissions")
            .Description("Feature Permissions.")
            .Type<NonNullType<ListType<NonNullType<PermissionType>>>>()
            .Resolve(async (context, cancellationToken) =>
            {
                var userContext = context.ResolveUser();
                var parent = context.Parent<Contracts.Output.Feature>();
#warning MK 2020-03-02: Must be replaced with a query handler that takes into account data access rules.
                var result =
                    await _repository
                        .GetConnectedAsync<Domain.Entities.Feature, Domain.Entities.Permission>(o =>
                            o.Id == parent.Id, Constants.ContainsLink);
                return result.MapFromDomain<Domain.Entities.Permission, Contracts.Output.Permission>();
            });

        descriptor
            .Field("businessAccounts")
            .Description("Assigned Business Accounts.")
            .Argument(Constants.Parameters.Search, o => o.Type<StringType>())
            .Argument(Constants.Parameters.SortBy, o => o.Type<SortInputType>())
            .Argument(Constants.Parameters.Pagination, o => o.Type<NonNullType<PaginationInputType>>())
            .Type<PaginationType<BusinessAccountType, Contracts.Output.BusinessAccount>>()
            .Resolve(async (context, cancellationToken) =>
            {
                var userContext = context.ResolveUser();
                var parent = context.Parent<Contracts.Output.Feature>();
                var (pagination, queryParamsInput) = context.ResolveQueryParameters<QueryParamsInput>();
                return await _mediator.Send(
                    new BaseAccessRangeQuery<Contracts.Output.Feature, QueryParamsInput,
                        Contracts.Output.BusinessAccount>(userContext, parent, pagination.Offset, pagination.Limit,
                        queryParamsInput), cancellationToken);
            });

        descriptor
            .Field("licensedFeature")
            .Description("Licensed Feature.")
            .Type<NonNullType<LicensedFeatureType>>()
            .Resolve(async (context, cancellationToken) =>
            {
                var userContext = context.ResolveUser();
                var parent = context.Parent<Contracts.Output.Feature>();
                var dataLoader = context.DataLoader<LicensedFeatureByFeatureIdBatchDataLoader>();
                return await dataLoader.LoadAsync(parent.Id, cancellationToken);
            });

        descriptor
            .Field("features")
            .Description("Features that the feature depends on.")
            .Type<NonNullType<ListType<NonNullType<FeatureType>>>>()
            .Resolve(async (context, cancellationToken) =>
            {
                var userContext = context.ResolveUser();
                var parent = context.Parent<Contracts.Output.Feature>();
                var dataLoader = context.DataLoader<CoDependentFeaturesByFeatureIdBatchDataLoader>();
                return await dataLoader.LoadAsync(parent.Id, cancellationToken);
            });
    }
}