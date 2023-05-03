using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Runtime.Contracts.Response;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.VisibilityProviders
{
    [Collection(nameof(VisibilityProvidersCollection))]
    [Order(1)]
    public class FeatureVisibilityProviderTests
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public FeatureVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResultFilter))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_Feature_InHierarchy(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            // Arrange
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Feature> engine =
                new FeatureVisibilityProvider(_fixture.GraphClient);
            var index = Random.Next(features.Length);
            var feature = features.ToList()[index];

            // Act
            var response = await engine.HasItemVisibilityAsync(identity, feature.Id, null);

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_Features_InHierarchy(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = features.Select(f => f.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Succeeds_On_Features_InHierarchy(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);
            var featureIds = features.Select(f => f.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = featureIds,
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(featureIds);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_Features_InHierarchy(string caseName,
            ClaimsPrincipal identity,
            Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ContextId = contextId,
                TenantIds = tenantIds
            }, 0, 10);
            var expectedResult = features.OrderBy(o => o.Id).Select(o => o.Name).ToList();

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResult, response.Data.Select(o => o.Name).ToList());
            Assert.Equal(features.Length, response.TotalItems);
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Features_InHierarchy_Including_Incorrect_One(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid? contextId)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = features.Select(f => f.Id).Concat(new[] {Guid.NewGuid()}).ToList(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_On_Inaccessible_Features_InHierarchy(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = features.Select(f => f.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessibleFeatures(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid[] accessibleFeatures)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = features.Select(f => f.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(accessibleFeatures);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_LicensedFeatures_InHierarchy(
            string caseName, ClaimsPrincipal identity, Feature[] features, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new FeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                TenantIds = tenantIds
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(features.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid?> CreateResultFilter()
        {
            var successData = CreateResult();
            var data = new TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid?>();
            foreach (dynamic d in successData)
            {
                var features = (Feature[]) d[2];
                if (features != null && features.Any())
                    data.Add(d[0], d[1], d[2], d[3], d[4]);
            }

            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid?> CreateResult()
        {
            var features = Graph.GetFeatures();
            var data = new TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid?>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    features.Where(f => f.IsEnabled).ToArray(),
                    null,
                    null
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {features[3], features[4]},
                    null,
                    null
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new Feature[] {features[5], features[6]},
                    null,
                    null
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new Feature[] { features[7]},
                    null,
                    null
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant8),
                        TenantName = Graph.Tenant8Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new Feature[] {features[3], features[4]},
                    null,
                    null
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant8),
                            TenantName = Graph.Tenant8Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {features[3], features[4]},
                    new[] {Guid.Parse(Graph.Tenant1)},
                    null
                },
                {
                    "Case 6",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant0),
                        TenantName = Graph.Tenant0Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {features[1], features[2]},
                    null,
                    Guid.Parse(Graph.LicensedFeature1)
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new Feature[] { },
                    null,
                    Guid.Parse(Graph.LicensedFeature1)
                }
            };

            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid[]> CreateFailureResult()
        {
            var features = Graph.GetFeatures();
            var data = new TheoryData<string, ClaimsPrincipal, Feature[], Guid[], Guid[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    features[..3].Concat(features[5..9]).ToArray(),
                    null,
                    new Guid[] { }
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant3),
                        TenantName = Graph.Tenant3Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    features[..5].Concat(features[7..9]).ToArray(),
                    null,
                    new Guid[] { }
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant14),
                        TenantName = Graph.Tenant14Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    features[..6].Concat(features[8..9]).ToArray(),
                    null,
                    new Guid[] { }
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant3),
                            TenantName = Graph.Tenant3Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant14),
                            TenantName = Graph.Tenant14Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    features[..5].Concat(features[8..9]).ToArray(),
                    new[] {Guid.Parse(Graph.Tenant3)},
                    Array.Empty<Guid>()
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                        }
                    }),
                    new[] {features[0], features[1], features[2], features[8]},
                    null,
                    new Guid[] {features[0].Id, features[1].Id, features[2].Id}
                }
            };
            return data;
        }

        #endregion
    }
}