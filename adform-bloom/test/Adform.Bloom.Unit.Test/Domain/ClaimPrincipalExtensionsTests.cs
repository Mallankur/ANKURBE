using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Adform.Bloom.Domain;
using Adform.Bloom.Domain.Extensions;
using IdentityModel;
using Xunit;

namespace Adform.Bloom.Unit.Test.Domain
{
    public class ClaimPrincipalExtensionsTests
    {
        [Theory]
        [InlineData("sub0", "tenant1")]
        [InlineData("sub1", "tenant2")]
        [InlineData("sub3", "tenant3")]
        public void GetTenants_Returns_Identity_TenantIds_When_AuthenticationType_Exist(string sub, string tenantId)
        {
            //Arrange
            var principals = BuildPrincipal(sub, tenantId);
            //Act
            var tenants = principals.GetTenants();
            //Assert
            Assert.Equal(1, tenants.Count);
            Assert.Contains(tenantId, tenants);
        }


        [Theory]
        [InlineData("sub0", "tenant1")]
        [InlineData("sub1", "tenant2")]
        [InlineData("sub3", "tenant3")]
        public void GetTenants_Returns_EmptyList_When_AuthenticationType_Doesnt_Exist(string sub, string tenantId)
        {
            //Arrange
            var principals = BuildPrincipal(sub, tenantId);
            //Act
            var tenants = principals.GetTenants(Guid.NewGuid().ToString());
            //Assert
            Assert.Equal(0, tenants.Count);
            Assert.Empty(tenants);
        }

        [Theory]
        [MemberData(nameof(GenerateTenantScenarios))]
        public void GetTenants_Returns_Limited_Identity_TenantIds_When_AuthenticationType_Exist(TenantScenario s)
        {
            //Arrange
            var principals = BuildPrincipal(s.Sub, s.PrincipalTenantIds, s.IsAdformAdmin);
            //Act
            var tenants = principals.GetTenants(limitTo: s.LimitedToTenantIds);
            //Assert
            Assert.Equal(s.ExpectedResult.Length, tenants.Count);
            Assert.True(tenants.SequenceEqual(s.ExpectedResult));
        }

        [Theory]
        [MemberData(nameof(Test))]
        public void IsAdformAdmin_Returns_True_For_Principal_With_Role_AdformAdmin(string subId,
            (string, string)[] tenantIdsRoles)
        {
            //Arrange
            var principal = BuildPrincipal(subId, tenantIdsRoles);
            //Act
            var isAdmin = principal.IsAdformAdmin();
            //Assert
            Assert.True(isAdmin);
        }

        public static ClaimsPrincipal BuildPrincipal(string sub, string tenantId, bool isAdmin = false)
        {
            return BuildPrincipal(sub, new[] { tenantId }, isAdmin);
        }

        public static ClaimsPrincipal BuildPrincipal(string sub, string[] tenantIds, bool isAdmin)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, sub)
            };
            var identity = new ClaimsIdentity(claims, "IdSvr");
            var id = new ClaimsIdentity(Constants.Authentication.Bloom, "name", "role");

            foreach (var t in tenantIds)
            {
                var roleClaims = new Claim("role", "DemoRole", "json", t);
                id.AddClaim(roleClaims);
            }

            if (isAdmin)
            {
                var roleClaims = new Claim("role", "Adform Admin", "json", "tenant1");
                id.AddClaim(roleClaims);
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            claimsPrincipal.AddIdentity(id);
            return claimsPrincipal;
        }

        public static ClaimsPrincipal BuildPrincipal(string sub, IEnumerable<(string, string)> tenantIdsRoles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, sub)
            };
            var identity = new ClaimsIdentity(claims, "IdSvr");
            var id = new ClaimsIdentity(Constants.Authentication.Bloom, "name", "role");

            foreach (var t in tenantIdsRoles)
            {
                var roleClaims = new Claim("role", t.Item2, "json", t.Item1);
                id.AddClaim(roleClaims);
            }

            var claimsPrincipal = new ClaimsPrincipal(identity);
            claimsPrincipal.AddIdentity(id);
            return claimsPrincipal;
        }

        public static TheoryData<TenantScenario> GenerateTenantScenarios()
        {
            var validTenant = Guid.NewGuid();
            var validTenant2 = Guid.NewGuid();
            var outOfScopeTenant = Guid.NewGuid();

            return new TheoryData<TenantScenario>
            {
                new TenantScenario
                {
                    Sub = Guid.NewGuid().ToString(),
                    IsAdformAdmin = false,
                    PrincipalTenantIds = new[]
                        {validTenant.ToString(), validTenant2.ToString(), outOfScopeTenant.ToString()},
                    LimitedToTenantIds = new[] {validTenant, validTenant2},
                    ExpectedResult = new[]
                        {validTenant.ToString(), validTenant2.ToString()},
                },
                new TenantScenario
                {
                    Sub = Guid.NewGuid().ToString(),
                    IsAdformAdmin = false,
                    PrincipalTenantIds = new[]
                        {validTenant.ToString(), validTenant2.ToString(), outOfScopeTenant.ToString()},
                    LimitedToTenantIds = null,
                    ExpectedResult = new[]
                        {validTenant.ToString(), validTenant2.ToString(), outOfScopeTenant.ToString()},
                },
                new TenantScenario
                {
                    Sub = Guid.NewGuid().ToString(),
                    IsAdformAdmin = true,
                    PrincipalTenantIds = new[]
                        {validTenant.ToString(), validTenant2.ToString(), outOfScopeTenant.ToString()},
                    LimitedToTenantIds = new[] {validTenant, validTenant2},
                    ExpectedResult = new[]
                        {validTenant.ToString(), validTenant2.ToString()},
                },
                new TenantScenario
                {
                    Sub = Guid.NewGuid().ToString(),
                    IsAdformAdmin = true,
                    PrincipalTenantIds = new[] {outOfScopeTenant.ToString()},
                    LimitedToTenantIds = new[] {validTenant, validTenant2},
                    ExpectedResult = new[]
                        {validTenant.ToString(), validTenant2.ToString()},
                }
            };
        }

        public class TenantScenario
        {
            public string Sub { get; set; }
            public bool IsAdformAdmin { get; set; }
            public string[] PrincipalTenantIds { get; set; }
            public Guid[] LimitedToTenantIds { get; set; }
            public string[] ExpectedResult { get; set; }
        }

        public static TheoryData<string, (string, string)[]> Test()
        {
            var data = new TheoryData<string, (string, string)[]>();

            data.Add("sub1", new[] { ("tenant1", "Adform Admin") });
            data.Add("sub1", new[] { ("tenant1", "Local Admin"), ("tenant2", "Adform Admin") });
            data.Add("sub1", new[] { ("tenant1", "Adform Admin"), ("tenant2", "Adform Admin") });

            return data;
        }
    }
}