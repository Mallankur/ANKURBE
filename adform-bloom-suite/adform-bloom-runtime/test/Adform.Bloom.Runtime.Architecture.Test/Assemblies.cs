using System.Linq;
using System.Reflection;
using Adform.Bloom.Application.Handlers;
using Adform.Bloom.Runtime.Host.Controllers;
using Adform.Ciam.ExceptionHandling.Middlewares;
using Adform.Ciam.Monitoring.CustomStructures;

namespace Adform.Bloom.Runtime.Architecture.Test
{
    public static class Assemblies
    {
        public static readonly Assembly ApiAssembly = typeof(RuntimeController).Assembly;
        public static readonly Assembly ReadAssembly = typeof(RuntimeQueryHandler).Assembly;

        public static readonly Assembly MonitoringAssembly = typeof(CustomHistogram).Assembly;
        public static readonly Assembly ExceptionHandlingAssembly = typeof(ExceptionHandlingMiddleware).Assembly;

        public static bool DoesAssemblyReferenceAssembly(Assembly @base, Assembly target) =>
            @base.GetReferencedAssemblies().Any(a => target.GetName().Name.Equals(a.Name));

        public static Assembly[] AllBloomRuntimeAssemblies => new[]
        {
            ApiAssembly,
            ReadAssembly,
        };
    }
}