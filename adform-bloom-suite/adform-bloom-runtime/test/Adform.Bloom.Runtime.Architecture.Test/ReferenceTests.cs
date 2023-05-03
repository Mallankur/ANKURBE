using Xunit;

namespace Adform.Bloom.Runtime.Architecture.Test
{
    public class ReferenceTests
    {
        [Fact]
        public void No_Library_Except_API_Should_References_Monitoring_Instead_MonitoringAbstractions_Should_Be_Referenced()
        {
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.ReadAssembly, Assemblies.MonitoringAssembly));
        }

        [Fact]
        public void No_Library_Except_API_Should_References_ExceptionHandling_Instead_ExceptionHandlingAbstractions_Should_Be_Referenced()
        {
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.ReadAssembly, Assemblies.ExceptionHandlingAssembly));
        }
    }
}