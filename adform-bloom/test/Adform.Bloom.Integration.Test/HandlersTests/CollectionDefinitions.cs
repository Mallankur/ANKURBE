using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.HandlersTests
{
    [CollectionDefinition(nameof(HandlersCollection))]
    [Order(TestsConstants.HandlerTestsOrderStartsAt)]
    public class HandlersCollection : ICollectionFixture<TestsFixture>
    {
    }
}