using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Adform.Bloom.Api.Services
{
    public interface IClaimPrincipalGenerator
    {
        Task<ClaimsPrincipal> GenerateAsync(Guid subjectId, ClaimsPrincipal? actor = null, CancellationToken cancellationToken = default);
    }
}