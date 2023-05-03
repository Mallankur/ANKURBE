using Adform.Bloom.Api.Controllers;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Messages.Events;
using Adform.Bloom.Read.Interfaces;
using Adform.Bloom.Seeder;
using Adform.Bloom.Write.Commands;
using Adform.Ciam.ExceptionHandling.Middlewares;
using Adform.Ciam.Monitoring.CustomStructures;
using System.Linq;
using System.Reflection;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Runtime.Contracts.Services;

namespace Adform.Bloom.Architecture.Test
{
    public static class Assemblies
    {
        public static readonly Assembly ApiAssembly = typeof(FeaturesController).Assembly;
        public static readonly Assembly DataAccessAssembly = typeof(AdminGraphRepository).Assembly;
        public static readonly Assembly EventAssembly = typeof(SubjectAuthorizationResultChangedEvent).Assembly;
        public static readonly Assembly ReadAssembly = typeof(ISingleQuery<,>).Assembly;
        public static readonly Assembly WriteAssembly = typeof(NamedCreateCommand<>).Assembly;
        public static readonly Assembly InfrastructureAssembly = typeof(ErrorMessages).Assembly;
        public static readonly Assembly ClientAssembly = typeof(IBloomRuntimeClient).Assembly;
        public static readonly Assembly SeederAssembly = typeof(Graph).Assembly;
        public static readonly Assembly ContractAssembly = typeof(BaseNodeDto).Assembly;
        public static readonly Assembly DomainAssembly = typeof(Domain.Interfaces.IAccessValidator).Assembly;

        public static readonly Assembly MonitoringAssembly = typeof(CustomHistogram).Assembly;
        public static readonly Assembly ExceptionHandlingAssembly = typeof(ExceptionHandlingMiddleware).Assembly;

        public static Assembly[] AllBloomAssemblies => new[]
        {
            ApiAssembly,
            EventAssembly,
            ReadAssembly,
            WriteAssembly,
            InfrastructureAssembly,
            ClientAssembly,
            SeederAssembly,
            ContractAssembly,
            DomainAssembly
        };


        public static bool DoesAssemblyReferenceAssembly(Assembly @base, Assembly target)
        {
            return @base.GetReferencedAssemblies().Any(a => target.GetName().Name.Equals(a.Name));
        }
    }
}