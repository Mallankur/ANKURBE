using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Cache
{
    [CollectionDefinition(nameof(CacheCollection))]
    [Order(TestsConstants.CacheStartsAt)]
    public class CacheCollection : ICollectionFixture<TestsFixture>
    {
    }
}