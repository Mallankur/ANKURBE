using System;
using System.Collections.Generic;
using System.Linq;

namespace Adform.Bloom.Architecture.Test
{
    public static class Helper
    {
        public const string CommandHandler = "CommandHandler";
        public const string QueryHandler = "QueryHandler";
        public const string Adform = "Adform";

        public static readonly IEnumerable<Type> AllClasses = Assemblies.AllBloomAssemblies
            .Where(a => a.FullName.StartsWith(Adform))
            .SelectMany(a => a.GetTypes()).Where(t => t.IsClass);

        public static readonly IEnumerable<Type> CommandHandlers = AllClasses
            .Where(c => !c.IsAbstract)
            .Where(c => c.Name.Contains(CommandHandler))
            .ToList();

        public static readonly IEnumerable<Type> QueryHandlers = AllClasses
            .Where(c => !c.IsAbstract)
            .Where(c => c.Name.Contains(QueryHandler))
            .ToList();
    }
}
