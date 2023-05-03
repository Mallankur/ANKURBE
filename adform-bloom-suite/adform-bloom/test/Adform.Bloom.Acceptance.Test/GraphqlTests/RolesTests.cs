using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.ValueObjects;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Domain.Entities;
using Adform.Ciam.OngDb.Repository;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLRolesCollection))]
    public class RolesTests
    {
        #region Constants

        private const string RoleQuery = @"
query roleQuery($roleId: ID!) {
  role(id: $roleId) {
    id
    name
    type
    features(pagination: {limit: 100, offset:0}) {
        features {
            id
            name
        }
    }
	}
}";

        private const string RoleQueryWithBusinessAccountId = @"
query roleQuery($roleId: ID!, $businessAccountId: ID!) {
  role(id: $roleId) {
    id
    name
    type
    features(pagination: {limit: 100, offset:0}, businessAccountId: $businessAccountId) {
        features {
            id
            name
        }
    }
	}
}";

        private const string RolesQueryWithSizeAndPage = @"
query rolesQuery($offset: Int!, $limit: Limit!) {
  roles(pagination: {limit: $limit, offset:$offset}) {
    roles{
        id
        name
        type
        features(pagination: {limit: 100, offset:0}) {
            features {
                id
                name
            }
        }
    }
	}
}";

        private const string RolesWithSearch = @"
query rolesQuery($search: String!, $fieldName: String!, $order: SortingOrder!) {
  roles(search:$search, pagination: {limit: 10, offset:0}, sortBy:{ fieldName:$fieldName, order:$order}) {
    roles{
        id
        name
        type
    }
	}
}";       
        
        private const string RolesWithSort = @"
query rolesQuery($fieldName: String!, $order: SortingOrder!) {
  roles(pagination: {limit: 10, offset:0}, sortBy:{ fieldName:$fieldName, order:$order}) {
    roles{
        id
        name
        type
    }
	}
}";

        private const string CreateRoleMutation = @"
mutation createRoleMutation($policyId:ID!, $tenantId:ID!, $roleName:String!, $roleDescription: String!) {
  createRole(
    policyId: $policyId,
    businessAccountId: $tenantId,
    role: {
        name: $roleName,
        description: $roleDescription
        enabled: true
    })
    {
        id
    }
}";

        private const string UpdateRoleMutation = @"
mutation{{
  updateRole(roleId: ""{0}"", role: {{
    name: ""{1}"",
    description: ""{2}"",
    enabled: true
  }}) {{
    id
  }}
}}
";

        private const string UpdateRoleToFeatureAssignmentsMutation = @"
mutation{{
  updateRoleToFeatureAssignments(roleId: ""{0}"",
    assignFeatureIds: {1}
    unassignFeatureIds: {2}
    )
}}
";

        private const string CreateTemplateRoleMutation = @"
mutation{{
  createRole(policyId: ""{0}"", businessAccountId: ""{1}"", role: {{
    name: ""{2}"",
    description: ""{3}"",
    enabled: true
  }},
    templateRole: true
) {{
    id
  }}
}}
";

        private const string CreateRoleMutationSingleOperation =
            @"  {0}:createRole(policyId: ""{1}"", businessAccountId: ""{2}"", role: {{
    name: ""{3}"",
    description: ""{4}"",
    enabled: true
  }}) {{
    id
  }}";

        private const string CreateRoleWithFeaturesMutation = @"
mutation{{
  createRole(policyId: ""{0}"", businessAccountId: ""{1}"", role: {{
    name: ""{2}""
  }}, featureIds: {3}) {{
    id
  }}
}}
";

        private const string DeleteRoleMutation = @"
mutation deleteRoleMutation($roleId:ID!) {
  deleteRole(id:$roleId)
}";

        private const string BulkDeleteRoleSingleOperation = @"  {0}:deleteRole(id: ""{1}"")";


        private const string RoleQueryWithTenant = @"
{{
  role(id: ""{0}""){{
      id
      name
      businessAccount{{
        id
        name
      }}
      users(pagination: {{
        limit: 1, offset: 0
      }}) {{
        totalCount
      }}
  }}
}}
";

        private const string BulkMutation = @"
