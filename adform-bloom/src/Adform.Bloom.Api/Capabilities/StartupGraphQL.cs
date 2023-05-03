using Adform.Bloom.Api.Graph.BusinessAccount;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Api.Graph.Feature;
using Adform.Bloom.Api.Graph.LicensedFeature;
using Adform.Bloom.Api.Graph.Permission;
using Adform.Bloom.Api.Graph.Policy;
using Adform.Bloom.Api.Graph.Role;
using Adform.Bloom.Api.Graph.User;
using Adform.Ciam.GraphQLAdvanced.Extensions;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Adform.Bloom.Api.Graph.PermissionBusinessAccount;
using HotChocolate.Types.Descriptors;

namespace Adform.Bloom.Api.Capabilities
{
    public static class StartupGraphQL
    {

        public static IServiceCollection ConfigureGraphql(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureGraphQL(configuration, (builder) =>
            {
                builder
                    .AddTypeConverter<EnumIntConverter>()
                    .AddTypeConverter<GuidConverter>()
                    .AddConvention<INamingConventions, EnumCompatibleNamingConvention>()
                    .AddQueryType()
                    .AddMutationType()
                    .BusinessAccountsGQL()
                    .UsersGQL()
                    .PoliciesGQL()
                    .RolesGQL()
                    .PermissionsGQL()
                    .FeaturesGQL()
                    .LicensedFeaturesGQL()
                    .PermissionBusinessAccountsGQL();
            });

            return services;
        }

        private static IRequestExecutorBuilder BusinessAccountsGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<BusinessAccountGQLQuery>()
                .AddTypeExtension<BusinessAccountGQLMutation>()
                .AddType<BusinessAccountType>()
                .AddType<BusinessAccountTypeEnum>()
                .AddType<BusinessAccountStatusEnum>()
                .AddTypeExtension<BusinessAccountTypeExtensions>();
        }

        private static IRequestExecutorBuilder UsersGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<UserGQLQuery>()
                .AddTypeExtension<UserGQLMutation>()
                .AddType<UserType>()
                .AddType<UserStatusTypeEnum>()
                .AddTypeExtension<UserTypeExtensions>();
        }

        private static IRequestExecutorBuilder PoliciesGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<PolicyGQLQuery>()
                .AddTypeExtension<PolicyGQLMutation>()
                .AddType<PolicyType>()
                .AddTypeExtension<PolicyTypeExtensions>()
                .AddType<RolesByPolicyIdBatchDataLoader>();
        }

        private static IRequestExecutorBuilder RolesGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<RoleGQLQuery>()
                .AddTypeExtension<RoleGQLMutation>()
                .AddType<RoleType>()
                .AddTypeExtension<RoleTypeExtensions>()
                .AddDataLoader<BusinessAccountByRoleIdBatchDataLoader>();
        }

        private static IRequestExecutorBuilder PermissionsGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<PermissionGQLQuery>()
                .AddTypeExtension<PermissionGQLMutation>()
                .AddType<PermissionType>()
                .AddTypeExtension<PermissionTypeExtensions>()
                .AddDataLoader<FeatureByPermissionIdBatchDataLoader>();
        }

        private static IRequestExecutorBuilder FeaturesGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<FeatureGQLQuery>()
                .AddTypeExtension<FeatureGQLMutation>()
                .AddType<FeatureType>()
                .AddTypeExtension<FeatureTypeExtensions>()
                .AddDataLoader<LicensedFeatureByFeatureIdBatchDataLoader>()
                .AddDataLoader<CoDependentFeaturesByFeatureIdBatchDataLoader>();
        }

        private static IRequestExecutorBuilder LicensedFeaturesGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<LicensedFeatureGQLQuery>()
                .AddTypeExtension<LicensedFeatureGQLMutation>()
                .AddType<LicensedFeatureType>()
                .AddTypeExtension<LicensedFeatureExtensions>();
        }

        private static IRequestExecutorBuilder PermissionBusinessAccountsGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<PermissionBusinessAccountGQLQuery>();
        }
    }
}