using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using GraphQL;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test.GraphqlTests
{
    [Collection(nameof(Collection))]
    public class ExistenceCheckGQLTest
    {
        private readonly TestsFixture _fixture;

        public ExistenceCheckGQLTest(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(Common.RoleExistenceQueryInput), MemberType = typeof(Common))]
        public async Task RoleExistenceCheck_MatchesExpectedResult(RoleExistenceQuery query, bool expectedResult, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = @"
                query roleExistsCheckQuery($tenantId: ID!, $roleName: String!) {
                  roleExistsCheck(tenantId: $tenantId, roleName:$roleName) {
                    exists
                  }
                }",
                Variables = new
                {
                    tenantId = query.TenantId,
                    roleName = query.RoleName
                }

            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendRoleExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Equal(expectedResult, result.Exists);
        }

        [Theory]
        [MemberData(nameof(Common.RoleExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        public async Task RoleExistenceCheck_InvalidRequest_ReturnsError(RoleExistenceQuery query, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = @"
                query roleExistsCheckQuery($tenantId: ID!, $roleName: String!) {
                  roleExistsCheck(tenantId: $tenantId, roleName:$roleName) {
                    exists
                  }
                }",
                Variables = new
                {
                    tenantId = query.TenantId,
                    roleName = query.RoleName
                }

            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendRoleExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Null(result);
        }


        [Theory]
        [MemberData(nameof(Common.NodeExistenceQueryInput), MemberType = typeof(Common))]
        public async Task NodeExistenceCheck_MatchesExpectedResult(NodeExistenceQuery query, bool expectedResult, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = $@"

                query {{
                  nodesExistCheck(nodes: {ToQueryArg(query.NodeDescriptors)}) {{
                    exists
                  }}
                }}",
            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendNodesExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Equal(expectedResult, result.Exists);
        }

        [Theory]
        [MemberData(nameof(Common.NodeExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        public async Task NodeExistenceCheck_InvalidRequest_ReturnsError(NodeExistenceQuery query, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = $@"

                query {{
                  nodesExistCheck(nodes: {ToQueryArg(query.NodeDescriptors)}) {{
                    exists
                  }}
                }}",
            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendNodesExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(Common.LegacyTenantExistenceQueryInput), MemberType = typeof(Common))]
        public async Task LegacyTenantExistenceCheck_MatchesExpectedResult(LegacyTenantExistenceQuery query, bool expectedResult, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = @"
                query legacyTenantsExistCheckQuery($tenantType: String!, $tenantLegacyIds: [Int!]!) {
                  legacyTenantsExistCheck(tenantType: $tenantType, tenantLegacyIds: $tenantLegacyIds) {
                    exists
                  }
                }",
                Variables = new
                {
                    tenantType = query.TenantType,
                    tenantLegacyIds = query.TenantLegacyIds.AsEnumerable()
                }
            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendLegacyTenantsExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Equal(expectedResult, result.Exists);
        }

        [Theory]
        [MemberData(nameof(Common.LegacyTenantExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        public async Task LegacyTenantExistenceCheck_InvalidRequest_ReturnsError(LegacyTenantExistenceQuery query, string unused)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = @"
                query legacyTenantsExistCheckQuery($tenantType: String!, $tenantLegacyIds: [Int!]!) {
                  legacyTenantsExistCheck(tenantType: $tenantType, tenantLegacyIds: $tenantLegacyIds) {
                    exists
                  }
                }",
                Variables = new
                {
                    tenantType = query.TenantType,
                    tenantLegacyIds = query.TenantLegacyIds.AsEnumerable()
                }
            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendLegacyTenantsExistenceCheckGraphQlQueryAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Null(result);
        }

        private static string ToQueryArg(List<NodeDescriptor> l)
        {
            string Stringifier(NodeDescriptor n)
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append($"{{label:\"{n.Label}\"");
                if (n.Id != null) stringBuilder.Append($" id:\"{n.Id}\"");
                if (n.UniqueName != null) stringBuilder.Append($" uniqueName:\"{n.UniqueName}\"");
                stringBuilder.Append("}");
                return stringBuilder.ToString();
            }

            return $"[{string.Join(",", l.Select((Func<NodeDescriptor, string>) Stringifier))}]";
        }
    }
}