mutation{{
{0}
}}
";


        private const string RolesWithPriority = @"
{{
  roles(prioritizeTemplateRoles:{0}, pagination:{{ limit: 10, offset: 0 }}, sortBy:{{ fieldName:""id"", order: desc }}) {{
    roles {{
      id
      name
    }}
  }}
}}
";

        #endregion

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public RolesTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Get* Tests

        [Fact, Order(0)]
        public async Task Get_Role_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsRoles(), 0, size);
            var selectedNode = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = selectedNode.Id;
            var nodeType = selectedNode.Type;
            var request = new GraphQLRequest
            {
                Query = RoleQuery,
                Variables = new {roleId = nodeId }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.role.id.ToString());
            Assert.Equal(nodeType.ToString().ToLowerInvariant(), response.role.type.ToString().ToLowerInvariant());
        }

        [Fact, Order(0)]
        public async Task Get_Role_Features_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = RoleQueryWithBusinessAccountId,
                Variables = new {roleId = Graph.CustomRole20, businessAccountId = Guid.Parse(Graph.Tenant13) }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject10, request);

            // Assert
            Assert.Equal(Graph.CustomRole20, response.role.id.ToString());
            Assert.NotEmpty(response.role.features.features);
            Assert.Equal(Graph.Feature7, response.role.features.features[0].id.ToString());
        }

        [Fact, Order(0)]
        public async Task Get_Role_Inaccessible_Features_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = RoleQueryWithBusinessAccountId,
                Variables = new { roleId = Graph.CustomRole20, businessAccountId = Guid.Parse(Graph.Tenant14) }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject10, request);

            // Assert
            Assert.Equal(Graph.CustomRole20, response.role.id.ToString());
            Assert.Empty(response.role.features.features);
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Role_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = RoleQuery,
                Variables = new {roleId = Guid.NewGuid()}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Role_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = RoleQuery,
                Variables = new {roleId = Graph.CustomRole5}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject6, request, true);
            var errors = response.Errors;

            Assert.True(errors.Length == 1);
            Assert.Equal("The subject of the token does not have access to a given entity.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Roles_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsRoles
                {
                    OrderBy = "Id",
                    SortingOrder = SortingOrder.Ascending
                }, 0, size);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = RolesQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Contracts.Output.Role>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.True(nodes.Data.OrderBy(x => x.Id).Select(p => p.Type)
                .SequenceEqual(result.OrderBy(o => o.Id).Select(p => p.Type)));
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Roles_DoesNotReturnTransitional()
        {
            // Arrange
            const int size = 10;
            var cypher = _fixture.OngDB.GraphClient.Cypher.Match("(r:Role)")
                .Return(r => r.As<Role>());
            var nodes = (await cypher.ResultsAsync).ToList();
            var nodesCount = nodes.Count;
            var request = new GraphQLRequest
            {
                Query = RolesQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Contracts.Output.Role>>();

            Assert.NotEqual(nodesCount, result.Count);
            Assert.True(!result.Select(r => r.Id).Contains(Guid.Parse(Graph.TransitionalRole24)));
        }


        [Fact, Order(0)]
        public async Task Get_Role_With_BusinessAccount_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsRoles(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var tenant =
                await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Tenant>(r => r.Id == nodeId,
                    Constants.OwnsIncomingLink);
            var query = string.Format(RoleQueryWithTenant, nodeId);
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.role.id.ToString());
            Assert.Equal(tenant.First().Id.ToString(), response.role.businessAccount.id.ToString());
        }

        [Fact, Order(0)]
        public async Task Get_Role_With_UsersCount_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsRoles(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;
            var subjectsAssigned =
                await _fixture.OngDB.VisibilityRepositoriesContainer
                    .Get<QueryParamsTenantIds, Contracts.Output.Subject>()
                    .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsTenantIds
                    {
                        ContextId = nodeId
                    }, 0, 0);
            var query = string.Format(RoleQueryWithTenant, nodeId);
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.role.id.ToString());
            Assert.Equal(subjectsAssigned.TotalItems.ToString(), response.role.users.totalCount.ToString());
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Roles_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsRoles, Contracts.Output.Role>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], new QueryParamsRoles(), page,
                    size);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = RolesQueryWithSizeAndPage,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Role>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_Roles_With_Search_Test()
        {
            // Arrange
            const string search = "0";
            var request = new GraphQLRequest
            {
                Query = RolesWithSearch,
                Variables = new
                {
                    search = search,
                    fieldName = "Name",
                    order = "desc"
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Role>>();
            var expectedRoleNames = new[]
                {Graph.CustomRole10Name, Graph.CustomRole0Name, Graph.CustomRole14Name, Graph.CustomRole20Name};
            Assert.True(result.All(x => expectedRoleNames.Contains(x.Name)));
        }
        
        [Theory, Order(0)]
        [MemberData(nameof(Scenarios.Get_Roles_With_Sort_Test), MemberType = typeof(Scenarios))]
        public async Task Get_Roles_With_Sort_Test(string fieldName, string order, string[] roleNames)
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = RolesWithSort,
                Variables = new
                {
                    fieldName = fieldName,
                    order = order
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Role>>();
            Assert.True(result.All(x => roleNames.Contains(x.Name)));
        }

        [Theory, Order(0)]
        [MemberData(nameof(Scenarios.Get_Roles_With_Priority_Test), MemberType = typeof(Scenarios))]
        public async Task Get_Roles_With_Priority_Test(bool prioritizeTemplateRoles, string[] expectedRoleNames)
        {
            // Arrange
            var query = string.Format(RolesWithPriority, prioritizeTemplateRoles.ToString().ToLower());
            var request = new GraphQLRequest(query);

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["roles"]["roles"];
            var result = jsonEle.ToObject<IReadOnlyCollection<Role>>();
            Assert.True(result.Select(p => p.Name).SequenceEqual(expectedRoleNames));
        }

        #endregion

        #region Create Role Tests

        [Theory, Order(1)]
        [InlineData("Case 0", Graph.Subject0, Graph.Tenant1, true)]
        [InlineData("Case 1", Graph.Subject0, Graph.Tenant0, true)]
        [InlineData("Case 2", Graph.Subject3, Graph.Tenant2, true)]
        [InlineData("Case 3", Graph.Subject3, Graph.Tenant16, false)]
        [InlineData("Case 4", Graph.Subject3, Graph.Tenant0, false)]
        [InlineData("Case 5", Graph.Subject5, Graph.Tenant4, true)]
        [InlineData("Case 6", Graph.Subject5, Graph.Tenant15, false)]
        public async Task Create_Role_And_BusinessAccount_Relation_Test(string caseName, string subjectId,
            string tenantId,
            bool hasAccessToTenant)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);

            var roleName = Guid.NewGuid().ToString();
            var roleDescription = Guid.NewGuid().ToString();
            var request = new GraphQLRequest
            {
                Query = CreateRoleMutation,
                Variables = new
                {
                    policyId = policyId,
                    tenantId = Guid.Parse(tenantId),
                    roleName = roleName,
                    roleDescription = roleDescription
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(subjectId, request, true);

            //Assert
            if (hasAccessToTenant)
                Assert.True(Guid.TryParse(response.Data.createRole.id.ToString(), out Guid guid));
            else
            {
                var errors = _fixture.ExtractGraphqlErrorsExtensions(response);
                Assert.Contains("tenant", errors.Keys);
            }
        }

        [Theory, Order(1)]
        [InlineData(Graph.Subject0, Graph.Tenant2, true, true)]
        [InlineData(Graph.Subject0, Graph.NonExistentSubject, false, true)]
        [InlineData(Graph.Subject3, Graph.Tenant2, true, false)]
        public async Task Create_TemplateRole_And_BusinessAccount_Relation_Test(string sub, string tenantId,
            bool hasAccessToTenant, bool canCreateTemplateRole)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);

            var roleName = Guid.NewGuid().ToString();
            var roleDescription = Guid.NewGuid().ToString();
            var mutation = string.Format(CreateTemplateRoleMutation, policyId, Guid.Parse(tenantId), roleName,
                roleDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(sub, request, true);

            //Assert
            if (hasAccessToTenant && canCreateTemplateRole)
                Assert.True(Guid.TryParse(response.Data.createRole.id.ToString(), out Guid guid));
            else
            {
                var errors = _fixture.ExtractGraphqlErrorsExtensions(response);
                if (!hasAccessToTenant)
                    Assert.Contains("tenant", errors.Keys);
                if (!canCreateTemplateRole)
                    Assert.Contains("role", errors.Keys);
            }
        }

        [Theory, Order(1)]
        [InlineData(Graph.Tenant15, "tenant")]
        [InlineData(Graph.Tenant2, "role")]
        public async Task Create_TemplateRole_Returns_Error_For_Invalid_SubjectRole_Test(string tenantId, string reason)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);
            var roleName = Guid.NewGuid().ToString();
            var roleDescription = Guid.NewGuid().ToString();
            var mutation = string.Format(CreateTemplateRoleMutation, policyId, Guid.Parse(tenantId), roleName,
                roleDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject3, request, true);

            //Assert
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);
            Assert.Contains(reason, errors.Keys);
        }

        [Theory, Order(1)]
        [InlineData("")]
        [InlineData("<script>")]
        public async Task Create_Role_Returns_Error_For_Invalid_Name(string name)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);
            var tenantId = Guid.Parse(Graph.Tenant0);

            var roleName = name;
            var roleDescription = string.Empty;
            var request = new GraphQLRequest
            {
                Query = CreateRoleMutation,
                Variables = new
                {
                    policyId = policyId,
                    tenantId = tenantId,
                    roleName = roleName,
                    roleDescription = roleDescription
                }
            };

            // Act
            var response =
                (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request,
                    true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("name"));
        }

        [Theory, Order(1)]
        [InlineData("<script>")]
        public async Task Create_Role_Returns_Error_For_Invalid_Description(string description)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);
            var tenantId = Guid.Parse(Graph.Tenant0);

            var roleName = Guid.NewGuid().ToString();
            var roleDescription = description;
            var request = new GraphQLRequest
            {
                Query = CreateRoleMutation,
                Variables = new
                {
                    policyId = policyId,
                    tenantId = tenantId,
                    roleName = roleName,
                    roleDescription = roleDescription
                }
            };

            // Act
            var response =
                (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request,
                    true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("description"));
        }

        [Fact, Order(1)]
        public async Task Create_Role_Returns_Error_For_Invalid_Tenant()
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);
            var tenantId = Guid.NewGuid().ToString();

            var roleName = Guid.NewGuid().ToString();
            var roleDescription = string.Empty;
            var request = new GraphQLRequest
            {
                Query = CreateRoleMutation,
                Variables = new
                {
                    policyId = policyId,
                    tenantId = tenantId,
                    roleName = roleName,
                    roleDescription = roleDescription
                }
            };

            // Act
            var response =
                (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request,
                    true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("tenant"));
        }

        [Fact, Order(1)]
        public async Task Create_Role_Returns_Error_For_Inaccessible_BusinessAccount()
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy3);
            var tenantId = Guid.Parse(Graph.Tenant3);
            var roleName = Guid.NewGuid().ToString();
            var roleDescription = string.Empty;
            var request = new GraphQLRequest
            {
                Query = CreateRoleMutation,
                Variables = new
                {
                    policyId = policyId,
                    tenantId = tenantId,
                    roleName = roleName,
                    roleDescription = roleDescription
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject3, request, true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("tenant"));
        }

        [Theory, Order(1)]
        [InlineData(Graph.Subject3, Graph.Tenant2)]
        [InlineData(Graph.Subject5, Graph.Tenant4)]
        public async Task Create_Role_With_Features_Assigns_Role_To_Permissions(string subjectId, string tenantId)
        {
            // Arrange
            var policyId = Guid.Parse(Graph.Policy1);
            var roleName = Guid.NewGuid().ToString();
            var selectedFeatures =
                (await _fixture.OngDB.GraphRepository.GetConnectedWithIntermediateAsync<Tenant, LicensedFeature, Feature>(
                    x => x.Id == Guid.Parse(tenantId), Constants.AssignedLink, Constants.ContainsLink)).Select(f => f.Id).ToList();
            
            var arr = $"[\"{string.Join("\",\"", selectedFeatures)}\"]";

            var permissionsTobeAssigned = (await Task.WhenAll(selectedFeatures.Select(id =>
                _fixture.OngDB.GraphRepository.GetConnectedAsync<Feature, Permission>(f => f.Id == id,
                    Constants.ContainsLink)))).SelectMany(x => x.Select(y => y.Id)).Distinct();

            var mutation = string.Format(CreateRoleWithFeaturesMutation, policyId, tenantId, roleName, arr);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(subjectId, request);

            // Assert
            Assert.True(Guid.TryParse(response.createRole.id.ToString(), out Guid id));
            var connectedPermissions =
                await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(x => x.Id == id,
                    Constants.ContainsLink);
            Assert.All(permissionsTobeAssigned, x => connectedPermissions.Select(x => x.Id).Contains(x));
        }

        #endregion


        #region Update Role Tests

        [Theory, Order(2)]
        [InlineData(Graph.Subject0, Graph.Tenant2, Graph.CustomRole20, Graph.CustomRole20Name, true, true)]
        [InlineData(Graph.Subject3, Graph.Tenant2, Graph.CustomRole6, Graph.CustomRole6Name, true, true)]
        [InlineData(Graph.Subject3, Graph.Tenant2, Graph.CustomRole20, Graph.CustomRole20Name, false, true)]
        [InlineData(Graph.Subject3, Graph.Tenant0, Graph.NonExistentRole, Graph.CustomRole6Name, true, false)]
        public async Task Update_Role(string subjectId, string tenantId, string roleId, string roleName,
            bool hasAccessToRole,
            bool roleExist)
        {
            // Arrange
            var name = roleName;
            var roleDescription = Guid.NewGuid().ToString();

            var node = await _fixture.OngDB.GraphRepository.GetNodeAsync<Role>(o => o.Id == Guid.Parse(roleId));
            var mutation = string.Format(UpdateRoleMutation, Guid.Parse(roleId), name,
                roleDescription);
            await _fixture.OngDB.GraphRepository.UpdateNodeAsync<Role>(o => o.Id == Guid.Parse(roleId), node);
            var request = new GraphQLRequest(mutation);
            // Act
            var response = (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(subjectId, request, true);

            //Assert
            if (roleExist && hasAccessToRole)
                Assert.True(Guid.TryParse(response.Data.updateRole.id.ToString(), out Guid guid));
            else
            {
                var errors = _fixture.ExtractGraphqlErrorsExtensions(response);
                    if(!roleExist)
                        Assert.True(errors.ContainsKey("code"));
                    else
                        Assert.True(errors.ContainsKey("role"));
            }
        }


        [Theory, Order(2)]
        [InlineData("")]
        [InlineData("<script>")]
        public async Task Update_Role_Returns_Error_For_Invalid_Name(string name)
        {
            // Arrange
            var roleId = Guid.Parse(Graph.Role1);

            var roleName = name;
            var roleDescription = string.Empty;
            var mutation = string.Format(UpdateRoleMutation, roleId, roleName, roleDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response =
                (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request,
                    true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("name"));
        }

        [Theory, Order(2)]
        [InlineData("<script>")]
        public async Task Update_Role_Returns_Error_For_Invalid_Description(string description)
        {
            // Arrange
            var roleId = Guid.Parse(Graph.Role1);

            var roleName = Guid.NewGuid().ToString();
            var roleDescription = description;
            var mutation = string.Format(UpdateRoleMutation, roleId, roleName, roleDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response =
                (GraphQLResponse<dynamic>) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request,
                    true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("description"));
        }

        [Fact, Order(2)]
        public async Task Update_Role_Returns_Error_For_Inaccessible_Role()
        {
            // Arrange
            var roleId = Guid.Parse(Graph.CustomRole10);
            var roleName = Graph.CustomRole10Name;
            var roleDescription = Guid.NewGuid().ToString();
            var mutation = string.Format(UpdateRoleMutation, roleId, roleName, roleDescription);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject3, request, true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("role"));
        }

        [Fact, Order(3)]
        public async Task UpdateRoleAssignmentsCommand_Succeed()
        {
            var roleId = Guid.Parse(Graph.CustomRole16);
            var featureId = Guid.Parse(Graph.Feature5);
            var featureIds = new List<Guid>
            {
                featureId
            };

            var startingState = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                x => x.Id == roleId,
                Constants.ContainsLink);
            var permissions = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Feature, Permission>(
                x => x.Id == featureId,
                Constants.ContainsLink);
            await UpdateRoleAssignmentsCommand_AssignOrUnassign(roleId, featureIds, true);
            var assignState = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                x => x.Id == roleId,
                Constants.ContainsLink);
            Assert.True(permissions.Select(o => o.Name).OrderBy(o => o)
                .SequenceEqual(assignState.Select(p => p.Name).Except(startingState.Select(p => p.Name)).OrderBy(o => o)));
            Assert.Equal(permissions.Count(), assignState.Count() - startingState.Count());
            await UpdateRoleAssignmentsCommand_AssignOrUnassign(roleId, featureIds, false);
            var endState = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(x => x.Id == roleId,
                Constants.ContainsLink);
            Assert.True(endState.Select(o => o.Name).OrderBy(o => o)
                .SequenceEqual(startingState.Select(p => p.Name).OrderBy(o => o)));
            Assert.Equal(startingState.Count(), endState.Count());
        }

        private async Task UpdateRoleAssignmentsCommand_AssignOrUnassign(Guid roleId, List<Guid> featureIds,
            bool assign)
        {
            // Arrange
            var mutation = string.Format(UpdateRoleToFeatureAssignmentsMutation, roleId,
                $"[\"{string.Join(",", featureIds.Select(t => t.ToString()))}\"]", "[]");
            if (!assign)
                mutation = string.Format(UpdateRoleToFeatureAssignmentsMutation, roleId, "[]",
                    $"[\"{string.Join(",", featureIds.Select(t => t.ToString()))}\"]");
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);
            var jsonEle = response["updateRoleToFeatureAssignments"];
            // Assert
            Assert.Equal(roleId.ToString(), jsonEle.ToString());
        }

        [Fact, Order(3)]
        public async Task UpdateRoleAssignmentsCommand_Returns_Error_For_Inaccessible_Role()
        {
            // Arrange
            var roleId = Guid.Parse(Graph.TransitionalRole21);
            var featureId = Guid.Parse(Graph.Feature2);
            var featureIds = new List<Guid>
            {
                featureId
            };
            var mutation = string.Format(UpdateRoleToFeatureAssignmentsMutation, roleId,
                $"[\"{string.Join(",", featureIds.Select(t => t.ToString()))}\"]", "[]");
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject3, request, true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("role"));
        }

        #endregion

        #region Other Tests

        [Theory, Order(4)]
        [InlineData(3)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task Bulk_Create_And_Delete_Roles(int rolesToCreateAndDelete)
        {
            // Prepare bulk create mutation
            var createOperations = new StringBuilder();
            for (var i = 0; i < rolesToCreateAndDelete; ++i)
            {
                var policyId = Guid.Parse(Graph.Policy1);

                var roleName = $"Bulk role {i}";
                var roleDescription = $"Bulk role {i}";
                var createMutation = string.Format(CreateRoleMutationSingleOperation, $"operation{i}", policyId,
                    Guid.Parse(Graph.Tenant1), roleName, roleDescription);
                createOperations.Append(createMutation);

                if (i != rolesToCreateAndDelete - 1)
                    createOperations.AppendLine(",");
            }

            // Send request & assert
            var bulkCreateResponse = await _fixture.SendGraphqlRequestAsync(Graph.Subject0,
                new GraphQLRequest
                    (string.Format(BulkMutation, createOperations)));

            var createdRolesIds = Enumerable.Range(0, rolesToCreateAndDelete)
                .Select(i => bulkCreateResponse[$"operation{i}"]["id"])
                .Select(r => r.Value)
                .Cast<string>()
                .ToList();
            Assert.Equal(rolesToCreateAndDelete, createdRolesIds.Count);

            var noOfCreatedRoles =
                await _fixture.OngDB.GraphRepository.GetCountAsync<Role>(r => r.Name.Contains("Bulk role"));
            Assert.Equal(rolesToCreateAndDelete, noOfCreatedRoles);

            // Prepare bulk delete mutation
            var deleteOperations = new StringBuilder();
            for (var i = 0; i < rolesToCreateAndDelete; ++i)
            {
                var deleteMutation = string.Format(BulkDeleteRoleSingleOperation, $"operation{i}", createdRolesIds[i]);
                deleteOperations.Append(deleteMutation);

                if (i != rolesToCreateAndDelete - 1)
                    deleteOperations.AppendLine(",");
            }

            //Send request & assert
            var bulkDeleteResponse = await _fixture.SendGraphqlRequestAsync(Graph.Subject0,
                new GraphQLRequest
                    (string.Format(BulkMutation, deleteOperations)));


            var deleteResults = Enumerable.Range(0, rolesToCreateAndDelete)
                .Select(i => bulkDeleteResponse[$"operation{i}"])
                .Select((r, i) => (r.Value, Index: i))
                .ToList();

            Assert.Equal(rolesToCreateAndDelete, deleteResults.Count);

            Assert.All(deleteResults, r => Assert.Equal(createdRolesIds[r.Index], r.Value));

            var noOfRoles = await _fixture.OngDB.GraphRepository.GetCountAsync<Role>(r => r.Name.Contains("Bulk role"));
            Assert.Equal(0, noOfRoles);
        }


        [Theory, Order(int.MaxValue)]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_Subject1), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_Subject3), MemberType = typeof(Scenarios))]
        [MemberData(nameof(Scenarios.Delete_Role_Results_For_AdformAdmin), MemberType = typeof(Scenarios))]
        public async Task Delete_Role_Test(string subjectId, string roleId, HttpStatusCode expectedCode)
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = DeleteRoleMutation,
                Variables = new
                {
                    roleId = roleId
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(subjectId, request);

            //Assert
            if (expectedCode == HttpStatusCode.NoContent)
            {
                var jsonEle = response["deleteRole"];
                Assert.Equal(roleId, jsonEle.ToString());
            }
            else
            {
                var extensions = ((GraphQLResponse<dynamic>) response).Errors.First().Extensions["code"];
                Assert.NotNull(extensions);
                Assert.True(extensions.Equals(ErrorReasons.AccessControlValidationFailedReason) ||
                            extensions.Equals("AUTH_NOT_AUTHORIZED"));
            }

            var deletedRole = await _fixture.OngDB.GraphRepository.GetNodeAsync<Role>(p => p.Id == Guid.Parse(roleId));
            if (expectedCode == HttpStatusCode.Forbidden)
                Assert.NotNull(deletedRole);
            else
                Assert.Null(deletedRole);
        }

        #endregion
    }
}