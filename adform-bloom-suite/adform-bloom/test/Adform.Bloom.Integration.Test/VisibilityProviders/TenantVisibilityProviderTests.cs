using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.OngDb.Repository;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.VisibilityProviders
{
    [Collection(nameof(VisibilityProvidersCollection))]
    [Order(1)]
    public class TenantVisibilityProviderTests
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public TenantVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_Tenant_InHierarchy(
            ClaimsPrincipal identity, Tenant[] features, Guid[] tenantIds)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);
            var index = Random.Next(features.Length);
            var feature = features.ToList()[index];

            // Act
            var response = await engine.HasItemVisibilityAsync(identity, feature.Id);

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_Tenants_InHierarchy(ClaimsPrincipal identity,
            Tenant[] tenants, Guid[] tenantIds)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsBusinessAccount
            {
                ResourceIds = tenants.Select(t => t.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessibleTenants(ClaimsPrincipal identity,
            Tenant[] tenants, Guid[] tenantIds)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);
            var allTenants = tenants.Select(t => t.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsBusinessAccount
            {
                ResourceIds = allTenants,
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(allTenants);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_Tenants_InHierarchy(ClaimsPrincipal identity,
            Tenant[] tenants, Guid[] tenantIds)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsBusinessAccount
            {
                OrderBy = "Name",
                SortingOrder = SortingOrder.Descending,
                TenantIds = tenantIds
            }, 1, 2);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(
                tenants.OrderByDescending(o => o.Name).Select(o => o.Name).Skip(1).Take(2).ToList(),
                response.Data.Select(o => o.Name).ToList());
            Assert.Equal(tenants.Length, response.TotalItems);
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Tenants_InHierarchy_Including_Incorrect_One(
            ClaimsPrincipal identity, Tenant[] tenants, Guid[] tenantIds)
        {
            if (identity.IsAdformAdmin())
                return;

            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsBusinessAccount
            {
                ResourceIds = tenants.Select(f => f.Id).Concat(new[] {Guid.NewGuid()}).ToList(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_On_Inaccessible_Tenants_InHierarchy(ClaimsPrincipal identity,
            Tenant[] tenants, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsBusinessAccount
            {
                ResourceIds = tenants.Select(t => t.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_OnlyAccessibleTenants(ClaimsPrincipal identity,
            Tenant[] tenants, Guid[] tenantIds, Guid[] accessibleTenants)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsBusinessAccount
            {
                ResourceIds = tenantIds,
                TenantIds = tenants.Select(t => t.Id).ToArray(),
            });

            // Assert
            response.Should().BeEquivalentTo(accessibleTenants);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_Tenants_InHierarchy(
            ClaimsPrincipal identity, Tenant[] tenants, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new TenantVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsBusinessAccount
            {
                TenantIds = tenantIds
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(tenants.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Generation

        public static TheoryData<ClaimsPrincipal, Tenant[], Guid[]> CreateResult()
        {
            var tenants = GetTenants();
            var data = new TheoryData<ClaimsPrincipal, Tenant[], Guid[]>
            {
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {ClaimPrincipalExtensions.AdformAdmin},
                    }),
                    tenants,
                    null
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[1]},
                    null
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[2]},
                    null
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant3),
                        TenantName = Graph.Tenant3Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[3]},
                    null
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant8),
                        TenantName = Graph.Tenant8Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[8]},
                    null
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[1]},
                    new[] {Guid.Parse(Graph.Tenant1)}
                },
                {
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
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {tenants[1], tenants[2]},
                    new[] {Guid.Parse(Graph.Tenant1), Guid.Parse(Graph.Tenant2)}
                },
            };

            return data;
        }

        public static TheoryData<ClaimsPrincipal, Tenant[], Guid[], Guid[]> CreateFailureResult()
        {
            var tenants = GetTenants();
            var data = new TheoryData<ClaimsPrincipal, Tenant[], Guid[], Guid[]>
            {
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.AdformAdminRoleName},
                        Permissions = new[] {""}
                    }),
                    new[]
                    {
                        new Tenant
                        {
                            Id = Guid.NewGuid(),
                            Name = "non-existent tenant"
                        },
                    },
                    null,
                    new Guid[] { }
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[2]},
                    null,
                    new Guid[] { }
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[0]},
                    null,
                    new Guid[] { }
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[0], tenants[1]},
                    null,
                    new Guid[] { }
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {tenants[1]},
                    new[] {Guid.Parse(Graph.Tenant2)},
                    new Guid[] { }
                },
                {
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
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {tenants[1], tenants[2]},
                    new[] {Guid.Parse(Graph.Tenant1), Guid.Parse(Graph.Tenant3)},
                    new Guid[] {tenants[1].Id}
                },
            };
            return data;
        }

        private static Tenant[] GetTenants()
        {
            return new[]
            {
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant0),
                    Name = Graph.Tenant0Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant1),
                    Name = Graph.Tenant1Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant2),
                    Name = Graph.Tenant2Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant3),
                    Name = Graph.Tenant3Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant4),
                    Name = Graph.Tenant4Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant5),
                    Name = Graph.Tenant5Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant6),
                    Name = Graph.Tenant6Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant7),
                    Name = Graph.Tenant7Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant8),
                    Name = Graph.Tenant8Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant9),
                    Name = Graph.Tenant9Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant10),
                    Name = Graph.Tenant10Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant11),
                    Name = Graph.Tenant11Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant12),
                    Name = Graph.Tenant12Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant13),
                    Name = Graph.Tenant13Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant14),
                    Name = Graph.Tenant14Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant15),
                    Name = Graph.Tenant15Name
                },
                new Tenant
                {
                    Id = Guid.Parse(Graph.Tenant16),
                    Name = Graph.Tenant16Name
                },
            };
        }

        #endregion
    }
}