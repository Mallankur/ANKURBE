using MediatR;
using System.Linq;
using Xunit;

namespace Adform.Bloom.Architecture.Test
{
    public class CommandHandlersTests
    {
        [Fact]
        public void Any_Class_Named_Command_Handler_Should_Implement_IRequestHandler()
        {
            var requestHandler1 = typeof(IRequestHandler<>);
            var requestHandler2 = typeof(IRequestHandler<,>);

            foreach (var handler in Helper.CommandHandlers)
            {
                var interfaces = handler
                    .GetInterfaces()
                    .Where(i => i.IsGenericType)
                    .Select(i => i.GetGenericTypeDefinition());

                Assert.Contains(interfaces, i => 
                    i == requestHandler1 
                    || 
                    i == requestHandler2);
            }
        }

        [Fact]
        public void Any_Command_Handler_Should_Be_In_Write_Project()
        {
            foreach (var handler in Helper.CommandHandlers)
                Assert.Equal(handler.Assembly.FullName, Assemblies.WriteAssembly.FullName);
        }


        [Fact]
        public void There_Are_Some_Command_Handlers()
        {
            Assert.NotEmpty(Helper.CommandHandlers);
        }
    }
}