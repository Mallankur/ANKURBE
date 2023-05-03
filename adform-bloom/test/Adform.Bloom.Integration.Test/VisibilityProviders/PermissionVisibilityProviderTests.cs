using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Interfaces;
using Adform.Bloom.DataAccess.Providers.Visibility;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using FluentAssertions;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Integration.Test.VisibilityProviders
{
    [Collection(nameof(VisibilityProvidersCollection))]
    [Order(1)]
    public class PermissionVisibilityProviderTests
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public PermissionVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_Permission_InHierarchy(string caseName, 
            ClaimsPrincipal identity, Permission[] permissions, Guid[] tenantIds, Guid[] permissionIds)
        {
            // Arrange
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Permission> engine =
                new PermissionVisibilityProvider(_fixture.GraphClient);
            var index = Random.Next(permissions.Length);
            var permission = permissions.ToList()[index];

            // Act
            var response = await engine.HasItemVisibilityAsync(identity, permission.Id);

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_Permissions_InHierarchy(string caseName, ClaimsPrincipal identity,
            IEnumerable<Permission> permissions, Guid[] tenantIds, Guid[] permissionIds)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = permissions.Select(p => p.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessiblePermissions(string caseName, ClaimsPrincipal identity,
            IEnumerable<Permission> permissions, Guid[] tenantIds, Guid[] permissionIds)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);
            var allResources = permissions.Select(p => p.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = allResources,
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(allResources);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_Permissions_InHierarchy(string caseName, 
            ClaimsPrincipal identity, Permission[] permissions, Guid[] tenantIds, Guid[] permissionIds)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                TenantIds = tenantIds,
                ResourceIds = permissionIds
            }, 0, 20);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(
                permissions.OrderBy(o => o.Id).Select(o => o.Name).ToList(),
                response.Data.Select(o => o.Name).ToList());
            Assert.Equal(permissions.Length, response.TotalItems);
        }

        #endregion

        #region Failure Scanarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Features_InHierarchy_Including_Incorrect_One(string caseName, 
            ClaimsPrincipal identity, Permission[] permissions, Guid[] tenantIds, Guid[] permissionIds)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = permissions.Select(f => f.Id).Concat(new[] {Guid.NewGuid()}).ToList(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_For_Inaccessible_Permissions_InHierarchy(string caseName, ClaimsPrincipal identity,
            Permission[] permissions, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = permissions.Select(s => s.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_OnlyAccessiblePermissions(string caseName, ClaimsPrincipal identity,
            Permission[] permissions, Guid[] tenantIds, Guid[] accessiblePermissions)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = permissions.Select(s => s.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(accessiblePermissions);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_Permissions_InHierarchy(string caseName, 
            ClaimsPrincipal identity, Permission[] permissions, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new PermissionVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                TenantIds = tenantIds
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(permissions.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Generation

        public static TheoryData<string, ClaimsPrincipal, Permission[], Guid[], Guid[]> CreateResult()
        {
            var permissions = GetPermissionsAssignedToFeatures();
            var data = new TheoryData<string, ClaimsPrincipal, Permission[], Guid[], Guid[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    permissions,
                    null,
                    null
                },
                {                    
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name, 
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[5], permissions[6],permissions[7], permissions[8]},
                    null,
                    null
                },
                {                    
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name, 
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[]
                    {
                        permissions[9], permissions[10], permissions[11], permissions[12]
                    },
                    null,
                    null
                },
                {                    
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, 
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[]
                    {
                        permissions[13], permissions[14], permissions[15], permissions[16]
                    },
                    null,
                    null
                },
                {                    
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant13), TenantName = Graph.Tenant13Name, 
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[13], permissions[14]},
                    null,
                    new[] {permissions[13].Id, permissions[14].Id}
                },
                {                    
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant11), TenantName = Graph.Tenant11Name, 
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[9],permissions[10]},
                    null,
                    new[] {permissions[9].Id,permissions[10].Id }
                },
                {                    
                    "Case 6",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant8), TenantName = Graph.Tenant8Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[5], permissions[6], permissions[7], permissions[8]},
                    null,
                    null
                },
                {                    
                    "Case 7",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant0), 
                        TenantName = Graph.Tenant0Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    permissions,
                    null,
                    null
                },
                {                    
                    "Case 8",
                    Common.Test.Common.BuildUser(new []
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant0), 
                            TenantName = Graph.Tenant0Name, 
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant8), 
                            TenantName = Graph.Tenant8Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {permissions[5], permissions[6], permissions[7], permissions[8]},
                    new[] {Guid.Parse(Graph.Tenant8)},
                    null
                }
            };

            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Permission[], Guid[], Guid[]> CreateFailureResult()
        {
            var permissions = GetPermissionsAssignedToFeatures();
            var data = new TheoryData<string, ClaimsPrincipal, Permission[], Guid[], Guid[]>
            {
                {                    
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[2], permissions[4], permissions[7]},
                    null,
                    new Guid[] {permissions[7].Id}
                },
                {                    
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[4]},
                    null,
                    new Guid[] { }
                },
                {                    
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[4]},
                    null,
                    new Guid[] { }
                },
                {                    
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6), TenantName = Graph.Tenant6Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[2], permissions[5], permissions[7]},
                    null,
                    new Guid[] {}
                },
                {                    
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7), TenantName = Graph.Tenant7Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[2], permissions[4], permissions[5]},
                    null,
                    new [] {permissions[5].Id}
                },
                {                    
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {permissions[0], permissions[1], permissions[3], permissions[6]},
                    new[] {Guid.Parse(Graph.Tenant7)},
                    new Guid[] { }
                },
                {                    
                    "Case 6",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant6), TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        }
                    }),
                    new[] {permissions[0], permissions[1], permissions[3], permissions[4], permissions[6]},
                    new[] {Guid.Parse(Graph.Tenant1)},
                    new Guid[] {permissions[6].Id}
                },
            };

            return data;
        }

        private static Permission[] GetPermissionsAssignedToFeatures()
        {
            var permissions = new[]
            {
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission0),
                    Name = Graph.Permission0Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission1),
                    Name = Graph.Permission1Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission2),
                    Name = Graph.Permission2Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission3),
                    Name = Graph.Permission3Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission4),
                    Name = Graph.Permission4Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission5),
                    Name = Graph.Permission5Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission6),
                    Name = Graph.Permission6Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission7),
                    Name = Graph.Permission7Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission8),
                    Name = Graph.Permission8Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission9),
                    Name = Graph.Permission9Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission10),
                    Name = Graph.Permission10Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission11),
                    Name = Graph.Permission11Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission12),
                    Name = Graph.Permission12Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission13),
                    Name = Graph.Permission13Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission14),
                    Name = Graph.Permission14Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission15),
                    Name = Graph.Permission15Name
                },
                new Permission
                {
                    Id = Guid.Parse(Graph.Permission16),
                    Name = Graph.Permission16Name
                }
            };
            return permissions;
        }

        #endregion
    }
}