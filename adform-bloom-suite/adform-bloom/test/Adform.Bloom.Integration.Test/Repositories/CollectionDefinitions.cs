using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Repositories
{
    [CollectionDefinition(nameof(RepositoriesCollection))]
    [Order(TestsConstants.RepositoryTestsOrderStartsAt)]
    public class RepositoriesCollection : ICollectionFixture<TestsFixture>
    {
        
    }
}