using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [CollectionDefinition(nameof(GraphQLFeaturesCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt)]
    public class GraphQLFeaturesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLPermissionsCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 1)]
    public class GraphQLPermissionsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLPoliciesCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 2)]
    public class GraphQLPoliciesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLRolesCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 3)]
    public class GraphQLRolesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLUsersCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 4)]
    public class GraphQLUsersCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLDeleteSubjectsCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 4)]
    public class GraphQLDeleteSubjectsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLCreateSubjectsCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 4)]
    public class GraphQLCreateSubjectsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLBusinessAccountsCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 5)]
    public class GraphQLBusinessAccountsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLLicensedFeaturesCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 6)]
    public class GraphQLLicensedFeaturesCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLPermissionBusinessAccountsCollection))]
    [Order(TestsConstants.GraphQLTestsOrderStartsAt + 7)]
    public class GraphQLPermissionBusinessAccountsCollection : ICollectionFixture<TestsFixture>
    {
    }

    [CollectionDefinition(nameof(GraphQLAssignUsersCollection))]
    [Order(TestsConstants.AssignmentTestsOrderStartsAt + 1)]
    public class GraphQLAssignUsersCollection : ICollectionFixture<TestsFixture>
    {
    }

}