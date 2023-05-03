using System.Collections.Generic;
using System.Linq;
using Adform.Ciam.SharedKernel.Entities;
using MediatR;
using Xunit;

namespace Adform.Bloom.Architecture.Test
{
    public class QueryHandlersTests
    {
        [Fact]
        public void Any_Class_Named_Query_Handler_Should_Implement_IRequestHandler()
        {
            var requestHandler = typeof(IRequestHandler<,>);
            foreach (var handler in Helper.QueryHandlers)
            {
                var interfaces = handler
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition());

                Assert.Contains(interfaces, i => i == requestHandler);
            }
        }

        [Fact]
        public void Any_Query_Handler_Should_Be_In_Read_Project()
        {
            foreach (var handler in Helper.QueryHandlers)
                Assert.Equal(handler.Assembly.FullName, Assemblies.ReadAssembly.FullName);
        }

        [Fact]
        public void Query_Handlers_Should_Return_Dtos()
        {
            var requestHandler = typeof(IRequestHandler<,>);
            var entityPagination = typeof(EntityPagination<>);
            var dataLoaderReturnType = typeof(IDictionary<,>);
            var entityCollection = typeof(IReadOnlyCollection<>);

            foreach (var handler in Helper.QueryHandlers)
            {
                var interfaces = handler.GetInterfaces()
                    .Where(i => i.IsGenericType);

                foreach (var i in interfaces)
                {
                    if (i.GetGenericTypeDefinition() == requestHandler)
                    {
                        var outputType = i.GenericTypeArguments[1];

                        if (outputType.IsGenericType && (outputType.GetGenericTypeDefinition() == entityPagination || outputType.GetGenericTypeDefinition() == entityCollection))
                            outputType = outputType.GenericTypeArguments[0];
                        else if (outputType.IsGenericType &&
                                 outputType.GetGenericTypeDefinition() == dataLoaderReturnType)
                            outputType = outputType.GenericTypeArguments[1].GenericTypeArguments[0];

                        if (outputType.IsGenericParameter)
                        {
                            foreach (var c in outputType.GetGenericParameterConstraints())
                            {
                                Assert.True(
                                    c.Assembly == Assemblies.ContractAssembly,
                                    $"Query handler {handler.Name} does not return a dto!");
                            }
                        }
                        else
                        {
                            Assert.True(
                                outputType.Assembly == Assemblies.ContractAssembly,
                                $"Query handler {handler.Name} does not return a dto!");
                        }
                    }
                }
            }
        }

        [Fact]
        public void There_Are_Some_QueryHandlers()
        {
            Assert.NotEmpty(Helper.QueryHandlers);
        }
    }
}