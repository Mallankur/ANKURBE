using Adform.Bloom.Runtime.Host.Graph.ExistenceCheck;
using Adform.Bloom.Runtime.Host.Graph.SubjectEvaluation;
using Adform.Ciam.GraphQLAdvanced.Extensions;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Runtime.Host.Capabilities
{
    public static class StartupGraphQL
    {
        public static IServiceCollection ConfigureGraphql(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureGraphQL(configuration, (builder) =>
            {
                builder
                    .AddQueryType(p => p.Name(OperationTypeNames.Query))
                    .EvaluationGQL()
                    .ExistenceGQL();
            });
         
            return services;
        }

        private static IRequestExecutorBuilder EvaluationGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<SubjectEvaluationGQLQuery>()
                .AddType<RuntimeResultType>()
                .AddType<SubjectRuntimeQueryInputType>();
        }

        private static IRequestExecutorBuilder ExistenceGQL(this IRequestExecutorBuilder builder)
        {
            return builder.AddTypeExtension<ExistenceCheckGQLQuery>()
                .AddType<ExistenceResultType>();
        }
    }
}