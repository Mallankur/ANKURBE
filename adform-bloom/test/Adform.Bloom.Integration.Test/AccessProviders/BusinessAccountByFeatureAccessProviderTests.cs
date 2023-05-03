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
using Moq;
using Xunit;
using Xunit.Extensions.Ordering;
using Feature = Adform.Bloom.Contracts.Output.Feature;

namespace Adform.Bloom.Integration.Test.AccessProviders;

[Collection(nameof(AcessProvidersCollection))]
[Order(1)]
public class BusinessAccountByFeatureAccessProviderTests
{
    private readonly TestsFixture _fixture;

    public BusinessAccountByFeatureAccessProviderTests(TestsFixture fixture)
    {
        _fixture = fixture;
    }

    #region Success scenarios

    [Theory]
    [MemberData(nameof(AdformAdminTestCases))]
    public async Task EvaluateAccess_AdformAdmin_ReturnsAll(string caseName, Feature feature, QueryParams queryParams,
        BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);
        var identity = Common.Test.Common.BuildUser(new RuntimeResponse
        {
            Roles = new[] { Graph.AdformAdminRoleName }
        });

        // Act
        var response = await engine.EvaluateAccessAsync(identity, feature, 0, 100, queryParams);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.OrderBy(OrderBy).Select(OrderBy), response.Data.Select(OrderBy));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));

        object OrderBy(BusinessAccount x)
        {
            return queryParams?.OrderBy != null
                ? typeof(BusinessAccount).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                : true;
        }
    }

    [Theory]
    [MemberData(nameof(LimitedVisibilitySubjectTestCases))]
    public async Task EvaluateAccess_LimitedVisibility_Returns_LimitedResults(string caseName, ClaimsPrincipal identity,
        Feature feature, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act
        var response = await engine.EvaluateAccessAsync(identity, feature, 0, 100, new QueryParams());

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.Select(x => x.Id).OrderBy(x => x),
            response.Data.Select(x => x.Id).OrderBy(x => x));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));
    }

    [Theory]
    [MemberData(nameof(FilteredResourcesTestCases))]
    public async Task EvaluateAccess_Filtered_Returns_FilteredResults(string caseName, ClaimsPrincipal identity,
        Feature feature, QueryParams queryParams, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act
        var response = await engine.EvaluateAccessAsync(identity, feature, 0, 100, queryParams);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.OrderBy(OrderBy).Select(OrderBy), response.Data.Select(OrderBy));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));

        object OrderBy(BusinessAccount x)
        {
            return queryParams?.OrderBy != null
                ? typeof(BusinessAccount).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                : true;
        }
    }

    [Theory]
    [MemberData(nameof(SearchTestCases))]
    public async Task EvaluateAccess_Search_Returns_FilteredResults(string caseName, ClaimsPrincipal identity,
        Feature feature, QueryParams queryParams, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act
        var response = await engine.EvaluateAccessAsync(identity, feature, 0, 100, queryParams);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.OrderBy(OrderBy).Select(OrderBy), response.Data.Select(OrderBy));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));

        object OrderBy(BusinessAccount x)
        {
            return queryParams?.OrderBy != null
                ? typeof(BusinessAccount).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                : true;
        }
    }

    [Theory]
    [MemberData(nameof(OrderedTestCases))]
    public async Task EvaluateAccess_Order_Returns_OrderedResults(string caseName, ClaimsPrincipal identity,
        Feature feature, QueryParams queryParams, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act
        var response = await engine.EvaluateAccessAsync(identity, feature, 0, 100, queryParams);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.Select(x => x.Id), response.Data.Select(x => x.Id));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));
    }

    [Theory]
    [MemberData(nameof(PaginatedTestCases))]
    public async Task EvaluateAccess_Pagination_Returns_PaginatedResults(string caseName, ClaimsPrincipal identity,
        Feature feature, int skip, int limit, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act
        var response =
            await engine.EvaluateAccessAsync(identity, feature, skip, limit, new QueryParams { OrderBy = "Name" });

        // Assert
        Assert.NotNull(response);
        Assert.Equal(businessAccounts.Select(x => x.Id).OrderBy(x => x),
            response.Data.Select(x => x.Id).OrderBy(x => x));
        Assert.All(businessAccounts, u => response.Data.Select(x => x.Id).Contains(u.Id));
    }

    #endregion

    #region Failure scenarios

    [Theory]
    [MemberData(nameof(LimitedVisibilitySubjectTestCases))]
    public async Task EvaluateAccess_ContextIdPassed_Throws_NotImplemented(string caseName, ClaimsPrincipal identity,
        Feature feature, BusinessAccount[] businessAccounts)
    {
        // Arrange
        var engine =
            new BusinessAccountByFeatureAccessProvider(_fixture.GraphClient, _fixture.BusinessAccountReadModel);

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            engine.EvaluateAccessAsync(identity, It.IsAny<Feature>(), 0, 0, new QueryParams { ContextId = Guid.Empty }));
    }

    #endregion

    #region Test Data

    public static TheoryData<string, Feature, QueryParams, BusinessAccount[]> AdformAdminTestCases()
    {
        return new TheoryData<string, Feature, QueryParams, BusinessAccount[]>
        {
            {
                "Case 0",
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature8),
                    Name = Graph.Feature8Name
                },
                new QueryParams(),
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant0),
                        Name = Graph.Tenant0Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant5),
                        Name = Graph.Tenant5Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant6),
                        Name = Graph.Tenant6Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant13),
                        Name = Graph.Tenant13Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant14),
                        Name = Graph.Tenant14Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant15),
                        Name = Graph.Tenant15Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant16),
                        Name = Graph.Tenant16Name
                    }
                }
            },

            {
                "Case 1",
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature0),
                    Name = Graph.Feature0Name
                },
                new QueryParams(),
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant0),
                        Name = Graph.Tenant0Name
                    }
                }
            },
            {
                "Case 2",
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature8),
                    Name = Graph.Feature8Name
                },
                new QueryParams
                {
                    ResourceIds = new[] {Guid.Parse(Graph.Tenant0), Guid.Parse(Graph.Tenant1)}
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant0),
                        Name = Graph.Tenant0Name
                    }
                }
            }
        };
    }

    public static TheoryData<string, ClaimsPrincipal, Feature, BusinessAccount[]> LimitedVisibilitySubjectTestCases()
    {
        return new TheoryData<string, ClaimsPrincipal, Feature, BusinessAccount[]>
        {
            {
                "Case 0",
                Common.Test.Common.BuildUser(new RuntimeResponse
                {
                    TenantId = Guid.Parse(Graph.Tenant4), TenantName = Graph.Tenant4Name,
                    Roles = new[] {Graph.OtherRole},
                    Permissions = new[] {""}
                }),
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature0),
                    Name = Graph.Feature0Name
                },
                new BusinessAccount[]
                {
                }
            },
            {
                "Case 1",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            }
        };
    }

    public static TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]>
        FilteredResourcesTestCases()
    {
        return new TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]>
        {
            {
                "Case 0",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    ResourceIds = new[] {Guid.Parse(Graph.Tenant1)}
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    }
                }
            },
            {
                "Case 1",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    ResourceIds = new[] {Guid.Parse(Graph.Tenant2)}
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 2",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    ResourceIds = new[] {Guid.Parse(Graph.Tenant3)}
                },
                new BusinessAccount[] { }
            },
            {
                "Case 3",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    ResourceIds = new[] {Guid.Parse(Graph.Tenant4)}
                },
                new BusinessAccount[] { }
            },
            {
                "Case 4",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    ResourceIds = new[]
                        {Guid.Parse(Graph.Tenant1), Guid.Parse(Graph.Tenant2), Guid.Parse(Graph.Tenant3)}
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            }
        };
    }

    public static TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]> SearchTestCases()
    {
        return new TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]>
        {
            {
                "Case 0",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "ant1"
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    }
                }
            },
            {
                "Case 1",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "ant2"
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 2",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "ant3"
                },
                new BusinessAccount[] { }
            },
            {
                "Case 3",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "ant4"
                },
                new BusinessAccount[] { }
            },
            {
                "Case 4",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "4"
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 5",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    Search = "ant"
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            }
        };
    }

    public static TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]> OrderedTestCases()
    {
        return new TheoryData<string, ClaimsPrincipal, Feature, QueryParams, BusinessAccount[]>
        {
            {
                "Case 0",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    OrderBy = nameof(BusinessAccount.Name)
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 1",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    OrderBy = nameof(BusinessAccount.Name),
                    SortingOrder = SortingOrder.Descending
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    }
                }
            },
            {
                "Case 2",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                new QueryParams
                {
                    OrderBy = nameof(BusinessAccount.Name),
                    SortingOrder = SortingOrder.Descending
                },
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    }
                }
            }
        };
    }

    public static TheoryData<string, ClaimsPrincipal, Feature, int, int, BusinessAccount[]> PaginatedTestCases()
    {
        return new TheoryData<string, ClaimsPrincipal, Feature, int, int, BusinessAccount[]>
        {
            {
                "Case 0",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                0,
                10,
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    },
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 1",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                1, 10,
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant2),
                        Name = Graph.Tenant2Name
                    }
                }
            },
            {
                "Case 2",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                0, 0,
                new BusinessAccount[] { }
            },
            {
                "Case 3",
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
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name,
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
                new Feature
                {
                    Id = Guid.Parse(Graph.Feature3),
                    Name = Graph.Feature0Name
                },
                0, 1,
                new BusinessAccount[]
                {
                    new()
                    {
                        Id = Guid.Parse(Graph.Tenant1),
                        Name = Graph.Tenant1Name
                    }
                }
            }
        };
    }

    #endregion
}