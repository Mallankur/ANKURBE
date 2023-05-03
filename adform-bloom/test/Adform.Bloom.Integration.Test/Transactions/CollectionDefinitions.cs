using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.Transactions
{
    [CollectionDefinition(nameof(DatabaseCollection))]
    [Order(TestsConstants.DatabaseOrderStartsAt)]
    public class DatabaseCollection : ICollectionFixture<TestsFixture>
    {
        
    }

    [CollectionDefinition(nameof(TransactionCollection))]
    [Order(TestsConstants.TransactionOrderStartsAt)]
    public class TransactionCollection : ICollectionFixture<TestsFixture>
    {

    }
}