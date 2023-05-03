using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adform.Bloom.Common.Test;
using Adform.Bloom.Contracts.Output;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Infrastructure;
using Adform.Bloom.Runtime.Contracts.Request;
using GraphQL;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions.Ordering;

namespace Adform.Bloom.Acceptance.Test.GraphqlTests
{
    [Collection(nameof(GraphQLPermissionBusinessAccountsCollection))]
    public class PermissionBusinessAccountsTests
    {
        #region Constatnts

        private const string UnauthorizedMessage = "The current user is not authorized to access this resource.";
        private const string ArgumentRequiredMessage = "The argument `{0}` is required.";

        private enum permissionBusinessAccountEvaluationParameter
        {
            ANY,
            ALL
        }

        private const string PermissionBusinessAccountsQuery = @"
{{
  permissionBusinessAccounts(userId: ""{0}"", permissionNames: {1}, permissionBusinessAccountEvaluationParameter: {2}) {{
      id
      name,
      legacyId
  }}
}}
";

        private const string PermissionBusinessAccountsWithFilterQuery = @"
{{
  permissionBusinessAccounts(userId: ""{0}"", permissionNames: {1}, permissionBusinessAccountEvaluationParameter: {2}), businessAccountIds: {3} {{
      id
      name,
      legacyId
  }}
}}
";

        private const string PermissionBusinessAccountsQueryWithoutEvaluationParameter = @"
{{
  permissionBusinessAccounts(userId: ""{0}"", permissionNames: {1}) {{
      id
      name,
      legacyId
  }}
}}
";

        private const string PermissionBusinessAccountsQueryWithoutPermissionNames = @"
{{
  permissionBusinessAccounts(userId: ""{0}"", permissionBusinessAccountEvaluationParameter: {1}) {{
      id
      name,
      legacyId
  }}
}}
";

        private const string PermissionBusinessAccountsQueryWithoutUserId = @"
{{
  permissionBusinessAccounts( permissionNames: {0}, permissionBusinessAccountEvaluationParameter: {1}) {{
      id
      name,
      legacyId
  }}
}}
";

        #endregion

        private readonly TestsFixture _fixture;

        public PermissionBusinessAccountsTests(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        #region Query Tests

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_AdformAdmin_Returns_ExpectedResult()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject0);
            var subjectId = Guid.Parse(Graph.Subject0);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            var expectedTenants =
                (await ExpectedTenants(actorId, subjectId, evaluationParam, permissionNames)).ToList();
            var query = string.Format(PermissionBusinessAccountsQuery, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = (JObject) await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);

            // Assert
            var jsonEle = response["permissionBusinessAccounts"];
            var actualTenants = jsonEle.ToObject<IReadOnlyCollection<BusinessAccount>>();

