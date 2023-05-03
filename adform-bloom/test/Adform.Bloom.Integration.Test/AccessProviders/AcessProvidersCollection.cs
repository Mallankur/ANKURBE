using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.AccessProviders
{
    [CollectionDefinition(nameof(AcessProvidersCollection))]
    [Order(TestsConstants.EngineTestsOrderStartsAt)]
    public class AcessProvidersCollection : ICollectionFixture<TestsFixture>
    {
    }
}