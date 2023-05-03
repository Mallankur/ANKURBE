using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Adform.Bloom.Runtime.Integration.Test.Utils
{
    public static class Common
    {
        public static HttpContext BuildUser(string actorId)
        {
            var claims = new List<Claim>
            {
                new Claim(Ciam.SharedKernel.Core.Constants.ClaimType.ActorId, actorId)
            };

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
    }
}