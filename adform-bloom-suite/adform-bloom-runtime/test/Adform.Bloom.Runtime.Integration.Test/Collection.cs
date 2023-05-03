using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test
{
    [CollectionDefinition(nameof(Collection))]
    public class Collection : ICollectionFixture<TestsFixture>
    {
    }
}
