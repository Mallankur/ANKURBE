using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Domain.Entities;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLPermissionsCollection))]
    public class PermissionsTests
    {
        private const string PermissionQuery = @"
query permissionQuery($permissionId: ID!) {
    permission(id: $permissionId) {
        id
        name
    }
}";

        private const string PermissionsQueryWithSizeAndPage = @"
query permissionsQuery($limit: Limit!, $offset:Int!) {
    permissions(pagination:{ limit: $limit, offset: $offset }) {
        permissions {
            id
            name
        }
    }
}";

        private const string CreatePermissionMutation = @"
mutation{{
  createPermission(permission: {{
    name: ""{0}"",
    description: ""{1}"",
    enabled: true
  }}) {{
    id
  }}
}}
";

        private const string AssignToRoleMutation = @"
mutation{{
  updatePermissionToRoleAssignments(assignment: {{
    permissionId: ""{0}"",
    roleId: ""{1}"",
    operation: {2}
  }})
}}
";

        private const string DeletePermissionMutation = @"
mutation deletePermissionMutation($permissionId:ID!) {
  deletePermission(id: $permissionId)
}";

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public PermissionsTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_Permission_Test()
        {
            // Arrange
            const int size = 10;
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsTenantIds(), 0, size);
            var permissionId = permissions.Data.ToArray()[Random.Next(0, permissions.Data.ToArray().Length - 1)].Id;
            var request = new GraphQLRequest
            {
                Query = PermissionQuery,
                Variables = new { permissionId = permissionId }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(permissionId.ToString(), response.permission.id.ToString());
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Permission_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = PermissionQuery,
                Variables = new { permissionId = Guid.NewGuid() }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Permission_Test()
        {
            // Arrange
            const int size = 20;
            var permissions = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject10], new QueryParamsTenantIds(), 0,
                    size)).Data.ToArray();
            var permissionId = permissions[Random.Next(0, permissions.Length - 1)].Id;
            var request = new GraphQLRequest
            {
                Query = PermissionQuery,
                Variables = new { permissionId = permissionId }
            };
            
            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject4, request, true);
            var errors = response.Errors;

            Assert.True(errors.Length == 1);
            Assert.Equal("The subject of the token does not have access to a given entity.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Permissions_Test()
        {
            // Arrange
            const int size = 10;
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsTenantIds(), 0, size);
            var permissionsCount = permissions.Data.ToArray().Length;
            var request = new GraphQLRequest
            {
                Query = PermissionsQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["permissions"]["permissions"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Permission>>();

            Assert.Equal(permissionsCount, result.Count);
            Assert.All(result, r => permissions.Data.Any(p => p.Id == r.Id));
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Permissions_With_Paging_Test(int size, int page)
        {
            // Arrange
            var permissions = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Permission>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsTenantIds(), page, size);
            var permissionsCount = permissions.Data.Count;
            var request = new GraphQLRequest
            {
                Query = PermissionsQueryWithSizeAndPage,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["permissions"]["permissions"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Permission>>();

            Assert.Equal(permissionsCount, result.Count);
            Assert.All(result, r => permissions.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(1)]
        public async Task Create_Permission_Test()
        {
            // Arrange
            var permissionName = Guid.NewGuid().ToString();
            var permissionDescription = Guid.NewGuid().ToString();
            var mutation = string.Format(CreatePermissionMutation, permissionName, permissionDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            //Assert
            Guid guid;
            Assert.True(Guid.TryParse(response.createPermission.id.ToString(), out guid));
        }

        [Theory, Order(1)]
        [MemberData(nameof(Scenarios.Assign_Permission_To_Role_Results_For_Subject3), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Assign_Permission_To_Role_Results_For_AdformAdmin), MemberType = typeof(Scenarios))]
        public async Task AssignAndUnassign_Permission_To_Role_Test(string subjectId, string roleId,
            string permissionId, bool success)
        {
            var permission =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Permission>(r => r.Id == Guid.Parse(permissionId));

            Assert.NotNull(permission);

            await AssignTest(subjectId, roleId, success, permission);

            if (success)
                await UnassignTest(subjectId, roleId, permission);
        }

        private async Task AssignTest(string subjectId, string roleId, bool success, Permission permission)
        {
            // Arrange
            var mutationAssign = string.Format(AssignToRoleMutation, permission.Id, Guid.Parse(roleId),
                LinkOperation.Assign.ToString().ToLowerInvariant());
            var requestAssign = new GraphQLRequest(mutationAssign);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(subjectId, requestAssign, !success);

            //Assert

            if (success)
            {
                var jsonEle = ((JObject) response)["updatePermissionToRoleAssignments"];
                Assert.Equal(roleId, jsonEle.ToString());
            }
            else
            {
                var errors = ((GraphQLResponse<dynamic>) response).Errors;
                var extensions = errors.First().Extensions["code"];
                Assert.NotNull(extensions);
                Assert.Equal("AUTH_NOT_AUTHORIZED", extensions);
            }

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                p => p.Id == Guid.Parse(roleId),
                rp => rp.Id == permission.Id, Constants.ContainsLink);

            if (success)
                Assert.True(hasLink);
            else
                Assert.False(hasLink);
        }

        private async Task UnassignTest(string subjectId, string roleId, Permission permission)
        {
            // Arrange
            var mutationUnassign = string.Format(AssignToRoleMutation, permission.Id, Guid.Parse(roleId),
                LinkOperation.Unassign.ToString().ToLowerInvariant());
            var requestUnassign = new GraphQLRequest(mutationUnassign);

            // Act
            var responseUnassign = (JObject) (await _fixture.SendGraphqlRequestAsync(subjectId, requestUnassign));

            //Assert

            var jsonEle = responseUnassign["updatePermissionToRoleAssignments"];
            Assert.Equal(roleId, jsonEle.ToString());
            var hasNoLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                p => p.Id == Guid.Parse(roleId),
                rp => rp.Id == permission.Id, Constants.ContainsLink);
            Assert.False(hasNoLink);
        }

        [Fact, Order(1)]
        public async Task Assign_Permission_To_Role_Without_Feature_Forbidden()
        {
            // Arrange
            var roleId = Graph.CustomRole0;
            var permission = await _fixture.OngDB.GraphRepository.CreateNodeAsync(new Permission
            {
                Name = "NoFeature_Permission"
            });
            
            var mutationAssign = string.Format(AssignToRoleMutation, permission.Id, roleId,
                LinkOperation.Assign.ToString().ToLowerInvariant());
            var requestAssign = new GraphQLRequest(mutationAssign);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject1, requestAssign, true);
            var errors = ((GraphQLResponse<dynamic>) response).Errors;

            //Assert
            var extensions = errors.First().Extensions["code"];
            Assert.NotNull(extensions);
            Assert.Equal("AUTH_NOT_AUTHORIZED", extensions);
        }

        [Theory, Order(int.MaxValue)]
        [MemberData(nameof(Graph.AllPermissionsData), MemberType = typeof(Graph))]
        public async Task Delete_Permission_Test(string permissionId)
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = DeletePermissionMutation,
                Variables = new { 
                    permissionId = Guid.Parse(permissionId)
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            //Assert
            var jsonEle = response["deletePermission"];
            Assert.Equal(permissionId, jsonEle.ToString());

            var deletedPermission =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Permission>(p => p.Id == Guid.Parse(permissionId));
            Assert.Null(deletedPermission);
        }
    }
}