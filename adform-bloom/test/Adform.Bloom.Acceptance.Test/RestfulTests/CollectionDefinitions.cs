using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.RestfulTests
{
    [CollectionDefinition(nameof(RestFeaturesCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt)]
    public class RestFeaturesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestPermissionsCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 1)]
    public class RestPermissionsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestPoliciesCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 2)]
    public class RestPoliciesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestRolesCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 3)]
    public class RestRolesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestSubjectsCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 4)]
    public class RestSubjectsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestDeleteSubjectsCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 5)]
    public class RestDeleteSubjectsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestBusinessAccountsCollection))]
    [Order(TestsConstants.RestTestsOrderStartsAt + 5)]
    public class RestBusinessAccountsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(RestAssignUsersCollection))]
    [Order(TestsConstants.AssignmentTestsOrderStartsAt + 2)]
    public class RestAssignUsersCollection : ICollectionFixture<TestsFixture>
    {
    }
}