            Assert.Equal(expectedTenants.Count(), actualTenants.Count);
            Assert.All(actualTenants, at => expectedTenants.Any(etId => etId == at.Id));
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_LocalAdmin_Returns_ExpectedResult()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject3);
            var subjectId = Guid.Parse(Graph.Subject2);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission5Name, Graph.Permission6Name};
            

            var expectedTenants =
                (await ExpectedTenants(actorId, subjectId, evaluationParam, permissionNames)).ToList();
            var query = string.Format(PermissionBusinessAccountsQuery, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = (JObject) await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);

            // Assert
            var jsonEle = response["permissionBusinessAccounts"];
            var actualTenants = jsonEle.ToObject<IReadOnlyCollection<BusinessAccount>>();

            Assert.Equal(expectedTenants.Count(), actualTenants.Count);
            Assert.All(actualTenants, at => expectedTenants.Any(etId => etId == at.Id));
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_LocalAdmin_WithoutAccess_Returns_Forbidden()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject3);
            var subjectId = Guid.Parse(Graph.Subject10);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            
            var expectedTenants =
                (await ExpectedTenants(actorId, subjectId, evaluationParam, permissionNames)).ToList();
            var query = string.Format(PermissionBusinessAccountsQuery, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(ErrorMessages.SubjectCannotAccessEntity, errors[0].Message);
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_RegularUser_Returns_Unauthorized()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject9);
            var subjectId = Guid.Parse(Graph.Subject10);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            var expectedTenants =
                (await ExpectedTenants(actorId, subjectId, evaluationParam, permissionNames)).ToList();
            var query = string.Format(PermissionBusinessAccountsQuery, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(UnauthorizedMessage, errors[0].Message);
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_WithBusinessAccountIds_Returns_ExpectedResult()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject9);
            var subjectId = Guid.Parse(Graph.Subject10);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            var businessAccountIds = new List<Guid>
                {Guid.Parse(Graph.Tenant3), Guid.Parse(Graph.Tenant6), Guid.Parse(Graph.Tenant11)};
            var expectedTenants =
                (await ExpectedTenants(actorId, subjectId, evaluationParam, permissionNames))
                .Where(et => businessAccountIds.Contains(et)).ToList();
            var query = string.Format(PermissionBusinessAccountsQuery, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(UnauthorizedMessage, errors[0].Message);
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_WithoutEvaluationParameter_Returns_ArgumentMissing()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject0);
            var subjectId = Guid.Parse(Graph.Subject0);
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            var query = string.Format(PermissionBusinessAccountsQueryWithoutEvaluationParameter, subjectId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]");
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(
                string.Format(ArgumentRequiredMessage,
                    Constants.Parameters.PermissionBusinessAccountEvaluationParameter), errors[0].Message);
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_WithoutPermissionNames_Returns_ArgumentMissing()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject0);
            var subjectId = Guid.Parse(Graph.Subject0);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var query = string.Format(PermissionBusinessAccountsQueryWithoutPermissionNames, subjectId,
                evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(string.Format(ArgumentRequiredMessage, Constants.Parameters.PermissionNames),
                errors[0].Message);
        }

        [Fact]
        [Order(0)]
        public async Task Get_PermissionBusinessAccounts_WithoutUserId_Returns_ArgumentMissing()
        {
            // Arrange
            var actorId = Guid.Parse(Graph.Subject0);
            var evaluationParam = permissionBusinessAccountEvaluationParameter.ALL;
            var permissionNames = new[] {Graph.Permission0Name, Graph.Permission14Name};
            var query = string.Format(PermissionBusinessAccountsQueryWithoutUserId,
                $"[{string.Join(",", permissionNames.Select(p => $"\"{p}\""))}]", evaluationParam.ToString().ToLowerInvariant());
            var request = new GraphQLRequest(query);

            // Act
            var response = await _fixture.SendGraphqlRequestAsync(actorId.ToString(), request);
            var errors = response.Errors;

            // Assert
            Assert.True(errors.Length == 1);
            Assert.Equal(string.Format(ArgumentRequiredMessage, Constants.Parameters.UserId), errors[0].Message);
        }

        #endregion

        private async Task<IEnumerable<Guid>> ExpectedTenants(Guid actorId, Guid subjectId,
            permissionBusinessAccountEvaluationParameter evaluationParam, IEnumerable<string> permissionNames)
        {
            var actorTenants = _fixture.BloomApiPrincipal[actorId.ToString()].GetTenants();
            var runtimeRequest = new SubjectRuntimeRequest
            {
                SubjectId = subjectId,
                TenantIds = actorTenants.Select(Guid.Parse)
            };
            var subjectTenants = await _fixture.RuntimeClient.InvokeAsync(runtimeRequest);
            var expectedTenants = subjectTenants.Where(t =>
                    evaluationParam == permissionBusinessAccountEvaluationParameter.ALL
                        ? permissionNames.All(t.Permissions.Contains)
                        : permissionNames.Any(t.Permissions.Contains))
                .Select(t => t.TenantId);

            return expectedTenants;
        }
    }
}