using System;
using System.Collections.Generic;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Handlers;
using Adform.Bloom.Read.Queries;
using Adform.Ciam.SharedKernel.Entities;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using LicensedFeature = Adform.Bloom.Contracts.Output.LicensedFeature;
using Permission = Adform.Bloom.Contracts.Output.Permission;
using Policy = Adform.Bloom.Contracts.Output.Policy;
using Role = Adform.Bloom.Contracts.Output.Role;
using Subject = Adform.Bloom.Contracts.Output.Subject;

namespace Adform.Bloom.Read.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureQueries(this IServiceCollection services)
        {
            ConfigureMappings(services);

            services.AddSingleton(typeof(IRequestHandler<PolicyQuery, Policy>),
                typeof(BaseSingleQueryHandler<PolicyQuery, QueryParamsTenantIdsInput,
                    QueryParamsTenantIds, Domain.Entities.Policy, Policy>));
            services.AddSingleton(typeof(IRequestHandler<PoliciesQuery, EntityPagination<Policy>>),
                typeof(BaseRangeQueryHandler<PoliciesQuery, QueryParamsTenantIdsInput, QueryParamsTenantIds, Policy>));

            services.AddSingleton(typeof(IRequestHandler<BusinessAccountQuery, BusinessAccount>),
                typeof(BusinessAccountSingleQueryHandler));
            services.AddSingleton(typeof(IRequestHandler<BusinessAccountsQuery, EntityPagination<BusinessAccount>>),
                typeof(BusinessAccountRangeQueryHandler));

            services.AddSingleton(typeof(IRequestHandler<UserQuery, User>),
                typeof(UserSingleQueryHandler));
            services.AddSingleton(typeof(IRequestHandler<UsersQuery, EntityPagination<User>>),
                typeof(UserRangeQueryHandler));

            services.AddSingleton(typeof(IRequestHandler<RoleQuery, Role>),
                typeof(RoleSingleQueryHandler));
            services.AddSingleton(typeof(IRequestHandler<RolesQuery, EntityPagination<Role>>),
                typeof(BaseRangeQueryHandler<RolesQuery, QueryParamsRolesInput, QueryParamsRoles, Role>));

            services.AddSingleton(typeof(IRequestHandler<PermissionQuery, Permission>),
                typeof(BaseSingleQueryHandler<PermissionQuery, QueryParamsTenantIdsInput,
                    QueryParamsTenantIds, Domain.Entities.Permission, Permission>));
            services.AddSingleton(typeof(IRequestHandler<PermissionsQuery, EntityPagination<Permission>>),
                typeof(BaseRangeQueryHandler<PermissionsQuery, QueryParamsTenantIdsInput, QueryParamsTenantIds, Permission>));

            services.AddSingleton(typeof(IRequestHandler<FeatureQuery, Feature>),
                typeof(BaseSingleQueryHandler<FeatureQuery, QueryParamsTenantIdsInput,
                    QueryParamsTenantIds, Domain.Entities.Feature, Feature>));
            services.AddSingleton(typeof(IRequestHandler<FeaturesQuery, EntityPagination<Feature>>),
                typeof(BaseRangeQueryHandler<FeaturesQuery, QueryParamsTenantIdsInput, QueryParamsTenantIds, Feature>));

            services.AddSingleton(typeof(IRequestHandler<LicensedFeatureQuery, LicensedFeature>),
                typeof(BaseSingleQueryHandler<LicensedFeatureQuery, QueryParamsTenantIdsAndPolicyTypesInput,
                    QueryParamsTenantIdsAndPolicyTypes, Domain.Entities.LicensedFeature, LicensedFeature>));
            services.AddSingleton(typeof(IRequestHandler<LicensedFeaturesQuery, EntityPagination<LicensedFeature>>),
                typeof(BaseRangeQueryHandler<LicensedFeaturesQuery, QueryParamsTenantIdsAndPolicyTypesInput, QueryParamsTenantIdsAndPolicyTypes,
                    LicensedFeature>));

            services.AddSingleton(typeof(IRequestHandler<CoDependentFeaturesQuery, IDictionary<Guid, List<Feature>>>),
                typeof(CoDependentFeaturesQueryHandler));


            services.AddSingleton(
                typeof(IRequestHandler<PermissionBusinessAccountsQuery, IReadOnlyCollection<BusinessAccount>>),
                typeof(PermissionBusinessAccountRangeQueryHandler));

            
            services.AddSingleton(
                typeof(IRequestHandler<BaseAccessRangeQuery<BusinessAccount, QueryParamsTenantIdsInput, Role>, EntityPagination<Role>>),
                typeof(BaseAccessRangeQueryHandler<BaseAccessRangeQuery<BusinessAccount, QueryParamsTenantIdsInput, Role>,
                    BusinessAccount, QueryParamsTenantIdsInput, QueryParamsTenantIds, Role>));
           
            services.AddSingleton(
                typeof(IRequestHandler<BaseAccessRangeQuery<Subject, QueryParamsTenantIdsInput, Role>, EntityPagination<Role>>),
                typeof(BaseAccessRangeQueryHandler<BaseAccessRangeQuery<Subject, QueryParamsTenantIdsInput, Role>, 
                    Subject, QueryParamsTenantIdsInput, QueryParamsTenantIds, Role>));
            
            services.AddSingleton(
                typeof(IRequestHandler<BaseAccessRangeQuery<BusinessAccount, QueryParamsTenantIdsInput, User>, EntityPagination<User>>),
                typeof(BaseAccessRangeQueryHandler<BaseAccessRangeQuery<BusinessAccount, QueryParamsTenantIdsInput, User>,
                    BusinessAccount, QueryParamsTenantIdsInput, QueryParamsTenantIds, User>));

            services.AddSingleton(
                typeof(IRequestHandler<BaseAccessRangeQuery<Role, QueryParamsTenantIdsInput, Feature>, EntityPagination<Feature>>),
                typeof(BaseAccessRangeQueryHandler<BaseAccessRangeQuery<Role, QueryParamsTenantIdsInput, Feature>,
                    Role, QueryParamsTenantIdsInput, QueryParamsTenantIds, Feature>));

            services
                .AddSingleton<IRequestHandler<BaseAccessRangeQuery<Feature, QueryParamsInput, BusinessAccount>,
                    EntityPagination<BusinessAccount>>, BaseAccessRangeQueryHandler<BaseAccessRangeQuery<Feature, QueryParamsInput, BusinessAccount>, Feature, QueryParamsInput, QueryParams, BusinessAccount>>();
            
            return services;
        }

        public static IServiceCollection ConfigureMappings(this IServiceCollection services)
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<QueryParamsTenantIdsInput, QueryParamsTenantIds>()
                .Map(dest => dest.Search, src => src.Search)
                .Map(dest => dest.OrderBy, src => src.FieldName)
                .Map(dest => dest.SortingOrder, src => src.Order)
                .Map(dest => dest.ContextId, src => src.ContextId)
                .Map(dest => dest.ResourceIds, src => src.ResourceIds)
                .Map(dest => dest.TenantIds, src => src.TenantIds);
            config.NewConfig<QueryParamsRolesInput, QueryParamsRoles>()
                .Map(dest => dest.Search, src => src.Search)
                .Map(dest => dest.OrderBy, src => src.FieldName)
                .Map(dest => dest.ContextId, src => src.ContextId)
                .Map(dest => dest.SortingOrder, src => src.Order)
                .Map(dest => dest.ResourceIds, src => src.ResourceIds)
                .Map(dest => dest.PrioritizeTemplateRoles, src => src.PrioritizeTemplateRoles);
            config.NewConfig<QueryParamsBusinessAccountInput, QueryParamsBusinessAccount>()
                .Map(dest => dest.Search, src => src.Search)
                .Map(dest => dest.OrderBy, src => src.FieldName)
                .Map(dest => dest.ContextId, src => src.ContextId)
                .Map(dest => dest.SortingOrder, src => src.Order)
                .Map(dest => dest.ResourceIds, src => src.ResourceIds)
                .Map(dest => dest.BusinessAccountType, src => src.BusinessAccountType);
            config.NewConfig<QueryParamsTenantIdsAndPolicyTypesInput, QueryParamsTenantIdsAndPolicyTypes>()
                .Map(dest => dest.Search, src => src.Search)
                .Map(dest => dest.ContextId, src => src.ContextId)
                .Map(dest => dest.OrderBy, src => src.FieldName)
                .Map(dest => dest.SortingOrder, src => src.Order)
                .Map(dest => dest.ResourceIds, src => src.ResourceIds)
                .Map(dest => dest.TenantIds, src => src.TenantIds)
                .Map(dest => dest.PolicyTypes, src => src.PolicyTypes);

            services.AddSingleton(config);
            services.AddSingleton<IMapper, ServiceMapper>();

            return services;
        }
    }
}