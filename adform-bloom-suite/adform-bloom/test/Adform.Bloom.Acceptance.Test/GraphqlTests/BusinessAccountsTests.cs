using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Read.Contracts.BusinessAccount;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;
using Feature = Adform.Bloom.Domain.Entities.Feature;
using LicensedFeature = Adform.Bloom.Domain.Entities.LicensedFeature;
using Permission = Adform.Bloom.Domain.Entities.Permission;
using Role = Adform.Bloom.Domain.Entities.Role;
using Subject = Adform.Bloom.Contracts.Output.Subject;
using Tenant = Adform.Bloom.Domain.Entities.Tenant;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLBusinessAccountsCollection))]
    public class BusinessAccountsTests
    {
        private const string TenantQuery = @"
query businessAccountQuery($tenantId: ID!) {
  businessAccount(id: $tenantId) {
    id
    name
    legacyId
    roles(pagination:{ limit: 100, offset: 0 }) {
      roles {
          id
          name
      }
    }
    users(pagination:{ limit: 100, offset: 0 }) {
      users{
          id
          username
      }
    }
	}
}";

        private const string TenantsQueryWithSizeAndPage = @"
query businessAccountsQuery($limit: Limit!, $offset: Int!) {
  businessAccounts(pagination:{ limit: $limit, offset: $offset}) {
    businessAccounts {
      id
      name
      legacyId
      roles(pagination:{ limit: 100, offset: 0 })
      {
        roles{
            id
            name
        }
      }
      users(pagination:{ limit: 100, offset: 0 })
      {
        users {
            id
            username
        }
      }
    }
  }
}";

        private const string TenantsQueryWithBusinessAccountType = @"
query businessAccountsQuery($type:BusinessAccountType!, $limit: Limit!, $offset: Int!) {
  businessAccounts(businessAccountType: $type, pagination:{ limit: $limit, offset: $offset}) {
    businessAccounts {
      id
      name
      legacyId
      type
    }
  }
}";

        private const string TenantQueryWithLicenseFeatures = @"
query businessAccountQuery($tenantId: ID!) {
  businessAccount(id: $tenantId) {
    id
    name
    type
    licensedFeatures(pagination:{offset:0, limit:3}) {
      totalCount
      licensedFeatures {
        name
      }
    }
	}
}";

        private const string AssignLicenseFeatureToTenant = @"
mutation{{
  updateBusinessAccountToLicensedFeaturesAssignments(
    businessAccountId: ""{0}""
    assignLicensedFeatureIds: [""{1}"",""{2}"", ""{3}""]
  )
}}
";

        private const string UnassignLicenseFeatureToTenant = @"
mutation{{
  updateBusinessAccountToLicensedFeaturesAssignments(
    businessAccountId: ""{0}""
    unassignLicensedFeatureIds: [""{1}"",""{2}""]
  )
}}
";

        private const string UpdateLicenseFeatureToTenantAssignments = @"
mutation{{
  updateBusinessAccountToLicensedFeaturesAssignments(
    businessAccountId: ""{0}""
    assignLicensedFeatureIds: [""{1}"",""{2}"", ""{3}""]
    unassignLicensedFeatureIds: [""{4}"",""{5}""]
  )
}}
";

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public BusinessAccountsTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Query Tests
        
        [Fact, Order(0)]
        public async Task Get_BusinessAccount_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var nodeId = node.Id;

            var request = new GraphQLRequest
            {
                Query = TenantQuery,
                Variables = new {tenantId = nodeId}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            Assert.Equal(nodeId.ToString(), response.businessAccount.id.ToString());
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_BusinessAccount_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = TenantQuery,
                Variables = new {tenantId = Guid.NewGuid()}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_BusinessAccount_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = TenantQuery,
                Variables = new {tenantId = Graph.Tenant2}
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject6, request, true);
            var errors = response.Errors;

            Assert.True(errors.Length == 1);
            Assert.Equal("The subject of the token does not have access to a given entity.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_BusinessAccounts_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 0, size);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = TenantsQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["businessAccounts"]["businessAccounts"];
            var result = jsonEle.ToObject<IReadOnlyCollection<BusinessAccount>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_BusinessAccounts_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), page, size);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = TenantsQueryWithSizeAndPage,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["businessAccounts"]["businessAccounts"];
            var result = jsonEle.ToObject<IReadOnlyCollection<BusinessAccount>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
        }

        [Fact]
        public async Task Get_BusinessAccounts_With_Users()
        {
            // Arrange
            var nodes = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount(), 0, 100)).Data;
            var ids = nodes.Select(x => x.Id).ToList();
            var request = new GraphQLRequest
            {
                Query = TenantsQueryWithSizeAndPage,
                Variables = new
                {
                    offset = 0,
                    limit = 100
                }
            };

            var expectedUsersIds = ids
                .Select(async t => new KeyValuePair<Guid, Guid[]>(t,
                    (await _fixture.OngDB.GraphRepository.GetConnectedWithIntermediateAsync<Tenant, Group, Subject>
                        (x => x.Id == t, Constants.BelongsIncomingLink, Constants.MemberOfIncomingLink))
                    .Select(s => s.Id).Distinct().ToArray())).Select(subjects => subjects.Result)
                .ToDictionary(tenant => tenant.Key, subjects => subjects.Value);

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            foreach (var businessAccount in response["businessAccounts"]["businessAccounts"])
            {
                var baId = businessAccount.ToObject<BusinessAccount>().Id;
                var result = businessAccount["users"]["users"].ToObject<IReadOnlyCollection<User>>();
                var expected = expectedUsersIds[baId];
                var actual = result.Select(x => x.Id).ToList();
                Assert.All(expected, u => actual.Contains(u));
                Assert.Equal(expected.Distinct().Count(), actual.Distinct().Count());
            }
        }

        [Theory]
        [InlineData(BusinessAccountType.Adform)]
        [InlineData(BusinessAccountType.Agency)]
        [InlineData(BusinessAccountType.Publisher)]
        [InlineData(BusinessAccountType.DataProvider)]
        public async Task Get_BusinessAccounts_With_BusinessAccountType(BusinessAccountType type)
        {
            // Arrange
            var nodes = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsBusinessAccount, Contracts.Output.Tenant>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsBusinessAccount
                    {
                        BusinessAccountType = (int) type
                    }, 0, 100));

            var nodesCount = nodes.Data.Count;
            var ids = nodes.Data.Select(x => x.Id).ToList();
            var request = new GraphQLRequest
            {
                Query = TenantsQueryWithBusinessAccountType,
                Variables = new
                {
                    offset = 0,
                    limit = 100,
                    type = type.ToString().ToLowerFirstCharacter()
                }
            };
            
            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["businessAccounts"]["businessAccounts"];
            var result = jsonEle.ToObject<IReadOnlyCollection<BusinessAccount>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.True(result.All(x=>x.Type == type));
            Assert.True(result.Select(p=>p.Id).OrderBy(p=>p).SequenceEqual(ids.OrderBy(p=>p)));
        }
        
        [Fact]
        public async Task Get_BusinessAccounts_With_LicenseFeatures()
        {
            // Arrange
            var id = Graph.Tenant2;
            var request = new GraphQLRequest
            {
                Query = TenantQueryWithLicenseFeatures,
                Variables = new {tenantId = id}
            };

            var expectedLfNames =
                (await _fixture.OngDB.GraphRepository
                    .GetConnectedAsync<Tenant, LicensedFeature>(
                        x => x.Id.Equals(id), Constants.AssignedLink))
                .Where(p => p.IsEnabled)
                .Select(x => x.Name)
                .Distinct();

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert

            var jsonEle = response["businessAccount"]["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeature>>();

            Assert.True(expectedLfNames.Intersect(result.Select(x => x.Name)).Count() ==
                        expectedLfNames.Count());
        }

        #endregion

        #region Mutation Tests

        [Fact, Order(2)]
        public async Task Assign_LicensedFeature_To_Tenant()
        {
            // Arrange
            const string tenantId = Graph.Tenant13;
            const string licenseFeatureId = Graph.LicensedFeature2;
            const string currentLicensedFeatureId = Graph.LicensedFeature4;
            const string traffickerRoleId = Graph.Trafficker13Role;
            var traffickerPermissionsBefore = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id);
            var createDynamic = await GenerateAddionalLicensedFeatureAndFeatures();
            var permissions = new List<Guid>
            {
                Guid.Parse(Graph.Permission5),
                Guid.Parse(Graph.Permission6),
                Guid.Parse(Graph.Permission7),
                Guid.Parse(Graph.Permission8)
            };
            permissions.AddRange(createDynamic.permissions.Select(p => p.Id));
            var licensedFeatures = new List<Guid>
            {
                Guid.Parse(licenseFeatureId),
                createDynamic.licensedFeature.Id
            };

            var beforeLicensedFeaturesAssignment = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);
            var mutationAssign = string.Format(AssignLicenseFeatureToTenant, tenantId, licenseFeatureId, currentLicensedFeatureId,
                createDynamic.licensedFeature.Id);
            var requestAssign = new GraphQLRequest(mutationAssign);


            // Act
            var response = (JObject)(await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestAssign));

            //Assert
            var jsonEle = response["updateBusinessAccountToLicensedFeaturesAssignments"];
            Assert.Equal(tenantId, jsonEle.ToString());
            var afterLicensedFeaturesAssignment = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);
            Assert.Equal(licensedFeatures.Count + beforeLicensedFeaturesAssignment.Count, afterLicensedFeaturesAssignment.Count);
            var beforeIds = beforeLicensedFeaturesAssignment.Select(o => o.Id).ToList();
            var afterIds = afterLicensedFeaturesAssignment.Select(o => o.Id).ToList();
            var subtractionIds = afterIds.Except(beforeIds).OrderBy(o => o).ToList();
            var licensedFeaturesIds = licensedFeatures.OrderBy(o => o).Distinct().ToList();
            Assert.True(subtractionIds.SequenceEqual(licensedFeaturesIds));


            var traffickerPermissionsAfter = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id);

            var diffPermissions = traffickerPermissionsAfter.Except(traffickerPermissionsBefore).OrderBy(p => p);

            Assert.True(diffPermissions.SequenceEqual(permissions.OrderBy(p => p)));
        }

        [Fact, Order(3)]
        public async Task Unassign_LicensedFeature_From_Tenant()
        {
            // Arrange
            const string tenantId = Graph.Tenant2;
            const string licenseFeatureId = Graph.LicensedFeature3;
            const string traffickerRoleId = Graph.Trafficker2Role;

            await _fixture.OngDB.GraphRepository.AssignLicensedFeaturesToTenantAsync(
                Guid.Parse(tenantId),
                new List<Guid>()
                {
                    Guid.Parse(licenseFeatureId)
                });

            var createDynamic = await GenerateAddionalLicensedFeatureAndFeatures();

            var licensedFeatures = new List<Guid>
            {
                Guid.Parse(licenseFeatureId),
                createDynamic.licensedFeature.Id
            };

            var permissions = new List<Guid>
            {
                Guid.Parse(Graph.Permission9),
                Guid.Parse(Graph.Permission10),
                Guid.Parse(Graph.Permission11),
                Guid.Parse(Graph.Permission12)
            };
            permissions.AddRange(createDynamic.permissions.Select(p => p.Id));

            var traffickerPermissionsBefore = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id).OrderBy(p => p).ToList();

            await _fixture.OngDB.GraphRepository.AssignLicensedFeaturesToTenantAsync(
                Guid.Parse(tenantId), licensedFeatures);

            await _fixture.OngDB.GraphRepository.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(licensedFeatures, Guid.Parse(tenantId), new[] { Constants.Label.TRAFFICKER_ROLE });

            var beforeLicensedFeaturesAssignment = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);

            var traffickerPermissionsAfterAssignment = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id);

            var mutationUnassign = string.Format(UnassignLicenseFeatureToTenant, tenantId, licenseFeatureId,
                createDynamic.licensedFeature.Id);
            var requestUnassign = new GraphQLRequest(mutationUnassign);


            // Act
            var response = (JObject)(await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestUnassign));

            //Assert
            var jsonEle = response["updateBusinessAccountToLicensedFeaturesAssignments"];
            Assert.Equal(tenantId, jsonEle.ToString());
            var afterLicensedFeaturesUnassignment = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);

            Assert.Equal(Math.Abs(licensedFeatures.Count - beforeLicensedFeaturesAssignment.Count), afterLicensedFeaturesUnassignment.Count);
            var beforeIds = beforeLicensedFeaturesAssignment.Select(o => o.Id).ToList();
            var afterIds = afterLicensedFeaturesUnassignment.Select(o => o.Id).ToList();
            var substractionIds = beforeIds.Except(afterIds).OrderBy(o => o).ToList();
            var featuresIds = licensedFeatures.OrderBy(o => o).Distinct().ToList();
            Assert.True(substractionIds.SequenceEqual(featuresIds));

            var traffickerPermissionsAfterUnassignment = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id).OrderBy(p => p);

            var diffPermissions = traffickerPermissionsAfterAssignment.Except(traffickerPermissionsBefore).OrderBy(p => p);

            Assert.True(diffPermissions.SequenceEqual(permissions.OrderBy(p => p)));
            Assert.True(traffickerPermissionsBefore.SequenceEqual(traffickerPermissionsAfterUnassignment));
        }


        [Fact, Order(4)]
        public async Task Update_LicensedFeature_Assignments()
        {
            // Arrange
            const string tenantId = Graph.Tenant2;
            const string traffickerRoleId = Graph.Trafficker2Role;

            var createDynamicUnassign = await GenerateAddionalLicensedFeatureAndFeatures();
            var createDynamicAssign = await GenerateAddionalLicensedFeatureAndFeatures();

            var unassignLicensedFeatureIds = new[] {Guid.Parse(Graph.LicensedFeature3), createDynamicUnassign.licensedFeature.Id};
            var assignLicensedFeatureIds = new[] { Guid.Parse(Graph.LicensedFeature4), createDynamicAssign.licensedFeature.Id, Guid.Parse(Graph.LicensedFeature2)};

            var unassignPermissions = new List<Guid>
            {
                Guid.Parse(Graph.Permission9),
                Guid.Parse(Graph.Permission10),
                Guid.Parse(Graph.Permission11),
                Guid.Parse(Graph.Permission12)
            };
            unassignPermissions.AddRange(createDynamicUnassign.permissions.Select(p => p.Id));

            var assignPermissions = new List<Guid>
            {
                Guid.Parse(Graph.Permission13),
                Guid.Parse(Graph.Permission14),
                Guid.Parse(Graph.Permission15),
                Guid.Parse(Graph.Permission16)
            };
            assignPermissions.AddRange(createDynamicAssign.permissions.Select(p=>p.Id));


            await _fixture.OngDB.GraphRepository.AssignLicensedFeaturesToTenantAsync(
                Guid.Parse(tenantId), unassignLicensedFeatureIds);

            await _fixture.OngDB.GraphRepository.AssignPermissionsToRolesThroughLicensedFeatureAssignmentsAsync(unassignLicensedFeatureIds, Guid.Parse(tenantId), new[] { Constants.Label.TRAFFICKER_ROLE });
            
            var beforeUpdate = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);

            var traffickerPermissionsBefore = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id).OrderBy(p => p).ToList();

            var mutationUnassign = string.Format(UpdateLicenseFeatureToTenantAssignments, tenantId, assignLicensedFeatureIds[0], assignLicensedFeatureIds[1], assignLicensedFeatureIds[2], unassignLicensedFeatureIds[0], unassignLicensedFeatureIds[1]);
            var requestUnassign = new GraphQLRequest(mutationUnassign);


            // Act
            var response = (JObject)(await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestUnassign));

            //Assert
            var jsonEle = response["updateBusinessAccountToLicensedFeaturesAssignments"];
            Assert.Equal(tenantId, jsonEle.ToString());

            // Assert
            var afterUpdate = await _fixture.OngDB.GraphRepository.GetConnectedAsync<Tenant, LicensedFeature>(
                o => o.Id == Guid.Parse(tenantId), Constants.AssignedLink);

            var beforeUpdateIds = beforeUpdate.Select(o => o.Id).ToList();
            var afterUpdateIds = afterUpdate.Select(o => o.Id).ToList();
            var actualAssignedIds = afterUpdateIds.Except(beforeUpdateIds).OrderBy(o => o).Distinct().ToArray();
            var actualUnassigedIds = beforeUpdateIds.Except(afterUpdateIds).OrderBy(o => o).Distinct().ToArray();
            unassignLicensedFeatureIds = unassignLicensedFeatureIds.OrderBy(o => o).Distinct().ToArray();
            assignLicensedFeatureIds = assignLicensedFeatureIds.Except(beforeUpdateIds).ToArray().OrderBy(o => o).Distinct().ToArray();
            Assert.Equal(beforeUpdate.Count - unassignLicensedFeatureIds.Length + assignLicensedFeatureIds.Except(beforeUpdateIds).ToArray().Length, afterUpdate.Count);
            Assert.Equal(assignLicensedFeatureIds.Length, actualAssignedIds.Length);
            Assert.Equal(unassignLicensedFeatureIds.Length, actualUnassigedIds.Length);
            Assert.True(assignLicensedFeatureIds.SequenceEqual(actualAssignedIds));
            Assert.True(unassignLicensedFeatureIds.SequenceEqual(actualUnassigedIds));



            var traffickerPermissionsAfter = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Role, Permission>(
                o => o.Id == Guid.Parse(traffickerRoleId),
                Constants.ContainsLink)).Select(p => p.Id).OrderBy(p => p);

            var actualAssignedPermissionIds = traffickerPermissionsAfter.Except(traffickerPermissionsBefore).OrderBy(o => o).Distinct().ToArray();
            var actualUnassigedPermissionIds = traffickerPermissionsBefore.Except(traffickerPermissionsAfter).OrderBy(o => o).Distinct().ToArray();
            var unassignPermissionIds = unassignPermissions.OrderBy(o => o).Distinct().ToArray();
            var assignPermissionIds = assignPermissions.OrderBy(o => o).Distinct().ToArray();
            Assert.Equal(assignPermissionIds.Length, actualAssignedPermissionIds.Length);
            Assert.Equal(unassignPermissionIds.Length, actualUnassigedPermissionIds.Length);
            Assert.True(assignPermissionIds.SequenceEqual(actualAssignedPermissionIds));
            Assert.True(unassignPermissionIds.SequenceEqual(actualUnassigedPermissionIds));
        }

        #endregion

        public async Task<(LicensedFeature licensedFeature, IReadOnlyCollection<Feature> features, IReadOnlyCollection<Permission> permissions)>
            GenerateAddionalLicensedFeatureAndFeatures()
        {
            var permission =
                await _fixture.OngDB.GraphRepository.CreateNodeAsync(new Permission("TEMPORAL_PERMISSION"));
            var feature = await _fixture.OngDB.GraphRepository.CreateNodeAsync(new Feature("TEMPORAL_FEATURE"));
            var licensedFeature =
                await _fixture.OngDB.GraphRepository.CreateNodeAsync(new LicensedFeature("TEMPORAL_LICENSEDFEATURE"));
            await _fixture.OngDB.GraphRepository.CreateRelationshipAsync<LicensedFeature, Feature>(
                o => o.Id == licensedFeature.Id,
                p => p.Id == feature.Id, Constants.ContainsLink);
            await _fixture.OngDB.GraphRepository.CreateRelationshipAsync<Feature, Permission>(o => o.Id == feature.Id,
                p => p.Id == permission.Id, Constants.ContainsLink);

            return (licensedFeature, new[] { feature }, new[] { permission });
        }
    }
}