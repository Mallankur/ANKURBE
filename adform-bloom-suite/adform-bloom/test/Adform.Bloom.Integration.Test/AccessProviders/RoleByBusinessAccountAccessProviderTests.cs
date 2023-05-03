using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.DataAccess.Providers.Access;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Integration.Test.VisibilityProviders;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.OngDb.Repository;
using Xunit;
using Xunit.Extensions.Ordering;
using Role = Adform.Bloom.Domain.Entities.Role;

namespace Adform.Bloom.Integration.Test.AccessProviders
{
    [Collection(nameof(AcessProvidersCollection))]
    [Order(1)]
    public class RoleByBusinessAccountAccessProviderTests
    {
        private readonly TestsFixture _fixture;

        public RoleByBusinessAccountAccessProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateAccessAsync_Succeeds_On_Roles_InHierarchy(string caseName,
            ClaimsPrincipal identity, BusinessAccount context, QueryParamsTenantIds queryParams, Role[] roles)
        {
            // Arrange
            var engine = new RoleByBusinessAccountAccessProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(
                roles.OrderBy(OrderBy).Select(r => r.Name),
                response.Data.Select(r => r.Name));
            Assert.Equal(roles.Length, response.TotalItems);

            object OrderBy(Role x) => queryParams?.OrderBy != null
                ? typeof(Role).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                : true;
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateAccessAsync_Fails_On_Inaccessible_Roles_InHierarchy(
            ClaimsPrincipal identity, BusinessAccount context, QueryParamsTenantIds queryParams, Role[] roles)
        {
            // Arrange
            var engine = new RoleByBusinessAccountAccessProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.False(roles.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, Role[]> CreateResult()
        {
            var roles = GetRoles();
            var data = new TheoryData<string, ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, Role[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant0),
                        Name = Graph.Tenant0Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    roles[0..6].Concat(roles[25..]).ToArray()
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[6]}
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant3Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant3),
                        Name = Graph.Tenant3Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[17]}
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant4),
                        Name = Graph.Tenant4Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[13], roles[16]}
                },
                {
                    "Case 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant5),
                        Name = Graph.Tenant5Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[24]}
                },
                {
                    "Case 5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6), TenantName = Graph.Tenant6Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant6),
                        Name = Graph.Tenant6Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[21]}
                },
                {
                    "Case 6",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant3Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        },

                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        }
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant3),
                        Name = Graph.Tenant3Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[17]}
                },
                {
                    "Case 7",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant4),
                        Name = Graph.Tenant4Name
                    },
                    new QueryParamsTenantIds
                    {
                        Search = "13",
                        OrderBy = "Id"
                    },
                    new[] {roles[13]}
                },
                {
                    "Case 8",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant4),
                        Name = Graph.Tenant4Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Name",
                        SortingOrder = SortingOrder.Ascending
                    },
                    new[] {roles[13], roles[16]}.OrderBy(x => x.Name).ToArray()
                }
            };
            return data;
        }

        public static TheoryData<ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, Role[]> CreateFailureResult()
        {
            var roles = GetRoles();
            var data = new TheoryData<ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, Role[]>
            {
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[4], roles[5], roles[6], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[5], roles[6], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant4),
                        Name = Graph.Tenant4Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[7], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant5),
                        Name = Graph.Tenant5Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[4], roles[5], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant6), TenantName = Graph.Tenant6Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant6),
                        Name = Graph.Tenant6Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[5], roles[6], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7), TenantName = Graph.Tenant7Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant7),
                        Name = Graph.Tenant7Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[5], roles[6], roles[7], roles[9]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant5),
                        Name = Graph.Tenant5Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[0], roles[9], roles[6]}
                },
                {
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant3), TenantName = Graph.Tenant3Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        }
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant4),
                        Name = Graph.Tenant4Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[0], roles[9], roles[4], roles[5]}
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant0),
                        TenantName = Graph.Tenant0Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant0),
                        Name = Graph.Tenant0Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {roles[14], roles[15]}
                }
            };

            return data;
        }

        public static Role[] GetRoles()
        {
            var roles = new[]
            {
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole0),
                    Name = Graph.CustomRole0Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.Role1),
                    Name = Graph.Role1Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.Role2),
                    Name = Graph.Role2Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.Role3),
                    Name = Graph.Role3Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole4),
                    Name = Graph.CustomRole4Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole5),
                    Name = Graph.CustomRole5Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole6),
                    Name = Graph.CustomRole6Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole7),
                    Name = Graph.CustomRole7Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole8),
                    Name = Graph.CustomRole8Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole9),
                    Name = Graph.CustomRole9Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole10),
                    Name = Graph.CustomRole10Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole11),
                    Name = Graph.CustomRole11Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole12),
                    Name = Graph.CustomRole12Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole13),
                    Name = Graph.CustomRole13Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole14),
                    Name = Graph.CustomRole14Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole15),
                    Name = Graph.CustomRole15Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole16),
                    Name = Graph.CustomRole16Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole17),
                    Name = Graph.CustomRole17Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole18),
                    Name = Graph.CustomRole18Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole19),
                    Name = Graph.CustomRole19Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.CustomRole20),
                    Name = Graph.CustomRole20Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.TransitionalRole21),
                    Name = Graph.TransitionalRole21Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.TransitionalRole22),
                    Name = Graph.TransitionalRole22Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.TransitionalRole23),
                    Name = Graph.TransitionalRole23Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.TransitionalRole24),
                    Name = Graph.TransitionalRole24Name
                },
                new Role
                {
                    Id = Guid.Parse(Graph.LocalAdmin),
                    Name = Graph.LocalAdminRoleName
                },
                new Role
                {
                    Id = Guid.Parse(Graph.AdformAdmin),
                    Name = Graph.AdformAdminRoleName
                }

            };
            return roles;
        }

        #endregion
    }
}