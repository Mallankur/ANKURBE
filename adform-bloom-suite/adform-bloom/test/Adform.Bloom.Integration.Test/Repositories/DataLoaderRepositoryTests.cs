using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.DataAccess.Repositories;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using Xunit;

namespace Adform.Bloom.Integration.Test.Repositories
{
    [Collection(nameof(RepositoriesCollection))]
    public class DataLoaderRepositoryTests
    {
        private readonly TestsFixture _fixture;

        public DataLoaderRepositoryTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(CreatePolicyRoleResult))]
        public async Task GetNodesWithConnected_Policy_Role_Tests(IReadOnlyCollection<string> policyIds,
            Dictionary<string, HashSet<string>> expectedResults)
        {
            // Arrange
            var adminGraphRepository = new DataLoaderRepository(_fixture.GraphClient);

            // Act
            var response =
                (await adminGraphRepository.GetNodesWithConnectedAsync<Policy, Role>(
                    policyIds.Select(id => Guid.Parse(id)),
                    Constants.ContainsLink)).ToList();
            var resultMap = response.GroupBy(entity => entity.StartNodeId)
                .Select(g => (g.Key.ToString(), g.Select(r => r.ConnectedNode.Id.ToString()).ToHashSet()))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResults.Sum(x => x.Value.Count), response.Count);
            foreach (var id in expectedResults.Keys)
            {
                foreach (var roleId in expectedResults[id])
                {
                    Assert.Contains(roleId, resultMap[id]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(CreateRolePermissionResult))]
        public async Task GetNodesWithConnected_Role_Permission_Tests(IReadOnlyCollection<string> rolesIds,
            Dictionary<string, HashSet<string>> expectedResults)
        {
            // Arrange
            var adminGraphRepository = new DataLoaderRepository(_fixture.GraphClient);
            // Act
            var response =
                (await adminGraphRepository.GetNodesWithConnectedAsync<Role, Permission>(
                    rolesIds.Select(id => Guid.Parse(id)),
                    Constants.ContainsLink)).ToList();
            var resultMap = response.GroupBy(entity => entity.StartNodeId)
                .Select(g => (g.Key.ToString(), g.Select(r => r.ConnectedNode.Id.ToString()).ToHashSet()))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResults.Sum(x => x.Value.Count), response.Count);
            foreach (var roleId in expectedResults.Keys)
            {
                foreach (var permissionId in expectedResults[roleId])
                {
                    Assert.Contains(permissionId, resultMap[roleId]);
                }
            }
        }


        [Theory]
        [MemberData(nameof(CreateRoleSubjectsResult))]
        public async Task GetNodesWithIntermediateWithConnectedAsync_Role_Subjects_Test(
            string caseNumber,
            IReadOnlyCollection<string> roleIds,
            Dictionary<string, HashSet<string>> expectedResults)
        {
            // Arrange
            var adminGraphRepository = new DataLoaderRepository(_fixture.GraphClient);
            // Act
            var response =
                (await adminGraphRepository.GetNodesWithIntermediateWithConnectedAsync<Role, Group, Subject>(
                    roleIds.Select(id => Guid.Parse(id)), Constants.AssignedIncomingLink,
                    Constants.MemberOfIncomingLink)).ToList();
            var resultMap = response.GroupBy(entity => entity.StartNodeId)
                .Select(g => (g.Key.ToString(), g.Select(r => r.ConnectedNode.Id.ToString()).ToHashSet()))
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(expectedResults.SelectMany(x=>x.Value).Distinct().Count(), response.Count);
            foreach (var roleId in expectedResults.Keys)
            {
                foreach (var nodeId in expectedResults[roleId])
                {
                    Assert.Contains(nodeId, resultMap[roleId]);
                }
            }
        }

        public static TheoryData<string[], Dictionary<string, HashSet<string>>> CreatePolicyRoleResult()
        {
            var data = new TheoryData<string[], Dictionary<string, HashSet<string>>>();
            data.Add(new string[0], new Dictionary<string, HashSet<string>>());
             data.Add(new[] {Graph.Policy1}, new Dictionary<string, HashSet<string>>
             {
                 {
                     Graph.Policy1,
                     new HashSet<string>
                     {
                         Graph.CustomRole0,
                         Graph.Role1,
                         Graph.Role2,
                         Graph.Role3,
                         Graph.CustomRole4,
                         Graph.CustomRole5,
                         Graph.LocalAdmin,
                         Graph.IamAdmin
                     }
                 }
             });
            data.Add(new[] {Graph.Policy2, Graph.Policy3}, new Dictionary<string, HashSet<string>>
            {
                {
                    Graph.Policy2,
                    new HashSet<string>
                    {
                        Graph.CustomRole6,
                        Graph.CustomRole7,
                        Graph.CustomRole8,
                        Graph.CustomRole9,
                        Graph.CustomRole10,
                        Graph.CustomRole11,
                        Graph.BuySideAdmin,
                        Graph.Trafficker1Role,
                        Graph.Trafficker2Role,
                        Graph.Trafficker7Role,
                        Graph.Trafficker8Role,
                        Graph.Trafficker9Role,
                        Graph.Trafficker10Role
                    }
                },
                {
                    Graph.Policy3,
                    new HashSet<string>
                    {
                        Graph.CustomRole12,
                        Graph.CustomRole13,
                        Graph.CustomRole14,
                        Graph.CustomRole15,
                        Graph.CustomRole16,
                        Graph.CustomRole17,
                        Graph.SellSideAdmin,
                        Graph.Trafficker3Role,
                        Graph.Trafficker4Role,
                        Graph.Trafficker11Role,
                        Graph.Trafficker12Role
                    }
                }
            });
            return data;
        }

        public static TheoryData<string[], Dictionary<string, HashSet<string>>> CreateRolePermissionResult()
        {
            var data = new TheoryData<string[], Dictionary<string, HashSet<string>>>();

            data.Add(new string[0], new Dictionary<string, HashSet<string>>());
            data.Add(new[] {Graph.Role1}, new Dictionary<string, HashSet<string>>
            {
                {
                    Graph.Role1,
                    new HashSet<string>
                    {
                        Graph.Permission1,
                        Graph.Permission2,
                    }
                }
            });
            data.Add(new[] {Graph.Role1, Graph.Role2}, new Dictionary<string, HashSet<string>>
            {
                {
                    Graph.Role1,
                    new HashSet<string>
                    {
                        Graph.Permission1,
                        Graph.Permission2,
                    }
                },
                {
                    Graph.Role2,
                    new HashSet<string>
                    {
                        Graph.Permission3,
                        Graph.Permission4,
                    }
                }
            });
            return data;
        }

        public static TheoryData<string, string[], Dictionary<string, HashSet<string>>> CreateRoleSubjectsResult()
        {
            var data = new TheoryData<string, string[], Dictionary<string, HashSet<string>>>
            {
                {
                    "Case 0",
                    new string[0],
                    new Dictionary<string, HashSet<string>>()
                },
                {
                    "Case 1",
                    new[] {Graph.AdformAdmin},
                    new Dictionary<string, HashSet<string>>
                        {{Graph.AdformAdmin, new HashSet<string> {Graph.Subject0,}}}
                },
                {
                    "Case 2",
                    new[] {Graph.CustomRole6, Graph.CustomRole10},
                    new Dictionary<string, HashSet<string>>
                    {
                        {Graph.CustomRole10, new HashSet<string> {Graph.Trafficker0}},
                        {Graph.CustomRole6, new HashSet<string>
                        {
                             Graph.Subject2, Graph.Subject3
                        }}
                    }
                }
            };

            return data;
        }
    }
}