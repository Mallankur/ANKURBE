using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Read.Entities;
using GraphQL;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test.GraphqlTests
{
    [Collection(nameof(Collection))]
    public class SubjectEvaluationGQLTest
    {
        private readonly TestsFixture _fixture;

        public SubjectEvaluationGQLTest(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(Common.QueryInput), MemberType = typeof(Common))]
        public async Task SubjectEvaluation_OnCorrectInput_Returns_PolicyResult(SubjectRuntimeQuery data,
            IReadOnlyList<RuntimeResult> expectedResult)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new GraphQLRequest
            {
                Query = @"
                query subjectEvaluationQuery($subjectId: ID!,$inheritanceEnabled: Boolean!, $policyNames: [String!], 
                    $tenantIds: [ID!], $tenantLegacyIds: [Int!], $tenantType: String) {
                  subjectEvaluation(subjectId: $subjectId,inheritanceEnabled:$inheritanceEnabled,policyNames:$policyNames,
                    tenantIds:$tenantIds,tenantLegacyIds:$tenantLegacyIds,tenantType:$tenantType) {
                       roles
                       permissions
                       tenantId
                       tenantName
                       tenantLegacyId
                       tenantType
                  }
                }",
                Variables = new
                {
                    subjectId = data.SubjectId,
                    inheritanceEnabled = data.InheritanceEnabled,
                    tenantIds = data.TenantIds,
                    tenantLegacyIds = data.TenantLegacyIds,
                    policyNames = data.PolicyNames,
                    tenantType = data.TenantType
                }
            };

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------

            var result = await _fixture.SendGraphQlRequestAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------

            Assert.Equal(expectedResult.Select(p => p.TenantId).OrderBy(i => i),
                result.Select(p => p.TenantId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantName).OrderBy(i => i),
                result.Select(p => p.TenantName).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantLegacyId).OrderBy(i => i),
                result.Select(p => p.TenantLegacyId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantType).OrderBy(i => i),
                result.Select(p => p.TenantType).OrderBy(i => i));
            Assert.Equal(expectedResult.SelectMany(p => p.Roles).OrderBy(i => i),
                result.SelectMany(p => p.Roles).OrderBy(i => i));
            Assert.Equal(expectedResult.SelectMany(p => p.Permissions).OrderBy(i => i),
                result.SelectMany(p => p.Permissions).OrderBy(i => i));
        }
    }
}