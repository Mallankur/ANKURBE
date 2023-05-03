using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Adform.Bloom.Runtime.Infrastructure;
using Adform.Bloom.Runtime.Infrastructure.Persistence;
using Adform.Bloom.Runtime.Infrastructure.Services;
using Adform.Ciam.OngDb.Core.Extensions;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Adform.Bloom.Runtime.Read.Test.Infrastructure
{
    public class RuntimeProviderTests
    {
        private RuntimeProvider _provider;
        private readonly Mock<IDriver> _driver;

        public RuntimeProviderTests()
        {
            _driver = new Mock<IDriver>();
            _driver.SetupAllProperties();
        }

        [Theory]
        [MemberData(nameof(Common.Data), MemberType = typeof(Common))]
        public async Task GetSubjectEvaluation_Returns_Expected_Result(Common.TestData data)
        {
            // Arrange
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var mapService = new Mock<ICursorToResultConverter>();
            mapService
                .Setup(o => o.ConvertToRuntimeResultAsync(It.IsAny<IResultCursor>()))
                .ReturnsAsync(data.Output);

            var cursor = new Mock<IResultCursor>();
            session.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            var parameters = new Dictionary<string, object>();
            parameters.Add("subjectId", data.Input.SubjectId.ToString());
            var query = new StringBuilder();
            query.Append(Queries.BaseQuery());
            if (data.Input.TenantIds.Any())
            {
                query.Append(Queries.TenantQuery(data.Input.InheritanceEnabled, data.Input.TenantType));
            }

            if (data.Input.PolicyNames.Any())
            {
                query.Append(Queries.PolicyQuery(data.Input.InheritanceEnabled));
            }


            if (data.Input.TenantIds.Any())
            {
                parameters.Add("tenantIds", data.Input.TenantIds.Select(x => x.ToString()));
                query.Append(Queries.TenantIdWhere);
            }

            if (data.Input.TenantLegacyIds.Any())
            {
                parameters.Add("tenantLegacyIds", data.Input.TenantLegacyIds.Select(x => x.ToString()));
                query.Append(Queries.TenantTenantLegacyIdWhere);
            }

            if (data.Input.PolicyNames.Any())
            {
                parameters.Add("policyNames", data.Input.PolicyNames);
                query.Append(Queries.PolicyIdWhere);
            }

            query.Append(Queries.OptionalMatch);


            query.Append(Queries.ResultWithTenant);

            _provider = new RuntimeProvider(_driver.Object, mapService.Object);


            // Act
            var cypherResult = await _provider.GetSubjectEvaluation(data.Input);

            // Assert
            var expectedRoles = data.Output.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var actualRoles = cypherResult.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var expectedPermission = data.Output.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            var actualPermission = cypherResult.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            Assert.Equal(expectedRoles.Count, actualRoles.Count);
            Assert.Equal(expectedRoles, actualRoles);
            Assert.Equal(expectedPermission.Count, actualPermission.Count);
            Assert.Equal(expectedPermission, actualPermission);
            mapService.Verify(o => o.ConvertToRuntimeResultAsync(It.IsAny<IResultCursor>()), Times.Once);

            if (parameters.Any())
            {
                session.Verify(
                    o => o.RunAsync(It.Is<string>(m => m == query.ToString()),
                        It.Is<Dictionary<string, object>>(d => d.Keys.SequenceEqual(parameters.Keys))), Times.Once);
            }
            else
            {
                session.Verify(
                    o => o.RunAsync(It.Is<string>(m => m == query.ToString()), null), Times.Once);
            }
        }

        [Theory]
        [MemberData(nameof(Common.IntersectionData), MemberType = typeof(Common))]
        public async Task GetSubjectIntersection_Returns_Expected_Result(Common.IntersectionTestData data)
        {
            // Arrange
            var session = new Mock<IAsyncSession>(MockBehavior.Loose);
            var mapService = new Mock<ICursorToResultConverter>();
            mapService
                .Setup(o => o.ConvertToRuntimeResultAsync(It.IsAny<IResultCursor>()))
                .ReturnsAsync(data.Output);

            var cursor = new Mock<IResultCursor>();
            session.Setup(o => o.RunAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(cursor.Object);
            _driver.Setup(o => o.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(session.Object);

            var parameters = new Dictionary<string, object>();
            parameters.Add("subjectId", data.Input.SubjectId.ToString());
            parameters.Add("actorId", data.Input.ActorId);
            var query = new StringBuilder();
            query.Append(Queries.BaseIntersectionQuery()[0]);
            query.Append(Queries.InheritanceIntersectionQuery(data.Input.InheritanceEnabled)[0]);
            query.Append(Queries.BaseIntersectionQuery()[1]);
            query.Append(Queries.InheritanceIntersectionQuery(data.Input.InheritanceEnabled)[1]);
            if (data.Input.TenantIds.Any())
            {
                query.Append(Queries.TenantQuery(data.Input.InheritanceEnabled, data.Input.TenantType));
            }

            if (data.Input.PolicyNames.Any())
            {
                query.Append(Queries.PolicyQuery(data.Input.InheritanceEnabled));
            }


            if (data.Input.TenantIds.Any())
            {
                parameters.Add("tenantIds", data.Input.TenantIds.Select(x => x.ToString()));
                query.Append(Queries.TenantIdWhere);
            }

            if (data.Input.TenantLegacyIds.Any())
            {
                parameters.Add("tenantLegacyIds", data.Input.TenantLegacyIds.Select(x => x.ToString()));
                query.Append(Queries.TenantTenantLegacyIdWhere);
            }

            if (data.Input.PolicyNames.Any())
            {
                parameters.Add("policyNames", data.Input.PolicyNames);
                query.Append(Queries.PolicyIdWhere);
            }

            query.Append(Queries.OptionalMatch);


            query.Append(Queries.ResultWithTenant);

            _provider = new RuntimeProvider(_driver.Object, mapService.Object);


            // Act
            var cypherResult = await _provider.GetSubjectIntersection(data.Input);

            // Assert
            var expectedRoles = data.Output.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var actualRoles = cypherResult.SelectMany(p => p.Roles).OrderBy(i => i).ToList();
            var expectedPermission = data.Output.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            var actualPermission = cypherResult.SelectMany(p => p.Permissions).OrderBy(i => i).ToList();
            Assert.Equal(expectedRoles.Count, actualRoles.Count);
            Assert.Equal(expectedRoles, actualRoles);
            Assert.Equal(expectedPermission.Count, actualPermission.Count);
            Assert.Equal(expectedPermission, actualPermission);
            mapService.Verify(o => o.ConvertToRuntimeResultAsync(It.IsAny<IResultCursor>()), Times.Once);

            if (parameters.Any())
            {
                session.Verify(
                    o => o.RunAsync(It.Is<string>(m => m == query.ToString()),
                        It.Is<Dictionary<string, object>>(d => d.Keys.SequenceEqual(parameters.Keys))), Times.Once);
            }
            else
            {
                session.Verify(
                    o => o.RunAsync(It.Is<string>(m => m == query.ToString()), null), Times.Once);
            }
        }

        public static class Queries
        {
            public static Func<string> BaseQuery = () =>
                $"MATCH (s:Subject {{Id: $subjectId}}){Constants.MemberOfLink.ToCypher()}(g:Group){Constants.AssignedLink.ToCypher()}(r:Role),";

            public static Func<List<string>> BaseIntersectionQuery = () => new List<string>
            {
                $"MATCH (s:Subject {{Id: $actorId}}){Constants.MemberOfLink.ToCypher()}(g:Group){Constants.AssignedLink.ToCypher()}(r:Role)," +
                $"(s2:Subject{{Id: $subjectId}}){Constants.MemberOfLink.ToCypher()}(:Group){Constants.BelongsLink.ToCypher()}(t",
                ":Tenant) MATCH"
            };

            public static Func<bool, List<string>> InheritanceIntersectionQuery = (inheritanceEnabled) =>
                new List<string>
                {
                    inheritanceEnabled ? $"0:{Constants.Tenant}){Constants.ChildOfDepthIncomingLink.ToCypher()}(t" : "",
                    inheritanceEnabled
                        ? @$"(t0){Constants.ChildOfDepthLink.ToCypher()}(:{Constants.Tenant}){Constants.BelongsIncomingLink.ToCypher()}(:{Constants.Group}){Constants.MemberOfIncomingLink.ToCypher()}(s)
                MATCH "
                        : ""
                };

            public static Func<bool, string, string> TenantQuery = (inheritanceEnabled, typeTenant) =>
            {
                var inheritanceTenant = inheritanceEnabled ? $"{Constants.ChildOfDepthLink.ToCypher()}(:Tenant)" : "";
                return typeTenant == null
                    ? $"(t:Tenant){inheritanceTenant}{Constants.BelongsIncomingLink.ToCypher()}(g)"
                    : $"(t:Tenant:{typeTenant}){inheritanceTenant}{Constants.BelongsIncomingLink.ToCypher()}(g)";
            };

            public static Func<bool, string> PolicyQuery = (inheritanceEnabled) =>
            {
                var inheritancePolicy = inheritanceEnabled ? $"{Constants.ChildOfDepthLink.ToCypher()}(p0:Policy)" : "";
                return $",(p:Policy){inheritancePolicy}{Constants.ContainsLink.ToCypher()}(r)";
            };

            public static string TenantIdWhere = " WHERE t.Id in $tenantIds";

            public static string TenantTenantLegacyIdWhere = " AND t.LegacyId in $tenantLegacyIds";

            public static string PolicyIdWhere = " AND p.Name in $policyNames";

            public static string OptionalMatch =>
                $" OPTIONAL MATCH (r){Constants.ContainsLink.ToCypher()}(per:Permission)";

            public static string ResultWithTenant =
                " RETURN t.Id as TenantId, t.Name as TenantName, [x IN LABELS(t) WHERE NOT x = 'Tenant'][0] as TenantType, t.LegacyId as TenantLegacyId, collect(distinct r.Name) as Roles, collect(distinct per.Name) as Permissions";
        }
    }
}