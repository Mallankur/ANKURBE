using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Adform.Bloom.Client.Contracts;
using Adform.Bloom.Domain.Extensions;
using Adform.Bloom.Runtime.Contracts.Request;
using Adform.Bloom.Runtime.Contracts.Response;
using Adform.Bloom.Runtime.Contracts.Services;

namespace Adform.Bloom.Api.Services
{
    public class ClaimPrincipalGenerator : IClaimPrincipalGenerator
    {
        private readonly IBloomRuntimeClient _bloomRuntimeClient;

        public ClaimPrincipalGenerator(IBloomRuntimeClient bloomRuntimeClient)
        {
            _bloomRuntimeClient = bloomRuntimeClient;
        }

        public async Task<ClaimsPrincipal> GenerateAsync(Guid subjectId, ClaimsPrincipal? actor = null, CancellationToken cancellationToken = default)
        {
            var result = await _bloomRuntimeClient.InvokeAsync(new SubjectRuntimeRequest
            {
                SubjectId = subjectId
            }, cancellationToken: cancellationToken);
            if (actor != null && !actor.IsAdformAdmin())
            {
                var tenants = actor.GetTenants();
                result = result.Where(o => tenants.Contains(o.TenantId.ToString())).ToList();
            }

            var claim = new Claim(ClaimTypes.NameIdentifier, subjectId.ToString(), "json");
            var identity = new ClaimsIdentity();
            identity.AddClaim(claim);
            var user = new ClaimsPrincipal(identity);
            var bloomIdentity = EnhanceIdentity(result);
            user.AddIdentity(bloomIdentity);
            return user;
        }

        private ClaimsIdentity EnhanceIdentity(IEnumerable<RuntimeResponse> evaluationResult)
        {
            var id = new ClaimsIdentity(Authentication.Bloom, "name", "role");
            if (!evaluationResult.Any()) return id;

            var roleClaims = evaluationResult.SelectMany(x =>
                x.Roles.Select(y => new Claim(Claims.Role, y, "json", x.TenantId.ToString())));
            var permissionClaims = evaluationResult.SelectMany(x =>
                x.Permissions.Select(y => new Claim(Claims.Permission, y, "json", x.TenantId.ToString())));
            id.AddClaims(roleClaims);
            id.AddClaims(permissionClaims);
            return id;
        }
    }
}