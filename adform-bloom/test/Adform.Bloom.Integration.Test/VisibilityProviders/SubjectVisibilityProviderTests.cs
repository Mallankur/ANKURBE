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
    public class SubjectVisibilityProviderTests
    {
        private static readonly Random Random = new Random(Guid.NewGuid().GetHashCode());

        private readonly TestsFixture _fixture;

        public SubjectVisibilityProviderTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Happy Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Default_Implementation_Succeeds_On_Subject_InHierarchy(string caseName,
            ClaimsPrincipal identity, Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds)
        {
            // Arrange
            IVisibilityProvider<QueryParamsTenantIds, Contracts.Output.Subject> engine =
                new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var tasks = subjects.Select(x => engine.HasItemVisibilityAsync(identity, x.Id));
            var result = await Task.WhenAll(tasks);

            // Assert
            Assert.True(result.All(x => x));
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Succeeds_On_Subjects_InHierarchy(string caseName, ClaimsPrincipal identity,
            Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = subjects.Select(s => s.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.True(response);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task GetVisibleResourcesAsync_Returns_AllAccessibleSubjects(string caseName,
            ClaimsPrincipal identity,
            Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);
            var allSubjects = subjects.Select(s => s.Id).ToArray();

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = allSubjects,
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(allSubjects);
        }

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task EvaluateVisibilityAsync_Succeeds_On_Subjects_InHierarchy(string caseName,
            ClaimsPrincipal identity,
            Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ContextId = contextId,
                ResourceIds = resourceIds,
                TenantIds = tenantIds
            }, 0, 20);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(subjects.OrderBy(o => o.Id).Select(p => p.Id).ToList(),
                response.Data.Select(o => o.Id).ToList());
            Assert.Equal(subjects.Length, response.TotalItems);
        }

        #endregion

        #region Failure Scenarios

        [Theory]
        [MemberData(nameof(CreateResult))]
        public async Task HasVisibilityAsync_Fails_On_Multiple_Subjects_InHierarchy_Including_Incorrect_One(
            string caseName,
            ClaimsPrincipal identity, Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = subjects.Select(f => f.Id).Concat(new[] {Guid.NewGuid()}).ToList(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task HasVisibilityAsync_Fails_On_Subjects_InHierarchy(string caseName, ClaimsPrincipal identity,
            IEnumerable<Subject> subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds, Guid[] unused)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.HasVisibilityAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = subjects.Select(s => s.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            Assert.False(response);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult))]
        public async Task GetVisibleResourcesAsync_Returns_OnlyAccessibleSubjects(
            string caseName, ClaimsPrincipal identity,
            IEnumerable<Subject> subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds,
            Guid[] accessibleSubjects)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.GetVisibleResourcesAsync(identity, new QueryParamsTenantIds
            {
                ResourceIds = subjects.Select(s => s.Id).ToArray(),
                TenantIds = tenantIds
            });

            // Assert
            response.Should().BeEquivalentTo(accessibleSubjects);
        }

        [Theory]
        [MemberData(nameof(CreateFailureResult_With_ContextId))]
        public async Task EvaluateVisibilityAsync_Fails_On_Inaccessible_Subjects_InHierarchy(string caseName, 
            ClaimsPrincipal identity, Subject[] subjects, Guid[] tenantIds, Guid? contextId, Guid[] resourceIds,
            Guid[] unused)
        {
            // Arrange
            var engine = new SubjectVisibilityProvider(_fixture.GraphClient);

            // Act
            var response = await engine.EvaluateVisibilityAsync(identity, new QueryParamsTenantIds
            {
                TenantIds = tenantIds,
                ResourceIds = resourceIds,
                ContextId = contextId
            }, 0, 100);

            // Assert
            Assert.NotNull(response);
            Assert.False(subjects.All(r => response.Data.Any(x => x.Id == r.Id)));
        }

        #endregion

        #region Test Data Generation

        public static TheoryData<string, ClaimsPrincipal, Subject[], Guid[], Guid?, Guid[]> CreateResult()
        {
            var subjects = Graph.GetSubjects();
            var subjectsWithoutAdformEmployees =
                subjects.Where(s => s.Id != Guid.Parse(Graph.Subject0)).ToArray();

            var data = new TheoryData<string, ClaimsPrincipal, Subject[], Guid[], Guid?, Guid[]>
            {
                {
                    "CaseSuccess0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        Roles = new[] {Graph.AdformAdminRoleName},
                    }),
                    subjects.Take(11).ToArray(),
                    null,
                    null,
                    null
                },
                {
                    "CaseSuccess1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant1),
                        TenantName = Graph.Tenant1Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[4]},
                    null,
                    null,
                    null
                },
                {
                    "CaseSuccess2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[2], subjects[3]},
                    null,
                    null,
                    null
                },
                {
                    "CaseSuccess3",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[8], subjects[9], subjects[10]},
                    null,
                    null,
                    null
                },
                {
                    "CaseSuccess4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[8]},
                    new[] {Guid.Parse(Graph.Tenant5)},
                    null,
                    null
                },
                {
                    "CaseSuccess5",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[8]},
                    new[] {Guid.Parse(Graph.Tenant5), Guid.Parse(Graph.Tenant14)},
                    null,
                    null
                },
                {
                    "CaseSuccess6",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[2], subjects[3]},
                    null,
                    Guid.Parse(Graph.CustomRole6),
                    null
                },
                {
                    "CaseSuccess7",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant7),
                            TenantName = Graph.Tenant7Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {subjects[4]},
                    null,
                    Guid.Parse(Graph.CustomRole8),
                    null
                },
                {
                    "CaseSuccess8",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant7),
                            TenantName = Graph.Tenant7Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {subjects[2], subjects[3]},
                    null,
                    Guid.Parse(Graph.CustomRole6),
                    null
                },
                {
                    "CaseSuccess9",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
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
                    new []{subjects[3]},
                    null,
                    Guid.Parse(Graph.LocalAdmin),
                    null
                },
                {
                    "CaseSuccess10",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant1),
                            TenantName = Graph.Tenant1Name,
                            Roles = new[] {Graph.AdformAdminRoleName},
                            Permissions = new[] {""}
                        }
                    }),
                    Array.Empty<Subject>(),
                    new[] {Guid.Parse(Graph.Tenant1)},
                    Guid.Parse(Graph.LocalAdmin),
                    null
                },
                {
                    "CaseSuccess11",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant7),
                            TenantName = Graph.Tenant7Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {subjects[2]},
                    null,
                    Guid.Parse(Graph.CustomRole6),
                    new[] {subjects[2].Id}
                }
            };

            return data;
        }

        public static TheoryData<string, ClaimsPrincipal, Subject[], Guid[], Guid?, Guid[], Guid[]>
            CreateFailureResult_With_ContextId()
        {
            var subjects = Graph.GetSubjects();
            var initialResult = CreateFailureResult();
            initialResult.Add(
                "Case Failure Context 0",
                Common.Test.Common.BuildUser(new[]
                {
                    new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7),
                        TenantName = Graph.Tenant7Name,
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
                new[] {subjects[6]},
                null,
                Guid.Parse(Graph.Role3),
                null,
                new Guid[] { });
            initialResult.Add(
                "Case Failure Context 1",
                Common.Test.Common.BuildUser(new[]
                {
                    new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant7),
                        TenantName = Graph.Tenant7Name,
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
                new[] {subjects[5]},
                null,
                Guid.Parse(Graph.CustomRole8),
                null,
                new Guid[] { });
            initialResult.Add(
                "Case Failure Context 2",
                Common.Test.Common.BuildUser(new[]
                {
                    new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
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
                new[] {subjects[5]},
                new[] {Guid.Parse(Graph.Tenant5)},
                Guid.Parse(Graph.Role3),
                null,
                new Guid[] { });
            initialResult.Add(
                "Case Failure Context 3",
                Common.Test.Common.BuildUser(new[]
                {
                    new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5),
                        TenantName = Graph.Tenant5Name,
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
                new[] {subjects[2]},
                new[] {Guid.Parse(Graph.Tenant5)},
                Guid.Parse(Graph.Role3),
                new[] {subjects[0].Id},
                new Guid[] { });

            return initialResult;
        }

        public static TheoryData<string, ClaimsPrincipal, Subject[], Guid[], Guid?, Guid[], Guid[]> CreateFailureResult()
        {
            var subjects = Graph.GetSubjects();
            var data = new TheoryData<string, ClaimsPrincipal, Subject[], Guid[], Guid?, Guid[], Guid[]>
            {
                {
                    "Case Failure 0",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2), TenantName = Graph.Tenant2Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[0]},
                    null,
                    null,
                    null,
                    new Guid[] { }
                },
                {
                    "Case Failure 1",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[0], subjects[1]},
                    null,
                    null,
                    null,
                    new Guid[] { }
                },
                {
                    "Case Failure 2",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant2),
                        TenantName = Graph.Tenant2Name,
                        Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[1], subjects[2], subjects[3]},
                    new[] {Guid.Parse(Graph.Tenant5)},
                    null,
                    null,
                    new Guid[] { }
                },
                {
                    "Case Failure 3",
                    Common.Test.Common.BuildUser(new[]
                    {
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant2),
                            TenantName = Graph.Tenant2Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        },
                        new RuntimeResponse
                        {
                            TenantId = Guid.Parse(Graph.Tenant5),
                            TenantName = Graph.Tenant5Name,
                            Roles = new[] {Graph.OtherRole},
                            Permissions = new[] {""}
                        }
                    }),
                    new[] {subjects[1], subjects[2], subjects[3]},
                    new[] {Guid.Parse(Graph.Tenant2)},
                    null,
                    null,
                    new Guid[] {subjects[2].Id, subjects[3].Id}
                },
                {
                    "Case Failure 4",
                    Common.Test.Common.BuildUser(new RuntimeResponse
                    {
                        TenantId = Guid.Parse(Graph.Tenant5), TenantName = Graph.Tenant5Name, Roles = new[] {Graph.OtherRole},
                        Permissions = new[] {""}
                    }),
                    new[] {subjects[5]},
                    null,
                    null,
                    null,
                    new Guid[] { }
                },
            };


            return data;
        }

        #endregion
    }
}