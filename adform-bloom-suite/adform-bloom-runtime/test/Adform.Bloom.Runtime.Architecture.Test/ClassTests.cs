using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Xunit;

namespace Adform.Bloom.Runtime.Architecture.Test
{
    public class ClassTests
    {
        private const string CommandHandler = "CommandHandler";
        private const string QueryHandler = "QueryHandler";
        private const string Adform = "Adform";

        private static readonly IEnumerable<Type> AllClasses = Assemblies.AllBloomRuntimeAssemblies
            .Where(a => a.FullName.StartsWith(Adform))
            .SelectMany(a => a.GetTypes()).Where(t => t.IsClass);

        private static readonly IEnumerable<Type> CommandHandlers = AllClasses
            .Where(c => !c.IsAbstract)
            .Where(c => c.Name.Contains(CommandHandler))
            .ToList();

        private static readonly IEnumerable<Type> QueryHandlers = AllClasses
            .Where(c => !c.IsAbstract)
            .Where(c => c.Name.Contains(QueryHandler))
            .ToList();

        [Fact]
        public void There_Are_No_Command_Handlers()
        {
            Assert.Empty(CommandHandlers);
        }

        [Fact]
        public void There_Are_Some_QueryHandlers()
        {
            Assert.NotEmpty(QueryHandlers);
        }

        [Fact]
        public void Any_Class_Named_Query_Handler_Should_Implement_IRequestHandler()
        {
            var requestHandler = typeof(IRequestHandler<,>);
            foreach (var handler in QueryHandlers)
            {
                var interfaces = handler.GetInterfaces();
                Assert.Contains(interfaces, i => i.Name == requestHandler.Name);
            }
        }

        [Fact]
        public void Any_Query_Handler_Should_Be_In_Read_Project()
        {
            foreach (var handler in QueryHandlers)
            {
                Assert.Equal(handler.Assembly.FullName, Assemblies.ReadAssembly.FullName);
            }
        }

        [Fact]
        public void Any_Class_That_Implements_IRequestHandler_Should_Be_Called_Command_Or_Query_Handler()
        {
            var requestHandler1 = typeof(IRequestHandler<>);
            var requestHandler2 = typeof(IRequestHandler<,>);
            foreach (var c in AllClasses.Where(c => !c.IsAbstract))
            {
                var interfaces = c.GetInterfaces();
                if (interfaces.Any(i => i.Name == requestHandler1.Name || i.Name == requestHandler2.Name))
                {
                    Assert.True(c.Name.Contains(CommandHandler) || c.Name.Contains(QueryHandler),
                        $"{c.Name} has wrong name!");
                }
            }
        }
    }
}