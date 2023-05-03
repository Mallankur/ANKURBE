using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Adform.Bloom.Runtime.Contracts.Response;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Adform.Bloom.Common.Test
{
    public static class Common
    {
        public class Pagination<T>
        {
            public int TotalItems { get; set; }

            public int Offset { get; set; }

            public int Limit { get; set; }
        }

        public static List<Guid> ToGuids(this IEnumerable<string> ids)
        {
            return ids.Select(Guid.Parse).ToList();
        }
        
        public static ClaimsPrincipal BuildUser(RuntimeResponse[] result, string sub = Graph.SubjectUsedByBloomApi)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, sub, ClaimValueTypes.String, Guid.Empty.ToString())
            };
            var identity = new ClaimsIdentity(claims, "Bloom");

            var id = EnhanceIdentity(result, identity);

            var claimsPrincipal = new ClaimsPrincipal(identity);
            claimsPrincipal.AddIdentity(id);
            return claimsPrincipal;
        }

        public static ClaimsPrincipal BuildUser(RuntimeResponse result, string sub = Graph.SubjectUsedByBloomApi)
        {
            return BuildUser(new[] {result}, sub);
        }

        public static HttpContext BuildUser(string sub, string actorId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, sub)
            };

            if (!string.IsNullOrEmpty(actorId))
                claims.Add(new Claim(Ciam.SharedKernel.Core.Constants.ClaimType.ActorId, actorId));

            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.Identity.IsAuthenticated).Returns(true);
            mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.User).Returns(claimsPrincipal);
            return mockHttpContext.Object;
        }


        private static ClaimsIdentity EnhanceIdentity(IEnumerable<RuntimeResponse> evaluationResult,
            ClaimsIdentity identity = null)
        {
            if (evaluationResult is null) return null;

            var roleClaims = evaluationResult.SelectMany(x =>
                x.Roles.Select(y => new Claim("role", y, "json", x.TenantId.ToString())));
            var permissionClaims = evaluationResult.SelectMany(x =>
                x.Permissions.Select(y => new Claim("permission", y, "json", x.TenantId.ToString())));
            var id = identity ?? new ClaimsIdentity(Domain.Constants.Authentication.Bloom, "name", "role");
            id.AddClaims(roleClaims);
            id.AddClaims(permissionClaims);
            return id;
        }
    }
}