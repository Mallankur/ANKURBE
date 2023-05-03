using System;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Infrastructure.Cache;
using Bogus;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Infrastructure
{
    public class KeyGeneratorTests
    {
        private readonly Faker<SubjectQueryBase> _faker;

        public KeyGeneratorTests()
        {
            _faker = new Faker<SubjectQueryBase>()
                .RuleFor(p => p.PolicyNames, f => new[] {f.Random.Word()})
                .RuleFor(p => p.TenantType, f => f.Random.Word())
                .RuleFor(p => p.TenantIds, f => new[] {f.Random.Guid()})
                .RuleFor(p => p.TenantLegacyIds, f => new[] {f.Random.Int()})
                .RuleFor(p => p.SubjectId, f => f.Random.Guid())
                .RuleFor(p => p.InheritanceEnabled, f => f.Random.Bool());
        }

        [Fact]
        public async Task GenerateKey_Returns_Same_Key_When_Object_Has_Same_Properties()
        {
            // Arrange
            var request = _faker.Generate();
            var id = Guid.NewGuid();
            var generator = new KeyGenerator();

            // Act
            var key = generator.GenerateKey(request);
            var key2 = generator.GenerateKey(request);
            // Assert
            
            Assert.Equal(key, key2);
        }
    }
}