using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Infrastructure.Models;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;
using LicensedFeature = Adform.Bloom.Contracts.Output.LicensedFeature;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLFeaturesCollection))]
    public class FeaturesTests
    {
        private const string FeatureQuery = @"
query featureQuery($featureId:ID!) {
  feature(id:$featureId) {
    id
    name
    description
    enabled
    permissions
    {
        id
        name
    }
    businessAccounts(pagination: {offset: 0, limit:100})
    {
        businessAccounts{
            id
            name
        }
    }
    licensedFeature
    {
        id
        name
    }
    features
    {
        id
        name
    }
    }
}";

        private const string FeaturesQueryWithSize = @"
query featuresQuery($offset:Int!, $limit:Limit!) {
  features(pagination:{limit:$limit, offset:$offset}) {
    features{
        id
        name
        description
        enabled
        permissions {
            id
            name
        }
    businessAccounts(pagination: {offset: 0, limit:100})
    {
        businessAccounts{
            id
            name
        }
    }
        licensedFeature {
            id
            name
        }
	    }
    }
}";

        private const string CreateFeatureMutation = @"
mutation createFeatureMutation($featureName:String!, $featureDescription:String!, $featureEnabled:Boolean!) {
  createFeature(feature:{
    name: $featureName,
    description: $featureDescription,
    enabled: $featureEnabled })
    {
        id
    }
}";

        private const string DeleteFeatureMutation = @"
mutation deleteFeatureMutation($featureId:ID!) {
  deleteFeature(id: $featureId)
}";

        private const string AssignToFeatureMutation = @"
mutation updatePermissionToFeatureAssignmentsMutation($permissionId:ID!, $featureId:ID!, $operation:LinkOperation!) {
  updatePermissionToFeatureAssignments(assignment:{
    permissionId: $permissionId,
    featureId: $featureId,
    operation: $operation})
}";

        private const string AssignToFeatureCoDependency = @"
mutation updateFeatureCoDependencyMutation($featureId:ID!, $dependentOnId:ID!, $operation:LinkOperation!) {
  updateFeatureCoDependency(
    featureId: $featureId,
    dependentOnId: $dependentOnId,
    operation: $operation)
}";

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private class FeatureWithLicensedFeatures : Policy
        {
            public LicensedFeature LicensedFeature { get; set; }
            public IReadOnlyCollection<Feature> Features { get; set; }
        }

        private readonly TestsFixture _fixture;

        public FeaturesTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact, Order(0)]
        public async Task Get_Feature_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var licensedFeature = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Feature, LicensedFeature>(
                f => f.Id == node.Id,
                Constants.ContainsIncomingLink)).ToList();
            var nodeId = node.Id;
            var request = new GraphQLRequest
            {
                Query = FeatureQuery,
                Variables = new { featureId = nodeId }
            };

            // Act
            var res = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var result = response["feature"].ToObject<FeatureWithLicensedFeatures>();

            // Assert
            Assert.Equal(nodeId.ToString(), result.Id.ToString());
            Assert.Equal(node.Name, result.Name.ToString());
            Assert.Equal(node.Description, result.Description.ToString());
            Assert.Equal(node.Enabled.ToString(), result.IsEnabled.ToString());
            Assert.NotNull(licensedFeature.Single(p => p.Id == result.LicensedFeature.Id));
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_Feature_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = FeatureQuery,
                Variables = new { featureId =  Guid.NewGuid() }
            };
            
            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Forbidden_Feature_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = (await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject3],
                    new QueryParamsTenantIds(), 0, size)).Data.Where(f => !new[] { Graph.Feature5, Graph.Feature6 }.Contains(f.Id.ToString())).ToArray();
            var nodeId = nodes[Random.Next(0, nodes.Length - 1)].Id;
            var request = new GraphQLRequest
            {
                Query = FeatureQuery,
                Variables = new { featureId = nodeId }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject6, request, true);
            var errors = response.Errors;

            Assert.True(errors.Length == 1);
            Assert.Equal("The subject of the token does not have access to a given entity.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_Features_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIds(), 0, size);
            var licensedFeatureDictionary = await GetLicensedFeatureDictionary(nodes.Data.Select(p => p.Id));
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = FeaturesQueryWithSize,
                Variables = new { 
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["features"]["features"];
            var result = jsonEle.ToObject<IReadOnlyCollection<FeatureWithLicensedFeatures>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
            foreach (var f in result)
            {
                Assert.Equal(f.LicensedFeature.Id, licensedFeatureDictionary[f.Id].Id);
            }
        }

        [Fact, Order(0)]
        public async Task Get_Features_DoesNotReturnDisabled()
        {
            // Arrange
            const int size = 10;
            var cypher =  _fixture.OngDB.GraphClient.Cypher.Match("(f:Feature)")
                .Return(f => f.As<Feature>());
            var nodes = (await cypher.ResultsAsync).ToList();
            var nodesCount = nodes.Count;
            var request = new GraphQLRequest
            {
                Query = FeaturesQueryWithSize,
                Variables = new { 
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject)(await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["features"]["features"];
            var result = jsonEle.ToObject<IReadOnlyCollection<FeatureWithLicensedFeatures>>();

            Assert.NotEqual(nodesCount, result.Count);
            Assert.True(result.All(f => f.IsEnabled));
        }

        [Theory, Order(0)]
        [InlineData(3, 2)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_Features_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIds, Contracts.Output.Feature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0], 
                    new QueryParamsTenantIds(), page, size);
            var licensedFeatureDictionary = await GetLicensedFeatureDictionary(nodes.Data.Select(p => p.Id));
            var nodesCount = nodes.Data.Count();
            var request = new GraphQLRequest
            {
                Query = FeaturesQueryWithSize,
                Variables = new { 
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["features"]["features"];
            var result = jsonEle.ToObject<IReadOnlyCollection<FeatureWithLicensedFeatures>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
            foreach (var f in result)
            {
                Assert.Equal(f.LicensedFeature.Id, licensedFeatureDictionary[f.Id].Id);
            }
        }

        [Theory, Order(1)]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Create_Feature_Test(bool enabled)
        {
            // Arrange
            var nodeName = Guid.NewGuid().ToString();
            var nodeDescription = Guid.NewGuid().ToString();

            // Act
            var guid = await CreateFeatureAsync(nodeName, nodeDescription, enabled);

            // Assert
            var node =
                await _fixture.OngDB.GraphRepository.GetNodeAsync<Feature>(f => f.Id == guid);
            Assert.NotNull(node);
            Assert.Equal(nodeName, node.Name);
            Assert.Equal(nodeDescription, node.Description);
            Assert.Equal(enabled, node.IsEnabled);
        }

        [Fact, Order(2)]
        public async Task Delete_Feature_Test()
        {
            // Arrange
            var nodeId = await CreateFeatureAsync(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), true);
            var nodes = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Feature>(p => true);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = DeleteFeatureMutation,
                Variables = new { featureId = nodeId }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            //Assert
            using var jsonDoc = JsonDocument.Parse(response.ToString());
            var jsonEle = response["deleteFeature"];
            Assert.Equal(nodeId.ToString(), jsonEle.ToString());
            nodes = await _fixture.OngDB.GraphRepository.SearchPaginationAsync<Feature>(p => true);
            var newNodesCount = nodes.Data.Count;
            Assert.Equal(nodesCount - 1, newNodesCount);
        }

        [Fact, Order(2)]
        public async Task Assign_Permission_To_Feature()
        {
            // Arrange
            const string featureId = Graph.Feature1;
            var permission = await _fixture.OngDB.GraphRepository.CreateNodeAsync(new Permission
            {
                Name = "TEMPORAL_PERMISSION",
            });
            
            var requestAssign = new GraphQLRequest
            {
                Query = AssignToFeatureMutation,
                Variables = new
                {
                    permissionId = permission.Id,
                    featureId = featureId,
                    operation = LinkOperation.Assign.ToString().ToLowerInvariant()
                }
            };
            
            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestAssign));

            //Assert
            using var jsonAssign = JsonDocument.Parse(response.ToString());
            var jsonEle = response["updatePermissionToFeatureAssignments"];
            Assert.Equal(featureId, jsonEle.ToString());
            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Feature, Permission>(
                p => p.Id == Guid.Parse(featureId),
                rp => rp.Id == permission.Id, Constants.ContainsLink);
            Assert.True(hasLink);
        }

        [Fact, Order(2)]
        public async Task Unassign_Permission_From_Feature()
        {
            // Arrange
            const string featureId = Graph.Feature4;
            const string permissionId = Graph.Permission7;
            const string roleId = Graph.CustomRole8;
            const string notUnassignedPermissionId = Graph.Permission8;

            var featureToPermissionExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Feature, Permission>(
                    f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            var roleToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            var featureToPermissionNotToUnassignExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(notUnassignedPermissionId),
                    Constants.ContainsLink);

            var requestAssign = new GraphQLRequest
            {
                Query = AssignToFeatureMutation,
                Variables = new
                {
                    permissionId = permissionId,
                    featureId = featureId,
                    operation = LinkOperation.Unassign.ToString().ToLowerInvariant()
                }
            };

            // Act
            Assert.True(featureToPermissionExists);
            Assert.True(roleToPermissionExists);
            Assert.True(featureToPermissionNotToUnassignExists);
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestAssign));

            // Assert
            var jsonEle = response["updatePermissionToFeatureAssignments"];
            Assert.Equal(featureId, jsonEle.ToString());
            featureToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Feature, Permission>(
                f => f.Id == Guid.Parse(featureId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            Assert.False(featureToPermissionExists);
            roleToPermissionExists = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(permissionId), Constants.ContainsLink);
            Assert.False(roleToPermissionExists);
            featureToPermissionNotToUnassignExists =
                await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Role, Permission>(
                    f => f.Id == Guid.Parse(roleId), p => p.Id == Guid.Parse(notUnassignedPermissionId),
                    Constants.ContainsLink);
            Assert.True(featureToPermissionNotToUnassignExists);
        }

        [Fact, Order(2)]
        public async Task Assign_Feature_CoDependency()
        {
            // Arrange
            const string feature1 = Graph.Feature1;
            const string feature2 = Graph.Feature2;
            
            var requestAssign = new GraphQLRequest
            {
                Query = AssignToFeatureCoDependency,
                Variables = new { 
                    featureId = feature1,
                    dependentOnId = feature2,
                    operation = LinkOperation.Assign.ToString().ToLowerInvariant()
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestAssign));

            //Assert
            var jsonEle = response["updateFeatureCoDependency"];
            Assert.Equal(feature1, jsonEle.ToString());
            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Feature, Feature>(
                f1 => f1.Id == Guid.Parse(feature1),
                f2 => f2.Id == Guid.Parse(feature2), Constants.DependsOnLink);
            Assert.True(hasLink);
        }

        [Fact, Order(0)]
        public async Task Get_Feature_With_CoDependency_Test()
        {
            // Arrange
            const string nodeId = Graph.Feature8;
            var request = new GraphQLRequest
            {
                Query = FeatureQuery,
                Variables = new { featureId = nodeId }
            };
            var node = await _fixture.OngDB.GraphRepository.GetNodeAsync<Feature>(o => o.Id == Guid.Parse(nodeId));
            node.IsEnabled = true;
            node = await _fixture.OngDB.GraphRepository.UpdateNodeAsync<Feature>(p => p.Id == Guid.Parse(nodeId), node);

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var jsonEle = response["feature"];
            var result = jsonEle.ToObject<FeatureWithLicensedFeatures>();

            // Assert
            Assert.Equal(nodeId, result.Id.ToString());
            Assert.Equal(Graph.Feature7, result.Features.First().Id.ToString());
        }

        private async Task<Guid> CreateFeatureAsync(string name, string description, bool isEnabled)
        {
            
            var request = new GraphQLRequest
            {
                Query = CreateFeatureMutation,
                Variables = new
                {
                    featureName = name,
                    featureDescription = description,
                    featureEnabled = isEnabled
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            return Guid.Parse(response.createFeature.id.ToString());
        }

        private async Task<Dictionary<Guid, LicensedFeature>> GetLicensedFeatureDictionary(IEnumerable<Guid> ids)
        {
            var licensedFeatureDictionary = new Dictionary<Guid, LicensedFeature>();
            foreach (var id in ids)
            {
                var licensedFeature = (await _fixture.OngDB.GraphRepository.GetConnectedAsync<Feature, LicensedFeature>(
                    policy => policy.Id == id, Constants.ContainsIncomingLink)).ToList();
                licensedFeatureDictionary.Add(id, licensedFeature.Single());
            }

            return licensedFeatureDictionary;
        }
    }
}