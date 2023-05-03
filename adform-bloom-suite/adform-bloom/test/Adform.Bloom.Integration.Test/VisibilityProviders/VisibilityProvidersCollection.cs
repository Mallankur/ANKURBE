using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.VisibilityProviders
{
    [CollectionDefinition(nameof(VisibilityProvidersCollection))]
    [Order(TestsConstants.EngineTestsOrderStartsAt)]
    public class VisibilityProvidersCollection : ICollectionFixture<TestsFixture>
    {
    }
}