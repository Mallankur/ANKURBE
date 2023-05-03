using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Adform.Bloom.Domain.Extensions
{
    public static class ClaimPrincipalExtensions
    {
        public const string AdformAdmin = "Adform Admin";
        public const string LocalAdmin = "Local Admin";

        public static ICollection<string> GetTenants(this ClaimsPrincipal principal,
            string type = Constants.Authentication.Bloom,
            IReadOnlyCollection<Guid>? limitTo = null)
        {
            var identity = principal.Identities.FirstOrDefault(o => o.AuthenticationType == type);

            if (identity is null)
            {
                return new string[0];
            }

            var isAdmin = principal.IsAdformAdmin();
            var tenants = isAdmin ? new List<string>() : identity.Claims.Select(x => x.Issuer).Distinct();

            if (limitTo != null && limitTo.Count > 0)
            {
                tenants = isAdmin ? limitTo.Select(o => o.ToString()) : tenants.Where(t => limitTo.Contains(Guid.Parse(t)));
            }

            return tenants.ToList();
        }

        public static bool IsAdformAdmin(this ClaimsPrincipal principal, string type = Constants.Authentication.Bloom)
        {
            var identity = principal.Identities.FirstOrDefault(o => o.AuthenticationType == type);
            return (identity?.Claims.Any(x => x.Type == "role" && x.Value == AdformAdmin)).GetValueOrDefault();
        }

        public static bool IsLocalAdmin(this ClaimsPrincipal principal, string type = Constants.Authentication.Bloom)
        {
            var identity = principal.Identities.FirstOrDefault(o => o.AuthenticationType == type);
            return (identity?.Claims.Any(x => x.Type == "role" && x.Value == LocalAdmin)).GetValueOrDefault();
        }
    }
}