using System;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Domain.Entities;
using Adform.Bloom.Infrastructure;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLAssignUsersCollection))]
    public class GraphQLUsersAssigmentTests
    {

        private const string AssignToRoleMutation = @"
mutation{{
  updateUserAssignments(userId: ""{0}"",
      assignRoleBusinessAccountIds: [{{
        roleId:""{1}"",
        businessAccountId:""{2}""}}],
      assetsReassignments:[{{businessAccountType: agency, legacyBusinessAccountId: 1, newUserId: ""{3}""}}]
  )
}}
";

        private const string UnassignToRoleMutation = @"
mutation{{  
  updateUserAssignments(userId: ""{0}"",
      unassignRoleBusinessAccountIds: [{{
        roleId:""{1}"",
        businessAccountId:""{2}""}}],
      assetsReassignments:[{{businessAccountType: agency, legacyBusinessAccountId: 1, newUserId: ""{3}""}}]
  )
}}
";

        private readonly TestsFixture _fixture;

        public GraphQLUsersAssigmentTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory, Order(0)]
        [MemberData(nameof(Scenarios.Assign_User_To_Role_Results_For_Subject1), MemberType = typeof(Scenarios))]
        public async Task Assign_And_Then_Unassign_User_ToFrom_Role_Test(string tenantId, string subjectId, string roleId, bool canAssign)
        {
            await _fixture.OngDB.GraphRepository.BulkUnassignSubjectFromRolesAsync(
                Guid.Parse(subjectId),
                new[]
                {
                    new RoleTenant
                    {
                        RoleId =  Guid.Parse(roleId),
                        TenantId = Guid.Parse(tenantId)
                    }
                });

            await AssignTest(
                Guid.Parse(tenantId), Guid.Parse(subjectId), Guid.Parse(roleId), canAssign);

            if (canAssign)
                await UnassignTest(Guid.Parse(tenantId), Guid.Parse(subjectId), Guid.Parse(roleId));
        }


        private async Task AssignTest(Guid businessAccountId, Guid subjectId, Guid roleId, bool canAssign)
        {
            // Arrange
            var mutationAssign = string.Format(AssignToRoleMutation, subjectId, roleId, businessAccountId, subjectId);
            var requestAssign = new GraphQLRequest(mutationAssign);

            // Act
            var response =  await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestAssign, !canAssign);

            //Assert
            if (canAssign)
            {
                var jsonEle = response["updateUserAssignments"];
                Assert.Equal(subjectId.ToString(), jsonEle.ToString());
            }
            else
            {
                var errors = response.Errors;
                Assert.True(errors.Length == 1);
                Assert.Equal("The subject of the token does not have access to a role.", errors[0].Message);
            }

            var hasLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Subject, Group, Role>(s => s.Id == subjectId,
                p => p.Id == roleId, Constants.MemberOfWithVariableLink, Constants.AssignedLink);
            if (canAssign)
                Assert.True(hasLink);
            else
                Assert.False(hasLink);
        }
        private async Task UnassignTest(Guid tenantId, Guid subjectId, Guid roleId)
        {
            // Arrange
            var mutationUnassign = string.Format(UnassignToRoleMutation, subjectId, roleId, tenantId, subjectId);
            var requestUnassign = new GraphQLRequest(mutationUnassign);

            // Act
            var responseUnassign = (JObject)await _fixture.SendGraphqlRequestAsync(Graph.Subject0, requestUnassign);

            //Assert

            var jsonEle = responseUnassign["updateUserAssignments"];
            Assert.Equal(subjectId.ToString(), jsonEle.ToString());
            var hasNoLink = await _fixture.OngDB.GraphRepository.HasRelationshipAsync<Subject, Group, Role>(s => s.Id == subjectId,
                p => p.Id == roleId, Constants.MemberOfWithVariableLink, Constants.AssignedLink);
            Assert.False(hasNoLink);
        }
    }
}