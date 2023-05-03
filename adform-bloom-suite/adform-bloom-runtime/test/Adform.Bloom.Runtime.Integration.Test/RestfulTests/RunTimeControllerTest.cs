using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Adform.Bloom.Application.Queries;
using Adform.Bloom.Runtime.Integration.Test.Utils;
using Adform.Bloom.Runtime.Read.Entities;
using FluentResults;
using MediatR;
using Xunit;

namespace Adform.Bloom.Runtime.Integration.Test.RestfulTests
{
    [Collection(nameof(Collection))]
    public class RunTimeControllerTest 
    {
        private readonly TestsFixture _fixture;

        public RunTimeControllerTest(TestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [MemberData(nameof(Common.QueryInput), MemberType = typeof(Common))]
        public async Task Evaluate_OnCorrectInput_Returns_PolicyResult(SubjectRuntimeQuery query, IReadOnlyList<RuntimeResult> expectedResult)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/runtime/subject-evaluation");

            var input = JsonSerializer.Serialize(query);

            request.Content = new StringContent(input, Encoding.UTF8, "application/json");

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------
            var response = await _fixture.SendRestRequestAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            var result = await response.Content.ReadAsJsonAsync<IReadOnlyList<RuntimeResult>>();

            Assert.Equal(expectedResult.Select(p => p.TenantId).OrderBy(i => i), result.Select(p => p.TenantId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantName).OrderBy(i => i), result.Select(p => p.TenantName).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantLegacyId).OrderBy(i => i), result.Select(p => p.TenantLegacyId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantType).OrderBy(i => i), result.Select(p => p.TenantType).OrderBy(i => i));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(expectedResult.SelectMany(o => o.Roles).OrderBy(p=>p), result.SelectMany(o=>o.Roles).OrderBy(p => p));
            Assert.Equal(expectedResult.SelectMany(o => o.Permissions).OrderBy(p => p), result.SelectMany(o => o.Permissions).OrderBy(p => p));
        }

        [Theory]
        [MemberData(nameof(Common.IntersectionQueryInput), MemberType = typeof(Common))]
        public async Task Evaluate_OnCorrectInput_Returns_IntersectionResult(SubjectIntersectionQuery query, IReadOnlyList<RuntimeResult> expectedResult)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/runtime/subject-intersection");

            var input = JsonSerializer.Serialize(query);

            request.Content = new StringContent(input, Encoding.UTF8, "application/json");

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------
            var response = await _fixture.SendRestRequestAsync(query.ActorId.ToString(), request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            var result = response is not null
                ? await response.Content.ReadAsJsonAsync<IReadOnlyList<RuntimeResult>>()
                : new List<RuntimeResult>();

            Assert.Equal(expectedResult.Select(p => p.TenantId).OrderBy(i => i), result.Select(p => p.TenantId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantName).OrderBy(i => i), result.Select(p => p.TenantName).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantLegacyId).OrderBy(i => i), result.Select(p => p.TenantLegacyId).OrderBy(i => i));
            Assert.Equal(expectedResult.Select(p => p.TenantType).OrderBy(i => i), result.Select(p => p.TenantType).OrderBy(i => i));


            Assert.Equal(expectedResult.SelectMany(o => o.Roles).OrderBy(p => p), result.SelectMany(o => o.Roles).OrderBy(p => p));
            Assert.Equal(expectedResult.SelectMany(o => o.Permissions).OrderBy(p => p), result.SelectMany(o => o.Permissions).OrderBy(p => p));
        }

        [Theory]
        [MemberData(nameof(Common.LegacyTenantExistenceQueryInput), MemberType = typeof(Common))]
        [MemberData(nameof(Common.RoleExistenceQueryInput), MemberType = typeof(Common))]
        [MemberData(nameof(Common.NodeExistenceQueryInput), MemberType = typeof(Common))]
        public async Task CheckExistence_OnCorrectInput_Returns_ExpectedResult(IRequest<Result<bool>> query, bool expectedResult, string path)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/runtime/{path}");

            var input = Serialize(query);

            request.Content = new StringContent(input, Encoding.UTF8, "application/json");

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------
            var response = await _fixture.SendRestRequestAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            var result = await response.Content.ReadAsJsonAsync<ExistenceResult>();
            Assert.Equal(expectedResult, result.Exists);
        }

        [Theory]
        [MemberData(nameof(Common.LegacyTenantExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        [MemberData(nameof(Common.RoleExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        [MemberData(nameof(Common.NodeExistenceQueryValidationErrorInput), MemberType = typeof(Common))]
        public async Task CheckExistence_IncorrectRequest_ReturnsBadRequest(IRequest<Result<bool>> query, string path)
        {
            // ---------------------------------------------------------
            // Arrange
            // ---------------------------------------------------------
            var request = new HttpRequestMessage(HttpMethod.Post, $"/v1/runtime/{path}");

            var input = Serialize(query);

            request.Content = new StringContent(input, Encoding.UTF8, "application/json");

            // ---------------------------------------------------------
            // Act
            // ---------------------------------------------------------
            var response = await _fixture.SendRestRequestAsync(request);

            // ---------------------------------------------------------
            // Assert
            // ---------------------------------------------------------
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        private static string Serialize(IRequest<Result<bool>> query)
        {
            return query switch
            {
                NodeExistenceQuery nodeQuery => JsonSerializer.Serialize(nodeQuery),
                LegacyTenantExistenceQuery legacyTenantQuery => JsonSerializer.Serialize(legacyTenantQuery),
                RoleExistenceQuery roleQuery => JsonSerializer.Serialize(roleQuery),
                _ => throw new Exception("Unsupported query")
            };
        }
    }
}