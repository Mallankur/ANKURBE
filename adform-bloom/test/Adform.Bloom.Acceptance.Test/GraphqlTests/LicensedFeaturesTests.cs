using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Api.Graph.Common;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Ciam.OngDb.Repository;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLLicensedFeaturesCollection))]
    public class LicensedFeaturesTests
    {
        #region Constatnts

        private const string LicensedFeatureQuery = @"
query licensedFeatureQuery($licensedId: ID!) {
  licensedFeature(id: $licensedId) {
    id
    name
    description
    enabled
    features {
        features {
            id
            name
        }
    }
	}
}";

        private const string LicensedFeatureWithFilteredBusinessAccountQuery = @"
query licensedFeatureQuery($licensedId: ID!, $businessAccountIds: [ID!]) {
  licensedFeature(id: $licensedId,businessAccountIds: $businessAccountIds) {
    id
    name
    description
    enabled
    features(businessAccountIds:$businessAccountIds) {
        features {
            id
            name
        }
    }
	}
}";

        private const string LicensedFeaturesQueryWithSize = @"
query licensedFeaturesQuery($offset: Int!, $limit:Limit!) {
  licensedFeatures(pagination:{ offset: $offset, limit: $limit }) {
    licensedFeatures{
        id
        name
        description
        enabled
        features {
            features {
                id
                name
            }
        }
	    }
    }
}";

        private const string LicensedFeaturesBusinessAccountFilteredQueryWithSize = @"
query licensedFeaturesQuery($offset: Int!, $limit:Limit!,$businessAccountIds: [ID!]) {
  licensedFeatures(pagination:{ offset: $offset, limit: $limit }) {
    licensedFeatures{
        id
        name
        description
        enabled
        features(businessAccountIds:$businessAccountIds) {
            features {
                id
                name
            }
        }
	    }
    }
}";

        private const string LicensedFeaturesWithSearch = @"
query licensedFeaturesQuery($search: String) {
  licensedFeatures(search:$search,pagination:{ offset: 0, limit:10}) {
    licensedFeatures{
        id
        name
        description
        enabled
    }
    }
}";

        private const string LicensedFeaturesWithSort = @"
query licensedFeaturesQuery($fieldName: String!, $order: SortingOrder!) {
  licensedFeatures(sortBy:{fieldName:$fieldName, order: $order}, pagination:{ offset: 0, limit:10}) {
    licensedFeatures{
        id
        name
        description
        enabled
    }
    }
}";

        private const string LicensedFeaturesWithPolicyTypes = @"
query licensedFeaturesQuery($policyTypes:[PolicyTypeInput!]) {
  licensedFeatures(productNames:$policyTypes,pagination:{ offset: 0, limit:10}) {
    licensedFeatures{
        id
        name
        description
        enabled
    }
    }
}";
        
        private const string AssignLicenseFeatureToTenant = @"
mutation{{
  updateLicensedFeaturesToBusinessAccountAssignments(
    businessAccountId: ""{0}""
    assignLicensedFeatureIds: [""{1}"",""{2}"", ""{3}""]
  )
}}
";

        private const string UnassignLicenseFeatureToTenant = @"
mutation{{
  updateLicensedFeaturesToBusinessAccountAssignments(
    businessAccountId: ""{0}""
    unassignLicensedFeatureIds: [""{1}"",""{2}""]
  )
}}
";

        #endregion

        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private class FeaturePagination : Common.Test.Common.Pagination<Feature>
        {
            public IReadOnlyCollection<Feature> Features { get; set; }
        }

        private class LicensedFeatureWithFeatures : LicensedFeature
        {
            public FeaturePagination Features { get; set; }
        }

        private readonly TestsFixture _fixture;

        public LicensedFeaturesTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Query Tests

        [Fact, Order(0)]
        public async Task Get_LicensedFeature_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes(), 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var features = await GetFeaturesDictionary(new[] {node.Id});
            var nodeId = node.Id;
            var request = new GraphQLRequest
            {
                Query = LicensedFeatureQuery,
                Variables = new
                {
                    licensedId = nodeId
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var jsonEle = response["licensedFeature"];
            var result = jsonEle.ToObject<LicensedFeatureWithFeatures>();

            // Assert
            Assert.Equal(nodeId.ToString(), result.Id.ToString());
            Assert.Equal(node.Name, result.Name.ToString());
            Assert.Equal(node.Description, result.Description.ToString());
            Assert.Equal(node.Enabled.ToString(), result.IsEnabled.ToString());
            Assert.Equal(result.Features.Features.Count, features[nodeId].Count);
            Assert.All(result.Features.Features, r => features[nodeId].Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_LicensedFeature_BusinessAccountFiltered_Test()
        {
            // Arrange
            const int size = 10;
            var tenants = _fixture.BloomApiPrincipal[Graph.Subject0].GetTenants();
            var tenantIds = tenants.Select(p => Guid.Parse(p)).ToList().AsReadOnly();
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes
                    {
                        TenantIds = tenantIds
                    }, 0, size);
            var node = nodes.Data.ToArray()[Random.Next(0, nodes.Data.ToArray().Length - 1)];
            var features = await GetFeaturesDictionary(new[] {node.Id}, tenantIds);
            var nodeId = node.Id;
            var request = new GraphQLRequest
            {
                Query = LicensedFeatureWithFilteredBusinessAccountQuery,
                Variables = new
                {
                    licensedId = nodeId,
                    businessAccountIds = tenantIds
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));
            var jsonEle = response["licensedFeature"];
            var result = jsonEle.ToObject<LicensedFeatureWithFeatures>();

            // Assert
            Assert.Equal(nodeId.ToString(), result.Id.ToString());
            Assert.Equal(node.Name, result.Name.ToString());
            Assert.Equal(node.Description, result.Description.ToString());
            Assert.Equal(node.Enabled.ToString(), result.IsEnabled.ToString());
            Assert.Equal(result.Features.Features.Count, features[nodeId].Count);
            Assert.All(result.Features.Features, r => features[nodeId].Any(p => p.Id == r.Id));
        }

        [Fact, Order(0)]
        public async Task Get_NonExistent_LicensedFeature_Test()
        {
            // Arrange
            var request = new GraphQLRequest
            {
                Query = LicensedFeatureQuery,
                Variables = new
                {
                    licensedId = Guid.NewGuid()
                }
            };

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request, true);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal("Resource not found.", errors[0].Message);
        }

        [Fact, Order(0)]
        public async Task Get_LicensedFeatures_Test()
        {
            // Arrange
            const int size = 10;
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes(), 0, size);
            var featuresDictionary = await GetFeaturesDictionary(nodes.Data.Select(p => p.Id));
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesQueryWithSize,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeatureWithFeatures>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
            foreach (var f in result)
            {
                Assert.Equal(f.Features.Features.Count, featuresDictionary[f.Id].Count);
                Assert.All(f.Features.Features, r => featuresDictionary[f.Id].Any(p => p.Id == r.Id));
            }
        }

        [Fact, Order(0)]
        public async Task Get_LicensedFeatures_DoesNotReturnDisabled()
        {
            // Arrange
            const int size = 10;
            var cypher = _fixture.OngDB.GraphClient.Cypher.Match("(n:LicensedFeature)")
                .ReturnDistinct<LicensedFeature>("n");
            var nodes = (await cypher.ResultsAsync).ToList();
            var featuresDictionary = await GetFeaturesDictionary(nodes.Select(p => p.Id));
            var nodesCount = nodes.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesQueryWithSize,
                Variables = new
                {
                    offset = 0,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeatureWithFeatures>>();

            Assert.NotEqual(nodesCount, result.Count);
            Assert.True(result.All(lf => lf.IsEnabled));
        }

        [Fact, Order(0)]
        public async Task Get_LicensedFeatures_BusinessAccountFiltered_Test()
        {
            // Arrange
            const int size = 10;
            var tenants = _fixture.BloomApiPrincipal[Graph.Subject1].GetTenants();
            var tenantIds = new[] {Guid.Parse(tenants.ElementAt(Random.Next(tenants.Count)))};
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes
                    {
                        TenantIds = tenantIds
                    }, 0, size);
            var featuresDictionary = await GetFeaturesDictionary(nodes.Data.Select(p => p.Id), tenantIds);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesBusinessAccountFilteredQueryWithSize,
                Variables = new
                {
                    offset = 0,
                    limit = size,
                    businessAccountIds = tenantIds
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeatureWithFeatures>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
            foreach (var f in result)
            {
                Assert.Equal(f.Features.Features.Count, featuresDictionary[f.Id].Count);
                Assert.All(f.Features.Features, r => featuresDictionary[f.Id].Any(p => p.Id == r.Id));
            }
        }

        [Theory, Order(0)]
        [InlineData(1, 4)]
        [InlineData(2, 3)]
        public async Task Get_LicensedFeatures_With_Paging_Test(int size, int page)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes(), page, size);
            var featuresDictionary = await GetFeaturesDictionary(nodes.Data.Select(p => p.Id));
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesQueryWithSize,
                Variables = new
                {
                    offset = page,
                    limit = size
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeatureWithFeatures>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
            foreach (var f in result)
            {
                Assert.Equal(f.Features.Features.Count, featuresDictionary[f.Id].Count);
                Assert.All(f.Features.Features, r => featuresDictionary[f.Id].Any(p => p.Id == r.Id));
            }
        }

        [Theory, Order(0)]
        [InlineData("3")]
        [InlineData("feature")]
        [InlineData("theresNoMatchForThisParam")]
        public async Task Get_LicensedFeatures_With_Search(string search)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes
                    {
                        Search = search
                    }, 0, 10);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesWithSearch,
                Variables = new
                {
                    search = search
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeature>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
        }
        
        [Theory, Order(0)]
        [InlineData(PolicyTypeInput.Agency)]
        [InlineData(PolicyTypeInput.Identity)]
        [InlineData(PolicyTypeInput.Publisher)]
        [InlineData(PolicyTypeInput.DataProvider)]
        public async Task Get_LicensedFeatures_With_PolicyTypes(PolicyTypeInput policyInput)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes
                    {
                        PolicyTypes = new []{policyInput.ToString()}
                    }, 0, 10);
            var nodesCount = nodes.Data.Count;
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesWithPolicyTypes,
                Variables = new
                {
                    policyTypes = new []{policyInput.ToString().ToLowerFirstCharacter()}
                }
            };

            // Act
            var response = (JObject) (await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request));

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeature>>();

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
        }

        [Theory]
        [Order(0)]
        [InlineData(nameof(LicensedFeature.Id), SortingOrder.Ascending)]
        [InlineData(nameof(LicensedFeature.Id), SortingOrder.Descending)]
        [InlineData(nameof(LicensedFeature.Name), SortingOrder.Ascending)]
        [InlineData(nameof(LicensedFeature.Name), SortingOrder.Descending)]
        [InlineData("id", SortingOrder.Ascending)]
        [InlineData("id", SortingOrder.Descending)]
        [InlineData("name", SortingOrder.Ascending)]
        [InlineData("name", SortingOrder.Descending)]
        public async Task Get_LicensedFeatures_With_Sort(string fieldName, SortingOrder sortingOrder)
        {
            // Arrange
            var nodes = await _fixture.OngDB.VisibilityRepositoriesContainer
                .Get<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature>()
                .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                    new QueryParamsTenantIdsAndPolicyTypes
                    {
                        SortingOrder = sortingOrder,
                        OrderBy = fieldName.ToPascalCase()
                    }, 0, 10);
            var nodesCount = nodes.Data.Count;

            
            var request = new GraphQLRequest
            {
                Query = LicensedFeaturesWithSort,
                Variables = new
                {
                    fieldName = fieldName,
                    order = sortingOrder == 0 ? "asc" : "desc"
                }
            };

            // Act
            var response = (JObject) await _fixture.SendGraphqlRequestAsync(Graph.Subject0, request);

            // Assert
            var jsonEle = response["licensedFeatures"]["licensedFeatures"];
            var result = jsonEle.ToObject<IReadOnlyCollection<LicensedFeature>>();

            Assert.True(
                result.Select(lf => typeof(LicensedFeature).GetProperty(fieldName.ToPascalCase()).GetValue(lf))
                    .SequenceEqual(nodes.Data.Select(lf =>
                        typeof(Contracts.Output.LicensedFeature).GetProperty(fieldName.ToPascalCase())
                            .GetValue(lf, null))));

            Assert.Equal(nodesCount, result.Count);
            Assert.All(result, r => nodes.Data.Any(p => p.Id == r.Id));
            Assert.All(result, r => nodes.Data.Any(p => p.Name == r.Name));
            Assert.All(result, r => nodes.Data.Any(p => p.Description == r.Description));
            Assert.All(result, r => nodes.Data.Any(p => p.Enabled == r.IsEnabled));
        }

        #endregion



        private async Task<Dictionary<Guid, IReadOnlyCollection<Contracts.Output.Feature>>> GetFeaturesDictionary(
            IEnumerable<Guid> ids, IReadOnlyCollection<Guid> tenantIds = null)
        {
            var featuresDictionary = new Dictionary<Guid, IReadOnlyCollection<Contracts.Output.Feature>>();
            foreach (var id in ids)
            {
                var features = await _fixture.OngDB.VisibilityRepositoriesContainer
                    .Get<QueryParamsTenantIds, Contracts.Output.Feature>()
                    .EvaluateVisibilityAsync(_fixture.BloomApiPrincipal[Graph.Subject0],
                        new QueryParamsTenantIds
                        {
                            ContextId = id,
                            TenantIds = tenantIds,
                        }, 0, 100);
                featuresDictionary.Add(id, features.Data);
            }

            return featuresDictionary;
        }
    }
}