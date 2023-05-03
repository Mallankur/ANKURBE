using System;
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
    public class RoleVisibilityProviderTests
    {
        private readonly TestsFixture _fixture;

        public RoleVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_Role_InHierarchy(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, QueryParams queryParams)
        {
            // Arrange
            IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role> engine =
                new RoleVisibilityProvider(_fixture.GraphClient);

            // Act
            var tasks = roles.Select(x => engine.HasItemVisibilityAsync(identity, x.Id));
            var result = await Task.WhenAll(tasks);

            // Assert
            Assert.True(result.All(x => x));
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessibleRoles(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, QueryParams queryParams)
        {
            // Arrange
            IVisibilityProvider<QueryParamsRoles, Contracts.Output.Role> engine =
                new RoleVisibilityProvider(_fixture.GraphClient);
            var allResources = roles.Select(r => r.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsRoles
            {
                ResourceIds = roles.Select(r => r.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(allResources);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_Roles_InHierarchy(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, QueryParams queryParams)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsRoles
            {
                ResourceIds = roles.Select(r => r.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_Roles_InHierarchy(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, QueryParamsRoles filter)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);
            var sortedRoles = roles.OrderBy(Priority).ThenBy(OrderBy).Select(r => r.Name);

            // Act
            filter.TenantIds = tenantIds;
            var response = await engine.EvaluateVisibilityAsync(identity, filter, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(sortedRoles, response.Data.Select(r => r.Name));
            Assert.Equal(roles.Length, response.TotalItems);

            object Priority(Role x)
            {
                var prioritizeTemplateRoles = (filter as QueryParamsRoles)?.PrioritizeTemplateRoles;
                return (filter as QueryParamsRoles)?.PrioritizeTemplateRoles != null &&
                       (bool) prioritizeTemplateRoles
                    ? ((Graph.RoleWithType) x).Type
                    : true;
            }

            object OrderBy(Role x) => filter?.OrderBy != null && filter.OrderBy != "TenantName"
                ? typeof(Role).GetProperty(filter.OrderBy!)!.GetValue(x, null)
                : true;
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Roles_InHierarchy_Including_Incorrect_One(string caseName, 
            ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, QueryParams filter)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity,
                new QueryParamsRoles
                {
                    ResourceIds = roles.Select(f => f.Id).Concat(new[] {Guid.NewGuid()}).ToList(),
                    TenantIds = tenantIds
                });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_On_Inaccessible_Roles_InHierarchy(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);


            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsRoles
            {
                ResourceIds = roles.Select(r => r.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_OnlyAccessibleRoles(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, Guid[] accessibleRoles)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);


            // Act
            var response = await engine.GetVisibleResourcesAsync(identity,
                new QueryParamsRoles
                {
                    ResourceIds = roles.Select(r => r.Id).ToArray(),
                    TenantIds = tenantIds
                });

            // Assert
            response.Should().BeEquivalentTo(accessibleRoles);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_Roles_InHierarchy(
            string caseName, ClaimsPrincipal identity, Role[] roles, Guid[] tenantIds, Guid[] unused)
        {
            // Arrange
            var engine = new RoleVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsRoles
            {
                TenantIds = tenantIds
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(roles.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, Role[], Guid[], QueryParamsRoles> CreateResult()
        {
            var roles = Graph.GetRoles();
            var data = new TheoryData<string, ClaimsPrincipal, Role[], Guid[], QueryParamsRoles>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    roles[..21].Concat(new[] {roles[26]}).ToArray(),
                    null,
                    new QueryParamsRoles
                    {
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.OtherRole},
                        TenantId = Guid.Parse(Graph.Tenant0)
                    }),
                    new[]
                    {
                        roles[0], roles[1], roles[2], roles[3],
                        roles[4], roles[5], roles[26]
                    },
                    null,
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = false,
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.OtherRole},
                        TenantId = Guid.Parse(Graph.Tenant0)
                    }),
                    new[]
                    {
                        roles[0], roles[1], roles[2], roles[3],
                        roles[4], roles[5], roles[26]
                    },
                    null,
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = false,
                        OrderBy = "Name"
                    }
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.OtherRole},
                        TenantId = Guid.Parse(Graph.Tenant0)
                    }),
                    new[]
                    {
                        roles[0], roles[1], roles[2], roles[3],
                        roles[4], roles[5], roles[26]
                    },
                    null,
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = true,
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.OtherRole},
                        TenantId = Guid.Parse(Graph.Tenant0)
                    }),
                    new[]
                    {
                        roles[5]
                    },
                    null,
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = true,
                        OrderBy = "Id",
                        Search = "5"
                    }
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    new[]
                    {
                        roles[9], roles[11]
                    },
                    new[] {Guid.Parse(Graph.Tenant8)},
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = false,
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 6",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                    }),
                    new[]
                    {
                        roles[1], roles[2], roles[3], roles[7], roles[26]
                    },
                    null,
                    new QueryParamsRoles
                    {
                        PrioritizeTemplateRoles = true,
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant3Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[1], roles[2], roles[3], roles[17], roles[26]},
                    null,
                    new QueryParamsRoles
                    {
                        OrderBy = "Id"
                    }
                },
                {
                    "Case 8",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant3Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {roles[1], roles[2], roles[3], roles[7], roles[17], roles[26]},
                    null,
                    new QueryParamsRoles
                    {
                        OrderBy = "Id"
                    }
                }
            };

            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Role[], Guid[], Guid[]> CreateFailureResult()
        {
            var roles = Graph.GetRoles();
            var data = new TheoryData<string, ClaimsPrincipal, Role[], Guid[], Guid[]>
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
                    new[] {roles[4], roles[5], roles[6], roles[9]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant3Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[5], roles[6], roles[9]},
                    null,
                    new Guid[] { roles[6].Id }
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[7], roles[9]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[4], roles[5], roles[9]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6), TenantName = Graph.Tenant6Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[5], roles[6], roles[9]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7), TenantName = Graph.Tenant7Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[5], roles[6], roles[7], roles[9]},
                    null,
                    new Guid[] { roles[7].Id}
                },
                {
                    "Case 6",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[0], roles[9], roles[6]},
                    new[] {Guid.Parse(Graph.Tenant6)},
                    new Guid[] { }
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant6Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        }
                    }),
                    roles,
                    new[] {Guid.Parse(Graph.Tenant3), Guid.Parse(Graph.Tenant6)},
                    new Guid[] {roles[1].Id, roles[2].Id, roles[3].Id, roles[17].Id, roles[26].Id}
                },
                {
                    "Case 8",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant0),
                        TenantName = Graph.Tenant0Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[14], roles[15]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 9",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {roles[9]},
                    null,
                    new Guid[] { }
                },
                {
                    "Case 10",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    roles,
                    null,
                    roles[0..21].Concat(new []{roles[26]}).Select(p=>p.Id).ToArray()
                }
            };

            return data;
        }

        #endregion
    }
}