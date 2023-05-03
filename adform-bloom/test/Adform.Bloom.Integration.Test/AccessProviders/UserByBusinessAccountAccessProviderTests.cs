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

namespace Adform.Bloom.Integration.Test.AccessProviders
{
    [Collection(nameof(AcessProvidersCollection))]
    [Order(1)]
    public class UserByBusinessAccountAccessProviderTests
    {
        private readonly TestsFixture _fixture;

        public UserByBusinessAccountAccessProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateAccessAsync_Succeeds_On_Roles_InHierarchy(string caseNumber,
            ClaimsPrincipal identity, BusinessAccount context, QueryParamsTenantIds queryParams, User[] users)
        {
            // Arrange
            var engine = new UserByBusinessAccountAccessProvider(_fixture.GraphClient, _fixture.UserReadModel);

            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(users.OrderBy(OrderBy).Select(OrderBy), response.Data.Select(OrderBy));
            Assert.All(users, u => response.Data.Select(x => x.Id).Contains(u.Id));

            object OrderBy(User x)
            {
                return queryParams?.OrderBy != null
                    ? typeof(User).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                    : true;
            }
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateAccessAsync_Fails_On_Inaccessible_Roles_InHierarchy(
            ClaimsPrincipal identity, BusinessAccount context, QueryParamsTenantIds queryParams, User[] users)
        {
            // Arrange
            var engine = new RoleByBusinessAccountAccessProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.False(users.All(u => response.Data.Any(x => x.Id == u.Id)));
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, User[]> CreateResult()
        {
            var users = GetUsers();
            var traffickers = GetTraffickers();
            var data = new TheoryData<string, ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, User[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName}
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
                    new[]
                    {
                        users[0], users[1]
                    }
                },
                {
                    "Case 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7), TenantName = Graph.Tenant7Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant7),
                        Name = Graph.Tenant1Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {users[4]}
                },
                {
                    "Case 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name,
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
                    Array.Empty<User>()
                },
                {
                    "Case 3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1), TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.AdformAdminRoleName},
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
                    new[] {traffickers[0]}
                },
                {
                    "Case 4",
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
                    new[] {users[2], users[3]}
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
                    new []{users[8]}
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
                            TenantId = Guid.Parse(Graph.Tenant11), TenantName = Graph.Tenant11Name,
                            Roles = new[] {Graph.OtherRole}, Permissions = new[] {""}
                        }
                    }),
                    new BusinessAccount
                    {
                        Id = Guid.Parse(Graph.Tenant11),
                        Name = Graph.Tenant3Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[] {users[7]}
                },
                {
                    "Case 7",
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
                        Search = "2",
                        OrderBy = "Id"
                    },
                    new[] {users[2], users[3]}
                },
                {
                    "Case 8",
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
                        OrderBy = "Name",
                        SortingOrder = SortingOrder.Ascending
                    },
                    new[] {users[3], users[2]}.OrderBy(x => x.Name).ToArray()
                }
            };
            return data;
        }

        public static TheoryData<ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, User[]> CreateFailureResult()
        {
            var users = GetUsers();
            var data = new TheoryData<ClaimsPrincipal, BusinessAccount, QueryParamsTenantIds, User[]>
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
                    new[] {users[2], users[3], users[6], users[8]}
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
                    new[] {users[0], users[2], users[3], users[8]}
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
                    new[] {users[0], users[3]}
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
                    new[] {users[0], users[3]}
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
                    new[] {users[0], users[3]}
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
                    new[] {users[0], users[3]}
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
                    new[] {users[0]}
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
                    new[] {users[0], users[3]}
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
                    new[] {users[3]}
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
                        Search = "1",
                        OrderBy = "Id"
                    },
                    new[] {users[3]}
                }
            };

            return data;
        }

        public static User[] GetUsers()
        {
            var users = new[]
            {
                new User
                {
                    Id = Guid.Parse(Graph.Subject0),
                    Name = Graph.Subject0Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject1),
                    Name = Graph.Subject1Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject2),
                    Name = Graph.Subject2Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject3),
                    Name = Graph.Subject3Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject4),
                    Name = Graph.Subject4Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject5),
                    Name = Graph.Subject5Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject6),
                    Name = Graph.Subject6Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject7),
                    Name = Graph.Subject7Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject8),
                    Name = Graph.Subject8Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject9),
                    Name = Graph.Subject9Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Subject10),
                    Name = Graph.Subject10Name
                }
            };
            return users;
        }

        public static User[] GetTraffickers()
        {
            var users = new[]
            {
                new User
                {
                    Id = Guid.Parse(Graph.Trafficker0),
                    Name = Graph.Trafficker0Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Trafficker1),
                    Name = Graph.Trafficker1Name
                },
                new User
                {
                    Id = Guid.Parse(Graph.Trafficker2),
                    Name = Graph.Trafficker2Name
                }
            };
            return users;
        }
        #endregion
    }
}