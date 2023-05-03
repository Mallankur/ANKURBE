using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Providers.Access;
using Adform.Bloom.Infrastructure.Models;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Ciam.OngDb.Repository;
using Xunit;
using Xunit.Extensions.Ordering;
using Feature = Adform.Bloom.Contracts.Output.Feature;
using Role = Adform.Bloom.Contracts.Output.Role;

namespace Adform.Bloom.Integration.Test.AccessProviders
{
    [Collection(nameof(AcessProvidersCollection))]
    [Order(1)]
    public class FeatureByRoleAccessProviderTests
    {
        private readonly TestsFixture _fixture;

        public FeatureByRoleAccessProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateAccessAsync_Succeeds_On_Features_InHierarchy(string caseNumber,
            ClaimsPrincipal identity, Role context, QueryParamsTenantIds queryParams, Feature[] features)
        {
            // Arrange
            var engine = new FeatureByRoleAccessProvider(_fixture.GraphClient);
            var featuresOrdered = queryParams.SortingOrder == SortingOrder.Ascending
                ? features.OrderBy(OrderBy)
                : features.OrderByDescending(OrderBy);

            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(featuresOrdered.Select(OrderBy), response.Data.Select(OrderBy));
            Assert.All(features, u => response.Data.Select(x => x.Id).Contains(u.Id));

            object OrderBy(Feature x)
            {
                return queryParams?.OrderBy != null
                    ? typeof(Feature).GetProperty(queryParams.OrderBy!)!.GetValue(x, null)
                    : true;
            }
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task EvaluateAccessAsync_Fails_On_Inaccessible_Features_InHierarchy(
            ClaimsPrincipal identity, Role context, QueryParamsTenantIds queryParams, Feature[] features)
        {
            // Arrange
            var engine = new FeatureByRoleAccessProvider(_fixture.GraphClient);
        
            // Act
            var response = await engine.EvaluateAccessAsync(identity, context, 0, 100, queryParams);
        
            // Assert
            Assert.NotNull(response);
            Assert.False(features.All(u => response.Data.Any(x => x.Id == u.Id)));
        }

        #endregion

        #region Test Data Creation

        public static TheoryData<string, ClaimsPrincipal, Role, QueryParamsTenantIds, Feature[]> CreateResult()
        {
            var data = new TheoryData<string, ClaimsPrincipal, Role, QueryParamsTenantIds, Feature[]>
            {
                {
                    "Case 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName}
                    }),
                    new Role
                    {
                        Id = Guid.Parse(Graph.AdformAdmin),
                        Name = Graph.AdformAdminRoleName
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[]
                    {
                        new Feature
                        {
                            Id = Guid.Parse(Graph.Feature0),
                            Name = Graph.Feature0Name
                        }
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
                    new Role
                    {
                        Id = Guid.Parse(Graph.Trafficker7Role),
                        Name = Graph.TraffickerRoleName
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Name",
                        SortingOrder = SortingOrder.Ascending,
                        TenantIds = new [] { Guid.Parse(Graph.Tenant7) }
                    },
                    new[] {
                        new Feature
                        {
                            Id = Guid.Parse(Graph.Feature4),
                            Name = Graph.Feature4Name
                        },
                        new Feature
                        {
                            Id = Guid.Parse(Graph.Feature3),
                            Name = Graph.Feature3Name
                        }
                    }
                }
            };
            return data;
        }

        public static TheoryData<ClaimsPrincipal, Role, QueryParamsTenantIds, Feature[]> CreateFailureResult()
        {
            var data = new TheoryData<ClaimsPrincipal, Role, QueryParamsTenantIds, Feature[]>
            {
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName}
                    }),
                    new Role
                    {
                        Id = Guid.Parse(Graph.Role1),
                        Name = Graph.Role1Name
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id"
                    },
                    new[]
                    {
                        new Feature
                        {
                            Id = Guid.Parse(Graph.Feature0),
                            Name = Graph.Feature0Name
                        }
                    }
                },
                {
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName}
                    }),
                    new Role
                    {
                        Id = Guid.Parse(Graph.AdformAdmin),
                        Name = Graph.AdformAdminRoleName
                    },
                    new QueryParamsTenantIds
                    {
                        OrderBy = "Id",
                        TenantIds = new [] { Guid.Parse(Graph.Tenant7) }
                    },
                    new[]
                    {
                        new Feature
                        {
                            Id = Guid.Parse(Graph.Feature0),
                            Name = Graph.Feature0Name
                        }
                    }
                }
            };

            return data;
        }

        #endregion
    }
}