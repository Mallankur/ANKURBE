using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GreenDonut;

namespace Adform.Bloom.Unit.Test
{
    public static class Common
    {
        internal static ClaimsPrincipal BuildActorPrincipal(Guid actorId)
        {
            var claims = new List<Claim>
            { 
                new Claim(ClaimTypes.NameIdentifier, actorId.ToString()),
            };
            var identity = new ClaimsIdentity(claims, "password");
            return new ClaimsPrincipal(identity);
        }
        internal static ClaimsPrincipal BuildPrincipal(string tenantId = "")
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(
                            ClaimTypes.NameIdentifier,
                            Guid.Empty.ToString(),
                            "json",
                            tenantId)
                    }, Adform.Bloom.Domain.Constants.Authentication.Bloom));
        }

        internal static ClaimsPrincipal BuildPrincipal(IEnumerable<Guid> tenantIds)
        {
            return new ClaimsPrincipal(
                new ClaimsIdentity(GenerateClaims(), Adform.Bloom.Domain.Constants.Authentication.Bloom));

            IEnumerable<Claim> GenerateClaims()
            {
                foreach (var tenantId in tenantIds)
                {
                    yield return new Claim(
                        ClaimTypes.NameIdentifier,
                        Guid.Empty.ToString(),
                        "json",
                        tenantId.ToString());
                }
            }
        }

        public class ManualBatchScheduler
            : IBatchScheduler
        {
            private readonly object _sync = new object();
            private readonly ConcurrentQueue<Func<ValueTask>> _queue =
                new ConcurrentQueue<Func<ValueTask>>();

            public void Dispatch()
            {
                lock (_sync)
                {
                    while (_queue.TryDequeue(out Func<ValueTask> dispatch))
                    {
                        dispatch();
                    }
                }
            }

            public void Schedule(Func<ValueTask> dispatch)
            {
                _queue.Enqueue(dispatch);
            }
        }
    }
}