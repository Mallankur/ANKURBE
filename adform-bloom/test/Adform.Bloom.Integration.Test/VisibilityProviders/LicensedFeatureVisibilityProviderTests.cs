using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Input;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.VisibilityProviders
{
    [Collection(nameof(VisibilityProvidersCollection))]
    [Order(1)]
    public class LicensedFeatureVisibilityProviderTests
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public LicensedFeatureVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResultFilter))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_LicensedFeature_InHierarchy(string caseName, 
            ClaimsPrincipal identity, LicensedFeature[] features, Guid[] tenantIds)
        {
            // Arrange
            IVisibilityProvider<QueryParamsTenantIdsAndPolicyTypes, Contracts.Output.LicensedFeature> engine =
                new LicensedFeatureVisibilityProvider(_fixture.GraphClient);
            var index = Random.Next(features.Length);
            var feature = features.ToList()[index];

            // Act
            var response = await engine.HasItemVisibilityAsync(identity, feature.Id);

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_LicensedFeatures_InHierarchy(string caseName, ClaimsPrincipal identity,
            LicensedFeature[] licensedFeatures, IReadOnlyCollection<Guid> tenantIds, IReadOnlyCollection<string> policyNames)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                ResourceIds = licensedFeatures.Select(l => l.Id).ToArray(),
                PolicyTypes = policyNames,
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessibleFeatures(string caseName, ClaimsPrincipal identity,
            LicensedFeature[] licensedFeatures, IReadOnlyCollection<Guid> tenantIds, IReadOnlyCollection<string> policyNames)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);
            var allResources = licensedFeatures.Select(l => l.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                ResourceIds = allResources,
                PolicyTypes = policyNames,
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(allResources);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_LicensedFeatures_InHierarchy(string caseName, ClaimsPrincipal identity,
            LicensedFeature[] licensedFeatures, IReadOnlyCollection<Guid> tenantIds, IReadOnlyCollection<string> policyNames)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                TenantIds = tenantIds,
                PolicyTypes = policyNames,
                OrderBy = nameof(LicensedFeature.Name)
            }, 0, 10);
            var expectedResult =
                licensedFeatures.OrderBy(o => o.Name).Select(o => o.Name).ToList();
            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResult, response.Data.Select(o => o.Name).ToList());
            Assert.Equal(licensedFeatures.Length, response.TotalItems);
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Features_InHierarchy_Including_Incorrect_One(string caseName, 
            ClaimsPrincipal identity, LicensedFeature[] licensedFeatures, Guid[] tenantIds, IReadOnlyCollection<string> policyNames)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                ResourceIds = licensedFeatures.Select(f => f.Id).Concat(new[] { Guid.NewGuid() }).ToList(),
                PolicyTypes = policyNames,
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_On_Inaccessible_LicensedFeatures_InHierarchy(string caseName, 
            ClaimsPrincipal identity,
            LicensedFeature[] licensedFeatures, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                ResourceIds = licensedFeatures.Select(f => f.Id).ToList(),
                TenantIds = tenantIds
            });

            // Assert

            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_LicensedFeatures_InHierarchy(string caseName, 
            ClaimsPrincipal identity, LicensedFeature[] licensedFeatures, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                TenantIds = tenantIds
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(licensedFeatures.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_OnlyAccessibleFeatures(string caseName, 
            ClaimsPrincipal identity, LicensedFeature[] licensedFeatures, Guid[] tenantIds, Guid[] accessibleFeatures)
        {
            // Arrange
            var engine = new LicensedFeatureVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIdsAndPolicyTypes
            {
                TenantIds = tenantIds,
                ResourceIds = licensedFeatures.Select(l => l.Id).ToArray()
            });

            // Assert
            response.Should().BeEquivalentTo(accessibleFeatures);
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, LicensedFeature[], Guid[]> CreateResultFilter()
        {
            var successData = CreateResult();
            var data = new TheoryData<string, ClaimsPrincipal, LicensedFeature[], Guid[]>();
            foreach (dynamic d in successData)
            {
                var licensedFeatures = (LicensedFeature[])d[2];
                if (licensedFeatures != null && licensedFeatures.Any())
                    data.Add(d[0], d[1], d[2], d[3]);
            }

            return data;
        }

        public static TheoryData<string,ClaimsPrincipal, LicensedFeature[], Guid[], string[]> CreateResult()
        {
            var licensedFeatures = Graph.GetLicensedFeatures();
            var data = new TheoryData<string, ClaimsPrincipal, LicensedFeature[], Guid[], string[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    licensedFeatures.Where(l => l.IsEnabled).ToArray(),
                    null,
                    null
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    new []{licensedFeatures[0],licensedFeatures[2]},
                    null,
                    new []{PolicyTypeInput.Agency.ToString()}
                },
                { 
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {licensedFeatures[2]},
                    null,
                    null
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4),
                        TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new LicensedFeature[] { },
                    null,
                    null
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6),
                        TenantName = Graph.Tenant6Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new LicensedFeature[] { },
                    null,
                    null
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant8),
                        TenantName = Graph.Tenant8Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new LicensedFeature[] { licensedFeatures[2]},
                    null,
                    null
                },
                { 
                    "Case 6",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant0),
                            TenantName = Graph.Tenant0Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {licensedFeatures[2]},
                    new[] {Guid.Parse(Graph.Tenant1)},
                    null
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant6),
                            TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new LicensedFeature[] { },
                    new[] {Guid.Parse(Graph.Tenant6)},
                    null
                },
                {
                    "Case 8",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant6),
                            TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {licensedFeatures[2]},
                    new[] {Guid.Parse(Graph.Tenant1), Guid.Parse(Graph.Tenant6)},
                    null
                }
            };

            return data;
        }

        public static TheoryData<string,ClaimsPrincipal, LicensedFeature[], Guid[], Guid[]> CreateFailureResult()
        {
            var licensedFeatures = Graph.GetLicensedFeatures();
            var data = new TheoryData<string,ClaimsPrincipal, LicensedFeature[], Guid[], Guid[]>
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
                    new[] {licensedFeatures[1]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant8),
                        TenantName = Graph.Tenant8Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {licensedFeatures[0], licensedFeatures[1]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant0),
                        TenantName = Graph.Tenant0Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {licensedFeatures[0], licensedFeatures[1], licensedFeatures[2], licensedFeatures[3]},
                    null,
                    new Guid[] {licensedFeatures[0].Id, licensedFeatures[1].Id, licensedFeatures[2].Id }
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                        Permissions = new[] {""}
                    }),
                    new[] {licensedFeatures[0], licensedFeatures[1], licensedFeatures[2], licensedFeatures[3]},
                    new[] {Guid.Parse(Graph.Tenant0)},
                    new Guid[] {licensedFeatures[0].Id, licensedFeatures[1].Id, licensedFeatures[2].Id }
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                    }),
                    new[] {licensedFeatures[0], licensedFeatures[1], licensedFeatures[2], licensedFeatures[3]},
                    null,
                    new Guid[] { licensedFeatures[0].Id, licensedFeatures[1].Id, licensedFeatures[2].Id}
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant6),
                            TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {licensedFeatures[0], licensedFeatures[1], licensedFeatures[2]},
                    new[] {Guid.Parse(Graph.Tenant1)},
                    new Guid[] { licensedFeatures[2].Id}
                },
            };
            return data;
        }

        #endregion
    }
}