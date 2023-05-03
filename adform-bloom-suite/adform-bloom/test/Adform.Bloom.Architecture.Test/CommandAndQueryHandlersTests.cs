using MediatR;
using System.Linq;
using Xunit;

namespace Adform.Bloom.Architecture.Test
{
    public class CommandAndQueryHandlersTests
    {
        
        [Fact]
        public void Any_Class_That_Implements_IRequestHandler_Should_Be_Called_Command_Or_Query_Handler()
        {
            var requestHandler1 = typeof(IRequestHandler<>);
            var requestHandler2 = typeof(IRequestHandler<,>);
            foreach (var c in Helper.AllClasses.Where(c => !c.IsAbstract))
            {
                var interfaces = c.GetInterfaces().Where(i => i.IsGenericType).Select(i => i.GetGenericTypeDefinition());
                if (interfaces.Any(i => i == requestHandler1 || i == requestHandler2))
                    Assert.True(c.Name.Contains(Helper.CommandHandler) || c.Name.Contains(Helper.QueryHandler),
                        $"{c.Name} has wrong name!");
            }
        }
    }
}