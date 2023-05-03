using System;
using System.Collections.Generic;
using System.Linq;
using Adform.Ciam.Monitoring.Abstractions.Provider;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Adform.Bloom.Mediatr.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterMeasuredMediatrBehavior(this IServiceCollection services,
            string cqrsPostfix, string histogramName)
        {
            var requestHandlerType = typeof(IRequestHandler<,>);
            var pipelineType = typeof(IPipelineBehavior<,>);
            var behaviorType = typeof(MeasuredPipelineBehavior<,>);

            var interfaces = GetRegisteredGenericInterfaces(services, requestHandlerType, cqrsPostfix);

            if (!interfaces.Any())
            {
                throw new InvalidOperationException("Couldn't find any command/query handler interfaces");
            }

            foreach (var i in interfaces)
            {
                var pipelineConcreteType = pipelineType.MakeGenericType(i.GenericTypeArguments);
                var concreteBehaviorType = behaviorType.MakeGenericType(i.GenericTypeArguments);

                if (!services.Any(d => d.Lifetime == ServiceLifetime.Singleton &&
                                       d.ServiceType == pipelineConcreteType &&
                                       d.ImplementationType == concreteBehaviorType &&
                                       d.ImplementationFactory == null &&
                                       d.ImplementationInstance == null))
                    services.AddSingleton(pipelineConcreteType,
                        sp => Activator.CreateInstance(concreteBehaviorType,
                            sp.GetRequiredService<IMetricsProvider>()
                                .GetHistogram(histogramName))!);
            }
        }
        
        public static void RegisterTransactionalBehaviors(this IServiceCollection services,
            string commandHandlersPostfix)
        {
            var requestHandlerType = typeof(IRequestHandler<,>);
            var pipelineType = typeof(IPipelineBehavior<,>);
            var behaviorType = typeof(UnitOfWorkBehavior<,>);

            var interfaces = GetRegisteredGenericInterfaces(services, requestHandlerType, commandHandlersPostfix);

            if (!interfaces.Any())
            {
                throw new InvalidOperationException("Couldn't find any command/query handler interfaces");
            }
            
            foreach (var i in interfaces)
            {
                var pipelineConcreteType = pipelineType.MakeGenericType(i.GenericTypeArguments);
                var concreteBehaviorType = behaviorType.MakeGenericType(i.GenericTypeArguments);

                if (!services.Any(d => d.Lifetime == ServiceLifetime.Singleton &&
                                       d.ServiceType == pipelineConcreteType &&
                                       d.ImplementationType == concreteBehaviorType &&
                                       d.ImplementationFactory == null &&
                                       d.ImplementationInstance == null))
                    services.AddSingleton(pipelineConcreteType, concreteBehaviorType);
            }
        }

        private static IEnumerable<Type> GetRegisteredGenericInterfaces(IServiceCollection services, Type genericInterface,
            string postfix)
        {
            var registeredHandlers = services
                .Where(s => (s.ImplementationType?.Name ?? string.Empty).Contains(postfix))
                .ToArray();

            var types = registeredHandlers
                .Select(sd => sd.ImplementationType)
                .Where(t => t!.IsClass && !t.IsAbstract);

            var interfaces = types
                .SelectMany(t => t!.GetInterfaces())
                .Where(i =>
                    i.Name == genericInterface.Name &&
                    i.Namespace == genericInterface.Namespace)
                .Distinct();

            return interfaces;
        }
    }
}