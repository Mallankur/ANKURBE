using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Infrastructure.Models;
using Dapper;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;
using Role = Adform.Bloom.Contracts.Output.Role;
using Subject = Adform.Bloom.Domain.Entities.Subject;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLUsersCollection))]
    public class UsersTests
    {
        private const string UserQuery = @"
query userQuery($subjectId: ID!) {
    user(id: $subjectId) {
        id
    }
}";

        private const string UserQueryWithPaginatedBusinessAccounts = @"
query userWithPaginatedBusinessAccountsQuery($subjectId: ID!, $offset:Int!, $limit:Limit!) {
    user(id: $subjectId){
        id
        businessAccounts(pagination:{ limit: $limit, offset: $offset }, sortBy:{fieldName:""Name""})
        {
            totalCount
            offset
            limit
            businessAccounts 
            {
                id
                name
            }
        }
    }
}";

        private const string UserQueryWithPaginatedRoles = @"
query userWithPaginatedRolesQuery($subjectId: ID!, $offset:Int!, $limit:Limit!) {
    user(id: $subjectId){
        id
        roles(pagination:{ limit: $limit, offset: $offset }, sortBy:{fieldName:""Name""})
        {
            totalCount
            offset
            limit
            roles 
            {
                id
                name
                type
            }
        }
    }
}";

        private const string UserQueryWithPaginatedRolesAndTenantFiltered = @"
query userWithPaginatedRolesAndTenantFilteredQuery($subjectId: ID!, $offset:Int!, $limit:Limit!, $tenantIds: [ID!]) {
    user(id: $subjectId){
        id
        roles(pagination:{ limit: $limit, offset: $offset }, businessAccountIds:$tenantIds)
        {
            totalCount
            offset
            limit
            roles 
            {
                id
                name
            }
        }
    }
}";

        private const string UsersQueryWithSizeAndPage = @"
query usersQuery($limit: Limit!, $offset:Int!) {
    users(pagination:{ limit: $limit, offset: $offset }) {
        users {
            id
        }
    }
}";

        private const string UsersQueryWithSizeAndFilter = @"
query usersQuery($limit: Limit!, $offset:Int!, $subjectIds: [ID!]) {
    users(userIds:$subjectIds, pagination:{ limit:$limit, offset:$offset }) {
        users {
            id
        }
    }
}";

        private const string UsersQueryWithSearch = @"
query usersQuery($limit: Limit!, $offset:Int!, $search: String!) {
    users(search:$search, pagination:{ limit:$limit, offset:$offset }) {
        users {
            id
        }
    }
}";

        //TODO 
        private const string UsersQueryWithSort = @"
query usersQuery($limit: Limit!, $offset:Int!, $sortBy: SortInput!) {
    users(sortBy:$sortBy, pagination:{ limit:$limit, offset:$offset }) {
        users {
            id
            name
            twoFaEnabled
        }
    }
}";


        private const string UserDetailQuery = @"
query userQuery($subjectId: ID!) {
    user(id: $subjectId) {
        id
        name
        email
        username
        phone
        timezone
        locale
        firstName
        lastName
        company
        twoFaEnabled
        securityNotificationsEnabled
        status
    }
}";

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;
        private readonly ITestOutputHelper _output;

        public UsersTests(TestsFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact, Order(0)]
        public async Task Get_User_Detail_Test()
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, 10);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var request = new GraphQLRequest
            {
                Query = UserDetailQuery,
                Variables = new
                {
                    subjectId = nodeId
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.user.id.ToString());

            Assert.NotNull(response.user.name);
            Assert.NotNull(response.user.email);
            Assert.NotNull(response.user.username);
            Assert.NotNull(response.user.timezone);
            Assert.NotNull(response.user.locale);
            Assert.NotNull(response.user.firstName);
            Assert.NotNull(response.user.lastName);
            Assert.NotNull(response.user.company);
            Assert.NotNull(response.user.twoFaEnabled);
            Assert.NotNull(response.user.securityNotificationsEnabled);
            Assert.NotNull(response.user.status);
        }

        [Fact, Order(0)]
        public async Task Get_User_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var request = new GraphQLRequest
            {
                Query = UserQuery,
                Variables = new {subjectId = nodeId}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.user.id.ToString());
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_User_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQuery,
                Variables = new {subjectId = Guid.NewGuid()}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Can_Read_Other_Users_Assigned_To_The_Same_BusinessAccount()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQuery,
                Variables = new {subjectId = Graph.Subject2}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject3, request);

            // Assert
            Assert.Equal(Graph.Subject2, response.user.id.ToString());
        }


        [Fact, Order(0)]
        public async Task Get_Forbidden_User_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQuery,
                Variables = new {subjectId = Graph.Subject3}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject6, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("The subject of the token does not have access to a given entity.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_User_With_BusinessAccounts_Should_Return_BusinessAccounts_Accessible_ByActor()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 100
                }
            };
            var expect = new[] {Graph.Tenant2, Graph.Tenant9};

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var result = GetBusinessAccountsFromResponse(response);
            Assert.Equal(expect.Length, result.Count);
            Assert.True(expect.SequenceEqual(result.Select(o => o.Id.ToString())));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_BusinessAccounts_Should_Return_All_Users_BusinessAccounts_When_Actor_Is_Admin()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 0,
                    limit = 100
                }
            };

            // Act
            var responseAsAdmin =
                (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var responseAsSameUser = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var resultAsAdmin = GetBusinessAccountsFromResponse(responseAsAdmin);
            var resultAsSameUser = GetBusinessAccountsFromResponse(responseAsSameUser);

            Assert.True(resultAsAdmin.Select(x => x.Id).SequenceEqual(resultAsSameUser.Select(x => x.Id)));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_BusinessAccounts_With_Limit0_Should_Return_OutOfRange()
        {
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 0
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject1, request, true);
            var errors = response.Errors;
            // Assert
            Assert.True(errors.Length == 1);
            Assert.True(errors[0].Message.ToString().Contains("Value must be between"));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_BusinessAccounts_Check_BusinessAccounts_Pagination()
        {
            var allBusinessAccountsQuery = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 0,
                    limit = 100
                }
            };
            var businessAccountsQueryPart1 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 0,
                    limit = 5
                }
            };
            var businessAccountsQueryPart2 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 5,
                    limit = 5
                }
            };
            var businessAccountsQueryPart3 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedBusinessAccounts,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 10,
                    limit = 7
                }
            };
            var requests = new[]
            {
                allBusinessAccountsQuery,
                businessAccountsQueryPart1, businessAccountsQueryPart2, businessAccountsQueryPart3
            };

            // Act
            var responsePromise = requests.Select(async r =>
                (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, r)));
            var responses = await Task.WhenAll(responsePromise);

            // Assert
            var translatedResponses = responses.Select(GetBusinessAccountsFromResponse).ToList();

            Assert.Equal(17, translatedResponses[0].Count);
            Assert.Equal(5, translatedResponses[1].Count);
            Assert.Equal(5, translatedResponses[2].Count);
            Assert.Equal(7, translatedResponses[3].Count);
            var fullResultAtOnce = translatedResponses[0].OrderBy(o => o.Id).ToList();
            var fullResultCombinedFromSmallerParts = translatedResponses[1]
                .Concat(translatedResponses[2]).Concat(translatedResponses[3]).OrderBy(o => o.Id).ToList();
            Assert.True(fullResultAtOnce.Select(x => x.Id)
                .SequenceEqual(fullResultCombinedFromSmallerParts.Select(x => x.Id)));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_Roles_Should_Return_Assigned_Roles_Accessible_ByActor()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject9,
                    offset = 0,
                    limit = 100
                }
            };
            var expected = new[] {Graph.CustomRole18};
            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject8, request));

            // Assert
            var result = GetRolesFromResponse(response);

            Assert.Equal(expected.Length, result.Count);
            Assert.True(result.Select(r => r.Id.ToString()).OrderBy(x => x)
                .SequenceEqual(expected.OrderBy(x => x)));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_Roles_Should_Return_Transitional_Roles_As_Well()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject8,
                    offset = 0,
                    limit = 100
                }
            };
            var expected = new[] {Graph.TransitionalRole24};
            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject8, request));

            // Assert
            var result = GetRolesFromResponse(response);
            Assert.Contains(RoleCategory.Transitional, result.Select(x => x.Type));
            Assert.Equal(expected.Length, result.Count);
            Assert.True(result.Select(r => r.Id.ToString()).OrderBy(x => x)
                .SequenceEqual(expected.OrderBy(x => x)));
        }

        [Fact, Order(0)]
        public async Task
            Get_User_With_Roles_FilteredTenant_Should_Return_Assigned_Roles_Accessible_ByActor_Intersected_By_Tenant()
        {
            // Arrange
            var tenantIds = new List<Guid>() {Guid.Parse(Graph.Tenant2)};
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRolesAndTenantFiltered,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 100,
                    tenantIds = tenantIds
                }
            };
            var expected = new[] { Graph.CustomRole6 };
            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var result = GetRolesFromResponse(response);

            Assert.Equal(expected.Length, result.Count);
            Assert.True(result.Select(r => r.Id.ToString()).OrderBy(x => x)
                .SequenceEqual(expected.OrderBy(x => x)));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_Roles_Should_Return_All_Roles_Assigned_To_User_When_Actor_Is_Admin()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject0,
                    offset = 0,
                    limit = 100
                }
            };

            // Act
            var responseAsAdmin =
                (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var responseAsSameUser = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var resultAsAdmin = GetRolesFromResponse(responseAsAdmin);
            var resultAsSameUser = GetRolesFromResponse(responseAsSameUser);

            Assert.True(resultAsAdmin.Select(x => x.Id).SequenceEqual(resultAsSameUser.Select(x => x.Id)));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_Roles_With_Limit0_Should_Return_OutOfRange()
        {
            var request = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 0
                }
            };

            // Act
            var response = (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true));
            var errors = response.Errors;

            // Assert

            Assert.True(errors.Length == 1);
            Assert.True(errors[0].Message.ToString().Contains("Value must be between"));
        }

        [Fact, Order(0)]
        public async Task Get_User_With_Roles_Check_Roles_Pagination()
        {
            var allBusinessAccountsQuery = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 2
                }
            };
            var rolesQueryPart1 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 0,
                    limit = 1
                }
            };
            var rolesQueryPart2 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 1,
                    limit = 1
                }
            };
            var rolesQueryPart3 = new GraphQLRequest
            {
                Query = UserQueryWithPaginatedRoles,
                Variables = new
                {
                    subjectId = Graph.Subject3,
                    offset = 2,
                    limit = 1
                }
            };
            var requests = new[]
            {
                allBusinessAccountsQuery,
                rolesQueryPart1, rolesQueryPart2, rolesQueryPart3
            };

            // Act
            var responsePromise = requests.Select(async r =>
                (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, r)));
            var responses = await Task.WhenAll(responsePromise);

            // Assert
            var translatedResponses = responses.Select(GetRolesFromResponse).ToList();

            Assert.Equal(2, translatedResponses[0].Count());
            Assert.Equal(1, translatedResponses[1].Count());
            Assert.Equal(1, translatedResponses[2].Count());
            Assert.Empty(translatedResponses[3]);
            var fullResultAtOnce = translatedResponses[0].OrderBy(o => o.Id).ToList();
            var fullResultCombinedFromSmallerParts = translatedResponses[1]
                .Concat(translatedResponses[2])
                .Concat(translatedResponses[3]).OrderBy(o => o.Id).ToList();
            Assert.True(fullResultAtOnce.Select(x => x.Id)
                .SequenceEqual(fullResultCombinedFromSmallerParts.Select(x => x.Id)));
        }


        [Fact, Order(0)]
        public async Task Get_Users_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var nodesCount = nodes.Data.Count();
            var request = new GraphQLRequest
            {
                Query = UsersQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["users"]["users"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Subject>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }


        [Fact, Order(0)]
        public async Task Get_Users_FilterByIds_Test()
        {
            // Arrange
            const int size = 10;
            var ids = new[] {Graph.Subject0, Graph.Subject3};
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var nodesCount = nodes.Data.Count();
            var request = new GraphQLRequest
            {
                Query = UsersQueryWithSizeAndFilter,
                Variables = new
                {
                    subjectIds = ids,
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["users"]["users"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Subject>>();

            Assert.Equal(ids.Length, result.Count);
            Assert.True(result.OrderBy(p => p.Id).Select(p => p.Id.ToString()).SequenceEqual(ids.OrderBy(p => p)));
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Users_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), page, size);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = UsersQueryWithSizeAndPage,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["users"]["users"];
            var result = jsonEle.ToObject<IReadOnlyCollection<User>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Users_With_Search_Test()
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, 10);
            var users = await _fixture.SQL.PsqlConnection.QueryAsync<User>(
                "select * from users where email ~* @Search OR username ~* @Search", new
                {
                    Search = "aaa"
                });
            var expectedIds = nodes.Data.Select(x => x.Id).Intersect(users.Select(x => x.Id));
            var request = new GraphQLRequest
            {
                Query = UsersQueryWithSearch,
                Variables = new
                {
                    search = "aaa",
                    offset = 0,
                    limit = 10
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["users"]["users"];
            var result = jsonEle.ToObject<IReadOnlyCollection<User>>();
            _output.WriteLine(string.Join(", ", expectedIds.ToList()));
            _output.WriteLine(string.Join(", ", result.Select(x => x.Id).ToList()));
            Assert.True(expectedIds.OrderBy(o => o).SequenceEqual(result.OrderBy(o => o.Id).Select(x => x.Id)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_Users_With_Sort_Test(bool ascending)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, 20);
            var users = await _fixture.SQL.PsqlConnection.QueryAsync<User>(
                "select U.id ,U.username, U.email, U.name, U.two_factor_required as TwoFaEnabled from users AS U");
            var ids = new HashSet<Guid>(nodes.Data.Select(x => x.Id));
            var intersectedUsers = users.Where(p => ids.Contains(p.Id));

            var expectedUsers = ascending
                ? intersectedUsers.OrderBy(p => p.TwoFaEnabled).ThenBy(n => n.Id).ToList()
                : intersectedUsers.OrderByDescending(p => p.TwoFaEnabled).ThenByDescending(n => n.Id).ToList();

            var request = new GraphQLRequest
            {
                Query = UsersQueryWithSort,
                Variables = new
                {
                    sortBy = new
                    {
                        fieldName = "twoFaEnabled",
                        order = ascending ? "asc" : "desc"
                    },
                    offset = 0,
                    limit = 20
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["users"]["users"];
            var result = jsonEle.ToObject<IReadOnlyCollection<User>>();
            Assert.True(expectedUsers.Select(o => o.Id).SequenceEqual(result.Select(x => x.Id)));
        }

        private static IReadOnlyList<Role> GetRolesFromResponse(JObject response)
        {
            var jsonEle = response["user"]["roles"]["roles"];
            return jsonEle.ToObject<IReadOnlyList<Role>>();
        }

        private static IReadOnlyList<BusinessAccount> GetBusinessAccountsFromResponse(JObject response)
        {
            var jsonEle = response["user"]["businessAccounts"]["businessAccounts"];
            return jsonEle.ToObject<IReadOnlyList<BusinessAccount>>();
        }
    }
}