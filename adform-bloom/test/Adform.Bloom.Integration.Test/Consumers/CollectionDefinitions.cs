using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Consumers
{
    [CollectionDefinition(nameof(ConsumerCollection))]
    [Order(TestsConstants.ConsumerOrderStartsAt)]
    public class ConsumerCollection : ICollectionFixture<TestsFixture>
    {
        
    }

}