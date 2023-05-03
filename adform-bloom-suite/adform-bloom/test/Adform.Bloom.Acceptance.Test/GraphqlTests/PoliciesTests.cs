using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLPoliciesCollection))]
    public class PoliciesTests
    {
        private const string PolicyQuery = @"
query policyQuery($policyId: ID!) {
    policy(id: $policyId) {
        id
        name
        roles
        {
            id
            name
        }
    }
}";

        private const string PoliciesQueryWithSizeAndPage = @"
query policiesQuery($limit: Limit!, $offset:Int!) {
    policies(pagination:{ limit: $limit, offset: $offset }) {
        policies {
            id
            name
            roles
            {
                id
                name
            }
        }
    }
}";

        private const string CreatePolicyMutation = @"
mutation{{
  createPolicy(parentId: ""{0}"", policy: {{
    name: ""{1}"",
    description: ""{2}"",
    enabled: true
  }}) {{
    id
  }}
}}
";

        private const string DeletePolicyMutation = @"
mutation{{
  deletePolicy(id: ""{0}"")
}}
";
        private class PolicyWithRoles : Policy
        {
            public IReadOnlyCollection<Role> Roles { get; set; }
        }
        
        private readonly TestsFixture _fixture;

        public PoliciesTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_Policy_Test()
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1);
            var policyId = policies.Data.First().Id;
            var roles = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Policy, Role>(policy => policy.Id == policyId,
                Constants.ContainsLink)).ToList();
            var request = new GraphQLRequest
            {
                Query = PolicyQuery,
                Variables = new
                {
                    policyId = policyId
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var jsonEle = response["policy"];
            var result = jsonEle.ToObject<PolicyWithRoles>();

            // Assert
            Assert.Equal(policyId.ToString(), result.Id.ToString());
            Assert.Equal(result.Roles.Count, roles.Count);
            Assert.All(result.Roles, r => roles.Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Policy_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = PolicyQuery,
                Variables = new
                {
                    policyId = Guid.NewGuid()
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);

            //Assert
            var errors = ((GraphQLResponse<dynamic>)response).Errors;
            var extensions = errors.First().Extensions["code"];
            Assert.NotNull(extensions);
            Assert.Equal("notFound", extensions);
        }

        [Fact, Order(0)]
        public async Task Get_Policies_Test()
        {
            // Arrange
            const int size = 10;
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, size);
            var rolesDictionary = await GetRolesDictionary(policies.Data.Select(p => p.Id));
            var policiesCount = policies.Data.Count;
            var request = new GraphQLRequest
            {
                Query = PoliciesQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["policies"]["policies"];
            var result = jsonEle.ToObject<IReadOnlyCollection<PolicyWithRoles>>();

            Assert.Equal(policiesCount, result.Count);
            Assert.All(result, r => policies.Data.Any(p => p.Id == r.Id));
            foreach (var p in result)
            {
                Assert.Equal(p.Roles.Count, rolesDictionary[p.Id].Count);
                Assert.All(p.Roles, r => rolesDictionary[p.Id].Any(p => p.Id == r.Id));
            }
        }

        [Theory(Skip = "Flaky test"), Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Policies_With_Paging_Test(int size, int page)
        {
            // Arrange
            var policies = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, page, size);
            var policiesCount = policies.Data.Count;
            var rolesDictionary = await GetRolesDictionary(policies.Data.Select(p => p.Id));
            var request = new GraphQLRequest
            {
                Query = PoliciesQueryWithSizeAndPage,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["policies"]["policies"];
            var result = jsonEle.ToObject<IReadOnlyCollection<PolicyWithRoles>>();

            Assert.Equal(policiesCount, result.Count);
            Assert.All(result, r => policies.Data.Any(p => p.Id == r.Id));
            foreach (var p in result)
            {
                Assert.Equal(p.Roles.Count, rolesDictionary[p.Id].Count);
                Assert.All(p.Roles, r => rolesDictionary[p.Id].Any(p => p.Id == r.Id));
            }
        }

        [Fact, Order(1)]
        public async Task Create_Policy_Test()
        {
            // Arrange
            var policyName = Guid.NewGuid().ToString();
            var policyDescr = Guid.NewGuid().ToString();
            var rootPolicy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();
            var mutation = string.Format(CreatePolicyMutation, rootPolicy.Id, policyName, policyDescr);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            //Assert
            Guid guid;
            Assert.True(Guid.TryParse(response.createPolicy.id.ToString(), out guid));
        }

        [Fact, Order(1)]
        public async Task Create_Policy_And_Relation_Success_When_Parent_Is_Present()
        {
            // Arrange
            var policyName = Guid.NewGuid().ToString();
            var policyDescr = Guid.NewGuid().ToString();
            var rootPolicy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();
            var mutation = string.Format(CreatePolicyMutation, rootPolicy.Id, policyName, policyDescr);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            //Assert
            Assert.True(Guid.TryParse(response.createPolicy.id.ToString(), out Guid guid));

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Policy, Policy>(p => p.Id == guid,
                rp => rp.Id == rootPolicy.Id, Constants.ChildOfLink);
            Assert.True(hasLink);
        }

        [Fact, Order(1)]
        public async Task Create_Policy_And_Relation_Null_When_Parent_Is_Null()
        {
            // Arrange
            var policyName = Guid.NewGuid().ToString();
            var policyDescr = Guid.NewGuid().ToString();
            var rootPolicy = Guid.Empty;
            var mutation = string.Format(CreatePolicyMutation, rootPolicy, policyName, policyDescr);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = _fixture.ExtractGraphqlErrorsExtensions(response);

            // Assert
            Assert.True(errors.ContainsKey("policy"));
        }

        [Fact, Order(int.MaxValue)]
        public async Task Delete_Policy_Test()
        {
            // Arrange
            var rootPolicy =
                (await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Policy>(p => true, 0, 1)).Data.First();
            var mutation = string.Format(DeletePolicyMutation, rootPolicy.Id);
            var request = new GraphQLRequest(mutation);

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            //Assert
            var jsonEle = response["deletePolicy"];
            Assert.Equal(rootPolicy.Id.ToString(), jsonEle.ToString());
        }
        
        private async Task<Dictionary<Guid, IReadOnlyList<Role>>> GetRolesDictionary(IEnumerable<Guid> ids)
        {
            var rolesMap = new Dictionary<Guid, IReadOnlyList<Role>>();
            foreach (var id in ids)
            {
                var roles = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Policy, Role>(
                    policy => policy.Id == id, Constants.ContainsLink)).ToList();
                rolesMap.Add(id, roles);
            }

            return rolesMap;
        }
    }
}