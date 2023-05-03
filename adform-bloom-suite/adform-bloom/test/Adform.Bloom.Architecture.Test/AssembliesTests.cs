using System.Linq;
using Xunit;

namespace Adform.Bloom.Architecture.Test
{
    public class AssembliesTests
    {
        public const string AdformBloom = "Adform.Bloom";

        [Fact]
        public void All_Projects_Have_AdfromBloom_Prefix()
        {
            foreach (var a in Assemblies.AllBloomAssemblies)
                Assert.StartsWith(AdformBloom, a.GetName().Name);
        }

        [Fact]
        public void Domain_Does_Not_Reference_Any_AdformBloom_Projects()
        {
            Assert.DoesNotContain(Assemblies.DomainAssembly.GetReferencedAssemblies(), a => a.Name.StartsWith(AdformBloom));
        }

        [Fact]
        public void No_Library_Except_Api_Write_Read_And_DataAccess_Should_References_Infrastructure()
        {
            foreach (var a in Assemblies.AllBloomAssemblies.Where(a => 
                a != Assemblies.ApiAssembly && 
                a != Assemblies.WriteAssembly && 
                a != Assemblies.ReadAssembly && 
                a != Assemblies.DataAccessAssembly &&
                a != Assemblies.SeederAssembly))
                Assert.False(
                    Assemblies.DoesAssemblyReferenceAssembly(a, Assemblies.InfrastructureAssembly),
                    $"{a.GetName().Name} should not reference {Assemblies.InfrastructureAssembly.GetName().Name}");
        }

        [Fact]
        public void Client_Does_Not_Reference_Any_AdformBloom_Projects()
        {
            Assert.DoesNotContain(Assemblies.ClientAssembly.GetReferencedAssemblies(), a => a.Name.StartsWith(AdformBloom));
        }

        [Fact]
        public void Event_And_Read_Do_Not_Reference_Each_Other()
        {
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.EventAssembly, Assemblies.ReadAssembly));
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.ReadAssembly, Assemblies.EventAssembly));
        }

        [Fact]
        public void Event_Does_Not_Reference_Any_AdformBloom_Projects()
        {
            Assert.DoesNotContain(Assemblies.EventAssembly.GetReferencedAssemblies(), a => a.Name.StartsWith(AdformBloom));
        }

        [Fact]
        public void Domain_Does_Not_Reference_Any_Projects()
        {
            Assert.DoesNotContain(Assemblies.DomainAssembly.GetReferencedAssemblies(), a => a.Name.StartsWith(AdformBloom) && !a.Name.EndsWith("Entities"));
        }

        [Fact]
        public void
            No_Library_Except_API_Should_References_ExceptionHandling_Instead_ExceptionHandlingAbstractions_Should_Be_Referenced()
        {
            foreach (var a in Assemblies.AllBloomAssemblies.Where(a => a != Assemblies.ApiAssembly))
                Assert.False(
                    Assemblies.DoesAssemblyReferenceAssembly(a, Assemblies.ExceptionHandlingAssembly), 
                    $"{a.GetName().Name} should not reference {Assemblies.ExceptionHandlingAssembly.GetName().Name}");
        }

        [Fact]
        public void
            No_Library_Except_API_Should_References_Monitoring_Instead_MonitoringAbstractions_Should_Be_Referenced()
        {
            foreach (var a in Assemblies.AllBloomAssemblies.Where(a => a != Assemblies.ApiAssembly))
                Assert.False(
                    Assemblies.DoesAssemblyReferenceAssembly(a, Assemblies.MonitoringAssembly),
                    $"{a.GetName().Name} should not reference {Assemblies.MonitoringAssembly.GetName().Name}");
        }


        [Fact]
        public void Only_Read_Write_And_Api_Assemblies_Reference_DataAccess()
        {
            foreach(var a in Assemblies.AllBloomAssemblies.Where(a => 
                a != Assemblies.ReadAssembly 
                && a != Assemblies.WriteAssembly 
                && a != Assemblies.ApiAssembly))
                Assert.False(
                    Assemblies.DoesAssemblyReferenceAssembly(a, Assemblies.DataAccessAssembly),
                    $"{a.GetName().Name} should not reference {Assemblies.DataAccessAssembly.GetName().Name}");
        }

        [Fact]
        public void Read_And_Write_Do_Not_Reference_Each_Other()
        {
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.WriteAssembly, Assemblies.ReadAssembly));
            Assert.False(Assemblies.DoesAssemblyReferenceAssembly(Assemblies.ReadAssembly, Assemblies.WriteAssembly));
        }
    }
}